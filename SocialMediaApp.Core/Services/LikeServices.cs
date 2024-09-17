using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.Domain.IdentityEntites;
using SocialMediaApp.Core.DTO.LikeDTO;
using SocialMediaApp.Core.Hubs;
using SocialMediaApp.Core.IUnitOfWorkConfig;
using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Core.ServicesContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SocialMediaApp.Core.Services
{
    public class LikeServices : ILikeServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILikeRepository _likeRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LikeServices> _logger;
        private readonly IMapper _mapper;
        private readonly IHubContext<LikeHub> _hubContext;
        public LikeServices(
            IUnitOfWork unitOfWork,
            ILikeRepository likeRepository, 
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager, 
            ILogger<LikeServices> logger, IMapper mapper,
            IHubContext<LikeHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _likeRepository = likeRepository;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper;
            _hubContext = hubContext;
        }
        private string GetCurrentUserName()
        {
            var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userName))
                throw new UnauthorizedAccessException("User is not authenticated");

            return userName;
        }
        private async Task ExecuteWithTransactionAsync(Func<Task> action)
        {
            using(var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    await action();
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during the transaction");
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
        }
        public async Task<IEnumerable<LikeResponse>> GetAllAsync(Expression<Func<Like, bool>>? predicate, int pageIndex = 1, int pageSize = 10)
        {
            var likes = await _unitOfWork.Repository<Like>().GetAllAsync(predicate , includeProperties: "Profile,Profile.User",pageIndex:pageIndex,pageSize:pageSize);
            return _mapper.Map<IEnumerable<LikeResponse>>(likes);
        }

        public async Task<LikeResponse> GetByAsync(Expression<Func<Like,bool>> predicate , bool isTracked = true)
        {
            var like = await _unitOfWork.Repository<Like>().GetByAsync(predicate, includeProperties: "Profile,Profile.User", isTracked: isTracked);
            return _mapper.Map<LikeResponse>(like);
        }
        public async Task<LikeResponse> LikeAsync(Guid? id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var userName = GetCurrentUserName();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                throw new UnauthorizedAccessException("User is not authenticated");

            var profile = await _unitOfWork.Repository<Domain.Entites.Profile>()
                .GetByAsync(x => x.UserID == user.Id, includeProperties: "User");

            if (profile == null)
                throw new UnauthorizedAccessException("User profile not found");

            var tweet = await _unitOfWork.Repository<Tweet>().GetByAsync(x => x.TweetID == id);
            var comment = await _unitOfWork.Repository<Comment>().GetByAsync(x => x.CommentID == id);

            if (tweet == null && comment == null)
                throw new KeyNotFoundException("Tweet or Comment with the given ID was not found");

            bool isAlreadyLiked = tweet != null
                ? await _likeRepository.IsUserLikedToTweet(tweet.TweetID, profile.ProfileID)
                : await _likeRepository.IsUserLikedToComment(comment.CommentID, profile.ProfileID);

            if (isAlreadyLiked)
                throw new InvalidOperationException("User has already liked this item");

            var like = new Like
            {
                LikeID = Guid.NewGuid(),
                ProfileID = profile.ProfileID,
                Profile = profile,
                CreatedAt = DateTime.UtcNow,
            };

            await ExecuteWithTransactionAsync(async () =>
            {
                if (tweet != null)
                {
                    like.TweetID = tweet.TweetID;
                    tweet.TotalLikes++;
                }
                else
                {
                    like.CommentID = comment.CommentID;
                    comment.TotalLikes++;
                }

                await _likeRepository.LikeAsync(like);
                await _hubContext.Clients.All.SendAsync("UpdateLike", new { id = tweet?.TweetID ?? comment.CommentID, totalLikes = tweet?.TotalLikes ?? comment.TotalLikes });

            });

            return _mapper.Map<LikeResponse>(like);
        }

        public async Task<bool> UnLikeAsync(Guid? id)
        {
            if(id == null)
                throw new ArgumentNullException(nameof(id));
            var userName = GetCurrentUserName();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                throw new UnauthorizedAccessException("User is not authenticated");
            var profile = await _unitOfWork.Repository<Domain.Entites.Profile>()
                .GetByAsync(x => x.UserID == user.Id, includeProperties: "User");
            if (profile == null)
                throw new UnauthorizedAccessException("User profile not found");

            var like = await _unitOfWork.Repository<Like>().GetByAsync(x=>x.ProfileID== profile.ProfileID && (x.CommentID == id || x.TweetID == id),includeProperties: "Tweet,Comment");
            if(like == null)
                throw new KeyNotFoundException("Like with the given ID was not found");

            var result = false;
            await ExecuteWithTransactionAsync(async () =>
            {
                if(like.TweetID != null)
                {
                    var tweet = await _unitOfWork.Repository<Tweet>().GetByAsync(x => x.TweetID == like.TweetID);
                    if (tweet == null)
                        throw new KeyNotFoundException("Tweet with the given ID was not found");

                    tweet.TotalLikes--;
                }
                else
                {
                    var comment = await _unitOfWork.Repository<Comment>().GetByAsync(x => x.CommentID == like.CommentID);
                    if (comment == null)
                        throw new KeyNotFoundException("Comment with the given ID was not found");

                    comment.TotalLikes--;
                }
                result = await _likeRepository.UnLikeAsync(like.LikeID);

                await _hubContext.Clients.All.SendAsync("UpdateLike", new { id = like.Tweet?.TweetID ?? like.Comment.CommentID, totalLikes = like.Tweet?.TotalLikes ?? like.Comment.TotalLikes });
            });
            return result;
        }
        public async Task<bool> IsLikeToAsync(Guid? id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            var userName = GetCurrentUserName();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                throw new UnauthorizedAccessException("User is not authenticated");
            var profile = await _unitOfWork.Repository<Domain.Entites.Profile>()
                .GetByAsync(x => x.UserID == user.Id, includeProperties: "User");
            if (profile == null)
                throw new UnauthorizedAccessException("User profile not found");
            var like = await _unitOfWork.Repository<Like>().GetByAsync(x => x.ProfileID == profile.ProfileID && (x.CommentID == id || x.TweetID == id), includeProperties: "Tweet,Comment");
            return like != null;
        }
    }
}
