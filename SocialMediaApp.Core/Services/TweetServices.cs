using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.FilesTweetDTO;
using SocialMediaApp.Core.DTO.TweetDTO;
using SocialMediaApp.Core.Helper;
using SocialMediaApp.Core.IUnitOfWorkConfig;
using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Core.ServicesContract;
using System.Linq.Expressions;
using System.Security.Claims;


namespace SocialMediaApp.Core.Services
{
    public class TweetServices : ITweetServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITweetRepositroy _tweetRepositroy;
        private readonly IMapper _mapper;
        private readonly ILogger<TweetServices> _logger;
        private readonly ITweetFilesServices _tweetFilesServices;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TweetServices(IUnitOfWork unitOfWork,
            ITweetRepositroy tweetRepositroy,
            IMapper mapper,
            ILogger<TweetServices> logger,
            ITweetFilesServices tweetFilesServices,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _tweetRepositroy = tweetRepositroy;
            _mapper = mapper;
            _logger = logger;
            _tweetFilesServices = tweetFilesServices;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TweetResponse> CreateAsync(TweetAddRequest? tweetAddRequest)
        {
            if (tweetAddRequest == null)
            {
                throw new ArgumentNullException(nameof(tweetAddRequest));
            }

            ValidationHelper.ValidateModel(tweetAddRequest);

            var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userName))
                throw new UnauthorizedAccessException("User is not authenticated");

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var profileUser = await _unitOfWork.Repository<SocialMediaApp.Core.Domain.Entites.Profile>().GetByAsync(x => x.User.UserName == userName);
                    if (profileUser == null)
                        throw new InvalidOperationException("Profile not found");

                    var genre = await _unitOfWork.Repository<Genre>().GetByAsync(x => x.GenreID == tweetAddRequest.GenreID);
                    if (genre == null)
                        throw new InvalidOperationException("Genre not found");

                    var tweet = _mapper.Map<Tweet>(tweetAddRequest);
                    tweet.Profile = profileUser;
                    tweet.ProfileID = profileUser.ProfileID;
                    tweet.Genre = genre;
                    tweet.TweetID = Guid.NewGuid();
                    await _unitOfWork.Repository<Tweet>().CreateAsync(tweet);

                    if (tweetAddRequest.TweetFiles != null && tweetAddRequest.TweetFiles.Any())
                    {
                        var fileTweetAdd = new FileTweetAddRequest()
                        {
                            formFiles = tweetAddRequest.TweetFiles,
                            TweetID = tweet.TweetID
                        };
                        var files = await _tweetFilesServices.SaveTweetFileAsync(fileTweetAdd);
                        tweet.Files = files.ToList();
                    }

                    await _unitOfWork.CommitTransactionAsync();

                    var tweetResponse = _mapper.Map<TweetResponse>(tweet);
                    tweetResponse.UserName = userName;

                    return tweetResponse;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
        }


        public Task<TweetResponse> DeleteAsync(Guid? tweetID)
        {
            throw new NotImplementedException();
        }

        public Task<TweetResponse> GetAllAsync(Expression<Func<Tweet, bool>>? filter = null, string includeProperties = "", Expression<Func<Tweet, object>>? orderBy = null, int pageIndex = 1, int pageSize = 10)
        {
            throw new NotImplementedException();
        }

        public Task<TweetResponse> GetByAsync(Expression<Func<Tweet, bool>>? filter = null, bool isTracked = true, string includeProperties = "")
        {
            throw new NotImplementedException();
        }
    }
}
