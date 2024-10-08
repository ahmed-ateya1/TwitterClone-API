﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.Domain.IdentityEntites;
using SocialMediaApp.Core.DTO.CommentDTO;
using SocialMediaApp.Core.DTO.FilesCommentDTO;
using SocialMediaApp.Core.DTO.NotificationDTO;
using SocialMediaApp.Core.Enumeration;
using SocialMediaApp.Core.Helper;
using SocialMediaApp.Core.Hubs;
using SocialMediaApp.Core.IUnitOfWorkConfig;
using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Core.ServicesContract;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace SocialMediaApp.Core.Services
{
    public class CommentServices : ICommentServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommentRepository _commentRepository;
        private readonly ITweetRepositroy _tweetRepositroy;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentServices> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICommentFilesServices _commentFilesServices;
        private readonly IHubContext<CommentHub> _commentHubContext;
        private readonly IHubContext<NotificationHub> _notificationHubContext;

        public CommentServices(
            IUnitOfWork unitOfWork,
            ICommentRepository commentRepository,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            ILogger<CommentServices> logger,
            UserManager<ApplicationUser> userManager,
            ICommentFilesServices commentFilesServices,
            ITweetRepositroy tweetRepositroy,
            IHubContext<CommentHub> commentHubContext,
            IHubContext<NotificationHub> notificationHubContext )
        {
            _unitOfWork = unitOfWork;
            _commentRepository = commentRepository;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
            _commentFilesServices = commentFilesServices;
            _tweetRepositroy = tweetRepositroy;
            _commentHubContext = commentHubContext;
            _notificationHubContext = notificationHubContext;
        }

        private string GetCurrentUserName()
        {
            var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userName))
                throw new UnauthorizedAccessException("User is not authenticated");

            return userName;
        }
        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host.Value}/api/";
        }
        private async Task<SocialMediaApp.Core.Domain.Entites.Profile?> GetProfileIfAvailable()
        {
            var userName = GetCurrentUserName();
            if (userName == null) return null;

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return null;

            return await _unitOfWork.Repository<SocialMediaApp.Core.Domain.Entites.Profile>().GetByAsync(x => x.User.Id == user.Id);
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

        private async Task SetLikeStatusAsync(IEnumerable<CommentResponse> comments, Guid profileId)
        {
            var commentIds = comments.Select(t => t.CommentID).ToList();
            var likedComments = await _unitOfWork.Repository<Comment>()
                .GetAllAsync(l => commentIds.Contains(l.CommentID) && l.ProfileID == profileId);

            foreach (var comment in comments)
            {
                comment.IsLiked = comments.Any(l => l.CommentID == comment.CommentID);
            }
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

        private async Task<List<CommentFiles>> HandleCommentFilesAsync(IEnumerable<IFormFile>? formFiles, Guid commentId)
        {
            if (formFiles != null && formFiles.Any())
            {
                var filesCommentAdd = new FilesCommentAddRequest()
                {
                    formFiles = formFiles.ToList(),
                    CommentID = commentId
                };
                return (await _commentFilesServices.SaveTweetFileAsync(filesCommentAdd)).ToList();
            }
            return new List<CommentFiles>();
        }

        public async Task<CommentResponse> CreateAsync(CommentAddRequest? commentAddRequest)
        {
            if (commentAddRequest == null)
            {
                throw new ArgumentNullException(nameof(commentAddRequest));
            }

            ValidationHelper.ValidateModel(commentAddRequest);

            var tweet = await _unitOfWork.Repository<Tweet>().GetByAsync(
                x => x.TweetID == commentAddRequest.TweetID,
                includeProperties: "Profile,Profile.User,Files"
            );
            if (tweet == null)
            {
                throw new InvalidOperationException("Tweet not found.");
            }

            var profile = await GetProfileIfAvailable();
            if(profile == null)
            {
                throw new InvalidOperationException("Profile not found.");
            }
            Comment comment = null;

            await ExecuteWithTransaction(async () =>
            {
                comment = _mapper.Map<Comment>(commentAddRequest);
                comment.CommentID = Guid.NewGuid();
                comment.ProfileID = profile.ProfileID;
                comment.Profile = profile;
                comment.CreatedAt = DateTime.UtcNow;
                comment.UpdatedAt = DateTime.UtcNow;

                if (commentAddRequest.ParentID != null)
                {
                    var parentComment = await _unitOfWork.Repository<Comment>()
                        .GetByAsync(x => x.CommentID == commentAddRequest.ParentID, includeProperties: "Profile,Profile.User,Tweet,Files,Replies");

                    if (parentComment == null)
                    {
                        throw new InvalidOperationException("Parent comment not found.");
                    }

                    comment.ParentComment = parentComment;
                    comment.ParentCommentID = parentComment.CommentID;

                    parentComment.TotalComment++;
                    await _commentHubContext.Clients.All.SendAsync("ReceiveTotalCommentUpdate", parentComment.CommentID, parentComment.TotalComment);

                    await _commentRepository.UpdateAsync(parentComment);
                }

                tweet.TotalComments++;
                await _tweetRepositroy.UpdateAsync(tweet); 

                await _unitOfWork.Repository<Comment>().CreateAsync(comment);

                if (commentAddRequest.FormFiles != null && commentAddRequest.FormFiles.Any())
                {
                    comment.Files = await HandleCommentFilesAsync(commentAddRequest.FormFiles, comment.CommentID);
                }

                await _unitOfWork.CompleteAsync();
            });

            var commentResponse = _mapper.Map<CommentResponse>(comment);

            if (comment != null && comment.Replies.Any())
            {
                commentResponse.Replies.AddRange(_mapper.Map<List<CommentResponse>>(comment.Replies));
            }
            await _commentHubContext.Clients.All.SendAsync("ReceiveTotalCommentUpdate", tweet.TweetID, tweet.TotalComments);

            var notificationAddRequest = new NotificationAddRequest
            {
                Message = $"{profile.User.UserName} commented on your tweet: {tweet.Content}",
                NotificationType = NotificationType.COMMENT,
                ReferenceURL = $"{GetBaseUrl()}Tweet/getTweet/{tweet.TweetID}",
                ProfileID = tweet.ProfileID
            };
            await SetNotification(notificationAddRequest);
            return commentResponse;
        }

        public async Task<bool> DeleteAsync(Guid? commentID)
        {
            var result = false;

            await ExecuteWithTransaction(async () =>
            {
                var comment = await _unitOfWork.Repository<Comment>().GetByAsync(x => x.CommentID == commentID, includeProperties: "Likes,Tweet");
                if (comment == null)
                {
                    throw new InvalidOperationException("Comment not found.");
                }

                var replies = await _unitOfWork.Repository<Comment>().GetAllAsync(c => c.ParentCommentID == comment.CommentID);

                if (replies.Any())
                {
                    await _unitOfWork.Repository<Comment>().RemoveRangeAsync(replies);
                }

                if (comment.Files != null && comment.Files.Any())
                {
                    await _commentFilesServices.DeleteTweetFileAsync(comment.Files);
                }
                if (comment.Likes.Any())
                {
                    await _unitOfWork.Repository<Like>().RemoveRangeAsync(comment.Likes);
                }
                if (comment.Tweet != null)
                {
                    comment.Tweet.TotalComments--;
                    await _tweetRepositroy.UpdateAsync(comment.Tweet); 
                }

                result = await _unitOfWork.Repository<Comment>().DeleteAsync(comment);

                if (result)
                {
                    _logger.LogInformation("Comment deleted successfully: {commentID}", commentID);

                    if (comment.Tweet != null)
                    {
                        await _commentHubContext.Clients.All.SendAsync("ReceiveTotalCommentUpdate", comment.Tweet.TweetID, comment.Tweet.TotalComments);
                    }
                }
                else
                {
                    _logger.LogError("Error deleting comment: {commentID}", commentID);
                }
            });

            return result;
        }


        public async Task<IEnumerable<CommentResponse>> GetAllAsync(Expression<Func<Comment, bool>>? predict, int pageIndex = 1, int pageSize = 5)
        {
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 5 : pageSize;
            var comments = await _unitOfWork.Repository<Comment>().GetAllAsync(predict, includeProperties: "Profile,Profile.User,Tweet,Files,Replies", null, pageIndex, pageSize);
            var result = _mapper.Map<IEnumerable<CommentResponse>>(comments);
            var profile = await GetProfileIfAvailable();
            if(profile!=null)
            {
                await SetLikeStatusAsync(result, profile.ProfileID);
            }
            return result;
        }

        public async Task<CommentResponse> GetByAsync(Expression<Func<Comment, bool>> predict, bool isTracked = true)
        {
            var comment = await _unitOfWork.Repository<Comment>().GetByAsync(predict, isTracked, includeProperties: "Profile,Profile.User,Tweet,Files,Replies");
            var result = _mapper.Map<CommentResponse>(comment);
            var profile = await GetProfileIfAvailable();
            if(profile != null)
            {
                var like = await _unitOfWork.Repository<Like>().GetByAsync(x => x.CommentID == comment.CommentID && x.ProfileID == profile.ProfileID);
                result.IsLiked = like != null;
            }
            return result;
        }

        public async Task<CommentResponse> UpdateAsync(CommentUpdateRequest? commentUpdateRequest)
        {
            if (commentUpdateRequest == null)
            {
                throw new ArgumentNullException(nameof(commentUpdateRequest));
            }

            ValidationHelper.ValidateModel(commentUpdateRequest);
            var userName = GetCurrentUserName();
            if (string.IsNullOrEmpty(userName))
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            Comment comment = null;

            await ExecuteWithTransaction(async () =>
            {
                var user = await _userManager.FindByNameAsync(userName);
                var profileUser = await _unitOfWork.Repository<Domain.Entites.Profile>()
                    .GetByAsync(x => x.UserID == user.Id, isTracked: true, includeProperties: "User");

                comment = await _unitOfWork.Repository<Comment>().GetByAsync(x => x.CommentID == commentUpdateRequest.CommentId, isTracked: true, includeProperties: "Profile,Profile.User,Files");
                if (comment == null)
                {
                    throw new InvalidOperationException("Comment not found.");
                }

                if (commentUpdateRequest.FormFiles != null && commentUpdateRequest.FormFiles.Any())
                {
                    if (comment.Files != null && comment.Files.Any())
                    {
                        await _commentFilesServices.DeleteTweetFileAsync(comment.Files);
                    }
                    comment.Files = await HandleCommentFilesAsync(commentUpdateRequest.FormFiles, comment.CommentID);
                }

                _mapper.Map(commentUpdateRequest, comment);
                await _commentRepository.UpdateAsync(comment);
            });

            var commentResponse = _mapper.Map<CommentResponse>(comment);
            return commentResponse;
        }
    }
}
