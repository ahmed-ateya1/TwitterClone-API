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

    private async Task<SocialMediaApp.Core.Domain.Entites.Profile> GetCurrentProfileAsync()
    {
        var userName = GetCurrentUserName();
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
            throw new UnauthorizedAccessException("User is not authenticated");

        var profile = await _unitOfWork.Repository<SocialMediaApp.Core.Domain.Entites.Profile>().GetByAsync(x => x.User.Id == user.Id,includeProperties:"User")
            ?? throw new InvalidOperationException("Profile not found");

        return profile;
    }
    private async Task<SocialMediaApp.Core.Domain.Entites.Profile> CheckProfile()
    {
        var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!String.IsNullOrEmpty(userName))
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                var profile = await _unitOfWork.Repository<SocialMediaApp.Core.Domain.Entites.Profile>().GetByAsync(x => x.User.Id == user.Id);
                return profile;
            }
        }
        return null;
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
            var fileTweetAdd = new FileTweetAddRequest()
            {
                formFiles = formFiles.ToList(),
                TweetID = tweetId
            };
            return (await _tweetFilesServices.SaveTweetFileAsync(fileTweetAdd)).ToList();
        }
        return new List<TweetFiles>();
    }

    private async Task SetLikeStatusAsync(IEnumerable<TweetResponse> tweets, Guid profileId)
    {
        var tweetIds = tweets.Select(t => t.TweetID).ToList();
        var likedTweets = await _unitOfWork.Repository<Like>()
            .GetAllAsync(l => tweetIds.Contains((Guid)l.TweetID) && l.ProfileID == profileId);

        foreach (var tweet in tweets)
        {
            tweet.IsLiked = likedTweets.Any(l => l.TweetID == tweet.TweetID);
        }
    }

    public async Task<TweetResponse> CreateAsync(TweetAddRequest? tweetAddRequest)
    {
        if (tweetAddRequest == null) throw new ArgumentNullException(nameof(tweetAddRequest));
        ValidationHelper.ValidateModel(tweetAddRequest);

        var userName = GetCurrentUserName();
        Tweet tweet = null;

        await ExecuteWithTransaction(async () =>
        {
            var profileUser = await GetCurrentProfileAsync();

            var genre = await _unitOfWork.Repository<Genre>()
                .GetByAsync(x => x.GenreID == tweetAddRequest.GenreID)
                ?? throw new InvalidOperationException("Genre not found");

            tweet = _mapper.Map<Tweet>(tweetAddRequest);
            tweet.TweetID = Guid.NewGuid();
            tweet.ProfileID = profileUser.ProfileID;
            tweet.Profile = profileUser;
            tweet.Genre = genre;
            tweet.IsUpdated = false;
            await _unitOfWork.Repository<Tweet>().CreateAsync(tweet);
            tweet.Files = await HandleTweetFilesAsync(tweetAddRequest.TweetFiles, tweet.TweetID);
        });
        return _mapper.Map<TweetResponse>(tweet);
    }

    public async Task<bool> DeleteAsync(Guid? tweetID)
    {
        var tweet = await _unitOfWork.Repository<Tweet>().GetByAsync(x => x.TweetID == tweetID, isTracked: true, includeProperties: "Profile,Profile.User,Genre,Files,Comments,Likes")
                         ?? throw new InvalidOperationException("Tweet not found");

        await ExecuteWithTransaction(async () =>
        {
            if (tweet.Comments.Any())
            {
                await _unitOfWork.Repository<Comment>().RemoveRangeAsync(tweet.Comments);
            }
            if (tweet.Likes.Any())
            {
                await _unitOfWork.Repository<Like>().RemoveRangeAsync(tweet.Likes);
            }
            await _unitOfWork.Repository<Tweet>().DeleteAsync(tweet);
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
            "Profile,Profile.User,Genre,Files",
            orderBy,
            pageIndex,
            pageSize);

        var tweetResponses = _mapper.Map<IEnumerable<TweetResponse>>(tweets);

        var profile = await CheckProfile();
        if(profile != null)
        {
            await SetLikeStatusAsync(tweetResponses, profile.ProfileID);

        }
        return tweetResponses;
    }

    public async Task<TweetResponse> GetByAsync(Expression<Func<Tweet, bool>>? filter = null, bool isTracked = true)
    {
        var tweet = await _unitOfWork.Repository<Tweet>()
            .GetByAsync(filter, isTracked, "Profile,Profile.User,Genre,Files,Comments")
            ?? throw new InvalidOperationException("Tweet not found");

        var result = _mapper.Map<TweetResponse>(tweet);

        var profile = await CheckProfile();
        if (profile != null)
        {
            var like = await _unitOfWork.Repository<Like>().GetByAsync(x => x.TweetID == tweet.TweetID && x.ProfileID == profile.ProfileID);
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
                .GetByAsync(x => x.TweetID == tweetUpdateRequest.TweetID, true, "Profile,Profile.User,Genre,Files,Comments")
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
