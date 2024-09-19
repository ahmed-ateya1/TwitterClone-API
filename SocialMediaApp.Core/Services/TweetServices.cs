using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.Domain.IdentityEntites;
using SocialMediaApp.Core.DTO.FilesTweetDTO;
using SocialMediaApp.Core.DTO.TweetDTO;
using SocialMediaApp.Core.Helper;
using SocialMediaApp.Core.IUnitOfWorkConfig;
using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Core.ServicesContract;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class TweetServices : ITweetServices
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITweetRepositroy _tweetRepositroy;
    private readonly IMapper _mapper;
    private readonly ILogger<TweetServices> _logger;
    private readonly ITweetFilesServices _tweetFilesServices;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public TweetServices(
        IUnitOfWork unitOfWork,
        ITweetRepositroy tweetRepositroy,
        IMapper mapper,
        ILogger<TweetServices> logger,
        ITweetFilesServices tweetFilesServices,
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _tweetRepositroy = tweetRepositroy;
        _mapper = mapper;
        _logger = logger;
        _tweetFilesServices = tweetFilesServices;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    private string GetCurrentUserName()
    {
        var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userName))
            throw new UnauthorizedAccessException("User is not authenticated");

        return userName;
    }

    private async Task<SocialMediaApp.Core.Domain.Entites.Profile?> GetProfileIfAvailable()
    {
        var userName = GetCurrentUserName();
        if (userName == null) return null;

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null) return null;

        return await _unitOfWork.Repository<SocialMediaApp.Core.Domain.Entites.Profile>().GetByAsync(x => x.User.Id == user.Id);
    }

    private async Task ExecuteWithTransaction(Func<Task> action)
    {
        using (var transaction = await _unitOfWork.BeginTransactionAsync())
        {
            try
            {
                await action();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }

    private async Task<List<TweetFiles>> HandleTweetFilesAsync(IEnumerable<IFormFile>? formFiles, Guid tweetId)
    {
        if (formFiles != null && formFiles.Any())
        {
            try
            {
                var fileTweetAdd = new FileTweetAddRequest()
                {
                    formFiles = formFiles.ToList(),
                    TweetID = tweetId
                };
                return (await _tweetFilesServices.SaveTweetFileAsync(fileTweetAdd)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving tweet files");
                throw;
            }
        }
        return new List<TweetFiles>();
    }

    private async Task SetLikeAndRetweetStatusAsync(IEnumerable<TweetResponse> tweets, Guid profileId)
    {
        var tweetIds = tweets.Select(t => t.TweetID).ToList();
        var likedTweets = await _unitOfWork.Repository<Like>()
            .GetAllAsync(l => tweetIds.Contains((Guid)l.TweetID) && l.ProfileID == profileId);

        foreach (var tweet in tweets)
        {
            tweet.IsLiked = likedTweets.Any(l => l.TweetID == tweet.TweetID);
            var retweet = await _unitOfWork.Repository<Tweet>()
              .GetByAsync(x => x.ParentTweetID == tweet.TweetID && x.ProfileID == profileId);
            if (retweet != null)
            {
                tweet.IsRetweeted = true;
            }
        }
    }

    public async Task<TweetResponse> CreateAsync(TweetAddRequest? tweetAddRequest)
    {
        if (tweetAddRequest == null) throw new ArgumentNullException(nameof(tweetAddRequest));
        ValidationHelper.ValidateModel(tweetAddRequest);

        var profile = await GetProfileIfAvailable()
                      ?? throw new InvalidOperationException("Profile not found");

        Tweet tweet = null;

        await ExecuteWithTransaction(async () =>
        {
            tweet = _mapper.Map<Tweet>(tweetAddRequest);
            if (tweetAddRequest.ParentTweetID != null)
            {
                var parentTweet = await _unitOfWork.Repository<Tweet>()
                    .GetByAsync(x => x.TweetID == tweetAddRequest.ParentTweetID, isTracked: true)
                    ?? throw new InvalidOperationException("Parent tweet not found");

                tweet.ParentTweet = parentTweet;
                tweet.ParentTweetID = parentTweet.TweetID;
                tweet.ParentTweet.TotalRetweets++;
                await _tweetRepositroy.UpdateAsync(parentTweet);
            }
            tweet.TweetID = Guid.NewGuid();
            tweet.ProfileID = profile.ProfileID;
            tweet.Profile = profile;
            if (tweetAddRequest.GenreID != null)
            {
                var genre = await _unitOfWork.Repository<Genre>().GetByAsync(x => x.GenreID == tweetAddRequest.GenreID)
                    ?? throw new InvalidOperationException("Genre not found");
                tweet.GenreID = genre.GenreID;
                tweet.Genre = genre;
            }
            tweet.IsUpdated = false;
            await _unitOfWork.Repository<Tweet>().CreateAsync(tweet);
            tweet.Files = await HandleTweetFilesAsync(tweetAddRequest.TweetFiles, tweet.TweetID);
        });
        var response =  _mapper.Map<TweetResponse>(tweet);
        if(tweet!=null && tweet.ParentTweet != null)
        {
            response.ParentTweet = _mapper.Map<TweetResponse>(tweet.ParentTweet);
        }
        return response;
    }

    public async Task<bool> DeleteAsync(Guid? tweetID)
    {
        var tweet = await _unitOfWork.Repository<Tweet>()
            .GetByAsync(x => x.TweetID == tweetID, isTracked: true, includeProperties: "Profile,Profile.User,Genre,Files,Comments,Likes,Retweets")
            ?? throw new InvalidOperationException("Tweet not found");

        await ExecuteWithTransaction(async () =>
        {
            var childTweets = await _unitOfWork.Repository<Tweet>().GetAllAsync(x => x.ParentTweetID == tweetID);
            foreach (var childTweet in childTweets)
            {
                childTweet.ParentTweetID = null;
                await _tweetRepositroy.UpdateAsync(childTweet);
            }

            var tasks = new List<Task>();

            if (tweet.Retweets.Any())
                tasks.Add(_unitOfWork.Repository<Tweet>().RemoveRangeAsync(tweet.Retweets));

            if (tweet.Comments.Any())
                tasks.Add(_unitOfWork.Repository<Comment>().RemoveRangeAsync(tweet.Comments));

            if (tweet.Likes.Any())
                tasks.Add(_unitOfWork.Repository<Like>().RemoveRangeAsync(tweet.Likes));

            tasks.Add(_unitOfWork.Repository<Tweet>().DeleteAsync(tweet));

            await Task.WhenAll(tasks);
        });

        return true;
    }


    public async Task<IEnumerable<TweetResponse>> GetAllAsync(
        Expression<Func<Tweet, bool>>? filter = null,
        Expression<Func<Tweet, object>>? orderBy = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var tweets = await _unitOfWork.Repository<Tweet>().GetAllAsync(
            filter,
            "Profile,Profile.User,Genre,Files,ParentTweet,Retweets",
            orderBy,
            pageIndex,
            pageSize);

        var tweetResponses = _mapper.Map<IEnumerable<TweetResponse>>(tweets);
        var profile = await GetProfileIfAvailable();
        if (tweetResponses.Any()&&profile != null)
        {
            await SetLikeAndRetweetStatusAsync(tweetResponses, profile.ProfileID);
        }
        return tweetResponses;
    }

    public async Task<TweetResponse> GetByAsync(Expression<Func<Tweet, bool>>? filter = null, bool isTracked = true)
    {
        var tweet = await _unitOfWork.Repository<Tweet>()
            .GetByAsync(filter, isTracked, "Profile,Profile.User,Genre,Files,ParentTweet,Comments")
            ?? throw new InvalidOperationException("Tweet not found");

        var result = _mapper.Map<TweetResponse>(tweet);

        var profile = await GetProfileIfAvailable();
        if (profile != null)
        {
            var like = await _unitOfWork.Repository<Like>().GetByAsync(x => x.TweetID == tweet.TweetID && x.ProfileID == profile.ProfileID);
            var retweet = await _unitOfWork.Repository<Tweet>().GetByAsync(x => x.ParentTweetID == tweet.TweetID && x.ProfileID == profile.ProfileID);

            result.IsRetweeted = retweet != null;
            result.IsLiked = like != null;
        }

        return result;
    }


    public async Task<TweetResponse> UpdateAsync(TweetUpdateRequest? tweetUpdateRequest)
    {
        if (tweetUpdateRequest == null) throw new ArgumentNullException(nameof(tweetUpdateRequest));
        ValidationHelper.ValidateModel(tweetUpdateRequest);

        Tweet tweet = null;
        await ExecuteWithTransaction(async () =>
        {
            tweet = await _unitOfWork.Repository<Tweet>()
                .GetByAsync(x => x.TweetID == tweetUpdateRequest.TweetID, true, "Profile,Profile.User,Genre,Files,Comments,ParentTweet")
                ?? throw new InvalidOperationException("Tweet not found");

            if (tweetUpdateRequest.TweetFiles != null && tweetUpdateRequest.TweetFiles.Any())
            {
                if (tweet.Files != null && tweet.Files.Any())
                    await _tweetFilesServices.DeleteTweetFileAsync(tweet.Files);

                tweet.Files = await HandleTweetFilesAsync(tweetUpdateRequest.TweetFiles, tweet.TweetID);
            }

            _mapper.Map(tweetUpdateRequest, tweet);
            await _tweetRepositroy.UpdateAsync(tweet);
        });

        return _mapper.Map<TweetResponse>(tweet);
    }
}