using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.Domain.IdentityEntites;
using SocialMediaApp.Core.DTO.LikeDTO;
using SocialMediaApp.Core.DTO.NotificationDTO;
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
        private readonly IHubContext<NotificationHub> _notificationHubContext;

        public LikeServices(
            IUnitOfWork unitOfWork,
            ILikeRepository likeRepository,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            ILogger<LikeServices> logger,
            IMapper mapper,
            IHubContext<LikeHub> hubContext,
            IHubContext<NotificationHub> notificationHubContext)
        {
            _unitOfWork = unitOfWork;
            _likeRepository = likeRepository;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper;
            _hubContext = hubContext;
            _notificationHubContext = notificationHubContext;
        }

        private string GetCurrentUserName()
        {
            var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("User is not authenticated.");
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            return userName;
        }

        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host.Value}/api/";
        }

        private async Task<Domain.Entites.Profile?> GetProfileIfAvailable()
        {
            var userName = GetCurrentUserName();
            if (string.IsNullOrEmpty(userName))
            {
                _logger.LogWarning("Failed to retrieve profile: User name is null or empty.");
                return null;
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                _logger.LogWarning("Failed to retrieve profile: User not found.");
                return null;
            }

            return await _unitOfWork.Repository<Domain.Entites.Profile>().GetByAsync(x => x.UserID == user.Id, includeProperties: "User");
        }

        private async Task SetNotification(NotificationAddRequest notificationAddRequest)
        {
            var notification = _mapper.Map<Notification>(notificationAddRequest);
            notification.NotificationID = Guid.NewGuid();
            notification.Profile = await GetProfileIfAvailable();
            await _unitOfWork.Repository<Notification>().CreateAsync(notification);

            var notificationResponse = _mapper.Map<NotificationResponse>(notification);

            if (_notificationHubContext.Clients != null)
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.ProfileID == notificationAddRequest.ProfileID);
                if (user != null)
                {
                    await _notificationHubContext.Clients.User(user.Id.ToString())
                        .SendAsync("ReceiveNotification", notificationResponse);
                }
            }
        }

        private async Task ExecuteWithTransactionAsync(Func<Task> action)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    await action();
                    await _unitOfWork.CommitTransactionAsync();
                    _logger.LogInformation("Transaction committed successfully.");
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
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            _logger.LogInformation("Fetching likes with predicate: {Predicate}", predicate);
            var likes = await _unitOfWork.Repository<Like>().GetAllAsync(predicate, includeProperties: "Profile,Profile.User", pageIndex: pageIndex, pageSize: pageSize);
            return _mapper.Map<IEnumerable<LikeResponse>>(likes);
        }

        public async Task<LikeResponse> GetByAsync(Expression<Func<Like, bool>> predicate, bool isTracked = true)
        {
            _logger.LogInformation("Fetching like with predicate: {Predicate}", predicate);
            var like = await _unitOfWork.Repository<Like>().GetByAsync(predicate, includeProperties: "Profile,Profile.User", isTracked: isTracked);
            return _mapper.Map<LikeResponse>(like);
        }

        public async Task<LikeResponse> LikeAsync(Guid? id)
        {
            if (id == null)
            {
                _logger.LogWarning("LikeAsync called with null ID.");
                throw new ArgumentNullException(nameof(id));
            }

            var userName = GetCurrentUserName();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                _logger.LogWarning("User not found while attempting to like item.");
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            var profile = await _unitOfWork.Repository<Domain.Entites.Profile>().GetByAsync(x => x.UserID == user.Id, includeProperties: "User");
            if (profile == null)
            {
                _logger.LogWarning("Profile not found for user: {UserName}", userName);
                throw new UnauthorizedAccessException("User profile not found");
            }

            var tweet = await _unitOfWork.Repository<Tweet>().GetByAsync(x => x.TweetID == id);
            var comment = await _unitOfWork.Repository<Comment>().GetByAsync(x => x.CommentID == id);

            if (tweet == null && comment == null)
            {
                _logger.LogWarning("Tweet or Comment not found with ID: {Id}", id);
                throw new KeyNotFoundException("Tweet or Comment with the given ID was not found");
            }

            bool isAlreadyLiked = tweet != null
                ? await _likeRepository.IsUserLikedToTweet(tweet.TweetID, profile.ProfileID)
                : await _likeRepository.IsUserLikedToComment(comment.CommentID, profile.ProfileID);

            if (isAlreadyLiked)
            {
                _logger.LogWarning("User {UserName} has already liked the item with ID: {Id}", userName, id);
                throw new InvalidOperationException("User has already liked this item");
            }

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
                _logger.LogInformation("Like added successfully for item ID: {Id}", id);

                await _hubContext.Clients.All.SendAsync("UpdateLike", new { id = tweet?.TweetID ?? comment.CommentID, totalLikes = tweet?.TotalLikes ?? comment.TotalLikes });
            });

            var notificationMessage = $"{profile.User.UserName} liked your {tweet?.Content ?? comment?.Content ?? "content"}";
            var notificationAdd = new NotificationAddRequest
            {
                Message = notificationMessage,
                NotificationType = Enumeration.NotificationType.LIKE,
                ProfileID = tweet?.ProfileID ?? comment.ProfileID,
                ReferenceURL = tweet != null
                    ? $"{GetBaseUrl()}Tweet/getTweet/{tweet.TweetID}"
                    : $"{GetBaseUrl()}Comment/getComment/{comment.CommentID}"
            };

            await SetNotification(notificationAdd);

            return _mapper.Map<LikeResponse>(like);
        }

        public async Task<bool> UnLikeAsync(Guid? id)
        {
            if (id == null)
            {
                _logger.LogWarning("UnLikeAsync called with null ID.");
                throw new ArgumentNullException(nameof(id));
            }

            var userName = GetCurrentUserName();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                _logger.LogWarning("User not found while attempting to unlike item.");
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            var profile = await _unitOfWork.Repository<Domain.Entites.Profile>().GetByAsync(x => x.UserID == user.Id, includeProperties: "User");
            if (profile == null)
            {
                _logger.LogWarning("Profile not found for user: {UserName}", userName);
                throw new UnauthorizedAccessException("User profile not found");
            }

            var like = await _unitOfWork.Repository<Like>().GetByAsync(x => x.LikeID == id);
            if (like == null)
            {
                _logger.LogWarning("Like not found with ID: {Id}", id);
                throw new KeyNotFoundException("Like with the given ID was not found");
            }

            var tweet = await _unitOfWork.Repository<Tweet>().GetByAsync(x => x.TweetID == like.TweetID);
            var comment = await _unitOfWork.Repository<Comment>().GetByAsync(x => x.CommentID == like.CommentID);

            if (tweet == null && comment == null)
            {
                _logger.LogWarning("Associated Tweet or Comment not found for Like with ID: {Id}", id);
                throw new KeyNotFoundException("Associated Tweet or Comment with the given ID was not found");
            }

            await ExecuteWithTransactionAsync(async () =>
            {
                if (tweet != null)
                {
                    tweet.TotalLikes--;
                }
                else
                {
                    comment.TotalLikes--;
                }

                await _unitOfWork.Repository<Like>().DeleteAsync(like);
                _logger.LogInformation("Like removed successfully for item ID: {Id}", id);

                await _hubContext.Clients.All.SendAsync("UpdateLike", new { id = tweet?.TweetID ?? comment.CommentID, totalLikes = tweet?.TotalLikes ?? comment.TotalLikes });
            });

            return true;
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
