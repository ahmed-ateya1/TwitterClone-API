﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.Domain.IdentityEntites;
using SocialMediaApp.Core.DTO.NotificationDTO;
using SocialMediaApp.Core.DTO.UserConnectionsDTO;
using SocialMediaApp.Core.Hubs;
using SocialMediaApp.Core.IUnitOfWorkConfig;
using SocialMediaApp.Core.ServicesContract;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SocialMediaApp.Core.Services
{
    public class UserConnectionsServices : IUserConnectionsServices
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserConnectionsServices> _logger;
        private readonly IMapper _mapper;
        private readonly IHubContext<UserConnectionHub> _hubContext;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public UserConnectionsServices(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, ILogger<UserConnectionsServices> logger, IMapper mapper, IHubContext<UserConnectionHub> hubContext, IHubContext<NotificationHub> notificationHub = null)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _hubContext = hubContext;
            _notificationHub = notificationHub;
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

            if (_notificationHub.Clients != null)
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.ProfileID == notificationAddRequest.ProfileID);
                if (user != null)
                {
                    await _notificationHub.Clients.User(user.Id.ToString())
                        .SendAsync("ReceiveNotification", notificationResponse);
                }
            }
        }
        public async Task<FollowResponse> FollowAsync(Guid followedId)
        {
            _logger.LogInformation("Start FollowAsync Service");
            var followedUser = await _unitOfWork.Repository<Domain.Entites.Profile>().GetByAsync(prop => prop.ProfileID == followedId);
            if(followedUser == null)
            {
                _logger.LogWarning("FollowedId not found, Enter valid FollowedId");
                throw new Exception("FollowedId not found");
            }
            var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var follower = await _unitOfWork.Repository<Domain.Entites.Profile>().GetByAsync(prop => prop.ProfileID == user.ProfileID,includeProperties:"User");
            if (follower == null)
                throw new InvalidOperationException("follower not found");

            var isFollowed = await _unitOfWork.Repository<UserConnections>().GetByAsync(prop => prop.FollowedID == followedId && prop.FollowerID == follower.ProfileID);
            if (isFollowed != null)
            {
                _logger.LogWarning("Faild in Follow service: The followed Id was followed before by the same user!");
                throw new InvalidOperationException("You followed this profile before, so you can't follow it again!");
            }

            var newUserConnection = new UserConnections()
            {
                FollowedID = followedId,
                FollowerID = follower.ProfileID,
                CreatedAt = DateTime.Now,
                UserConnectionID = Guid.NewGuid(),
                ProfileId = follower.ProfileID,
                Profile = follower
            };
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    ++follower.TotalFollowing;
                    ++followedUser.TotalFollowers;
                    var result = await _unitOfWork.Repository<UserConnections>().CreateAsync(newUserConnection);
                    await _unitOfWork.CompleteAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    await _hubContext.Clients.All.SendAsync("ReceiveFollowingUpdate",followedUser.TotalFollowing,followedUser.TotalFollowers);
                    _logger.LogInformation("Follow Service Done Successully.");
                    var notificationAdd = new NotificationAddRequest()
                    {
                        Message = $"{follower.User.UserName} started following you",
                        NotificationType = Enumeration.NotificationType.FOLLOW,
                        ProfileID = followedId,
                        ReferenceURL = $"{GetBaseUrl()}Profile/getProfile/{follower.ProfileID}",
                    };
                    await SetNotification(notificationAdd);
                    return _mapper.Map<FollowResponse>(result); ;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("Error in Create UserConnections", ex);
                }
            }
        }

        public async Task<List<UserConnectionsResponse>> GetUserFollowingAsync(Guid profileId , int? pageIndex , int? pageSize )
        {
            _logger.LogInformation($"Start Get User following for profile : {profileId}");
            var profile = await _unitOfWork.Repository<Domain.Entites.Profile>().GetByAsync(prop => prop.ProfileID == profileId);
            if (profile == null)
            {
                _logger.LogWarning("profile not found, Enter valid profileId");
                throw new Exception("profile not found");
            }
            if (pageIndex == null & pageSize == null)
            {
                pageIndex = 1;
                pageSize = 10;
            }
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var Connections = await _unitOfWork.Repository<UserConnections>().GetAllAsync(filter: prop => prop.FollowerID == profileId, orderBy: prop => prop.CreatedAt, pageIndex: (int)pageIndex, pageSize: (int)pageSize);
                    var FollowingProfiles = new List<Domain.Entites.Profile>();
                    foreach (var person in Connections)
                    {
                        var result = await _unitOfWork.Repository<Domain.Entites.Profile>().GetByAsync(prop => prop.ProfileID == person.FollowedID, includeProperties: "User");
                        FollowingProfiles.Add(result);
                    }
                    _logger.LogInformation("Get following successfully.");
                    await _unitOfWork.CommitTransactionAsync();
                    var response = _mapper.Map<List<UserConnectionsResponse>>(FollowingProfiles);
                    response.ForEach(prop => prop.IsFollowed = true);
                    return response;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("Error in Get UserFollowing", ex);
                }
            }
        }
        public async Task<List<UserConnectionsResponse>> GetUserFollowersAsync(Guid profileId, int? pageIndex, int? pageSize)
        {
            _logger.LogInformation($"Start Get user followers for profile : {profileId}");
            var userName = GetCurrentUserName();
            var user = await _userManager.FindByNameAsync(userName);
            var loginUser = await _unitOfWork.Repository<Domain.Entites.Profile>().GetByAsync(prop => prop.ProfileID == profileId);
            var profile = await _unitOfWork.Repository<Domain.Entites.Profile>().GetByAsync(prop => prop.ProfileID == profileId);
            if (profile == null)
            {
                _logger.LogWarning("profile not found, Enter valid profileId");
                throw new Exception("profile not found");
            }
            if (pageIndex == null & pageSize == null)
            {
                pageIndex = 1;
                pageSize = 10;
            }
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var Connections = await _unitOfWork.Repository<UserConnections>().GetAllAsync(filter: prop => prop.FollowedID == profileId, orderBy: prop => prop.CreatedAt, pageIndex: (int)pageIndex, pageSize: (int)pageSize);
                    var FollowersProfiles = new List<Domain.Entites.Profile>();
                    foreach (var person in Connections)
                    {
                        var result = await _unitOfWork.Repository<Domain.Entites.Profile>().GetByAsync(prop => prop.ProfileID == person.FollowerID, includeProperties: "User");
                        FollowersProfiles.Add(result);
                    }
                    _logger.LogInformation("Get followers successfully.");
                    await _unitOfWork.CommitTransactionAsync();
                    var response = _mapper.Map<List<UserConnectionsResponse>>(FollowersProfiles);
                    foreach (var item in FollowersProfiles)
                    {
                        if (item.ProfileID == loginUser.ProfileID)
                        {
                            response.FirstOrDefault(prop => prop.Id == item.ProfileID).IsFollowed = true;
                        }
                    }
                    return response;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("Error in Get followers", ex);
                }
            }
        }

        public async Task UnfollowAsync(Guid UnfollowedId)
        {
            _logger.LogInformation("Start FollowAsync Service");
            var UnfollowedUser = await _unitOfWork.Repository<Domain.Entites.Profile>().GetByAsync(prop => prop.ProfileID == UnfollowedId);
            if (UnfollowedUser == null)
            {
                _logger.LogWarning("UnfollowedId not found, Enter valid UnfollowedId");
                throw new Exception("UnfollowedId not found");
            }
            var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var follower = await _unitOfWork.Repository<Domain.Entites.Profile>().GetByAsync(prop => prop.ProfileID == user.ProfileID);
            if (follower == null)
                throw new InvalidOperationException("follower not found");
            --follower.TotalFollowing;
            --UnfollowedUser.TotalFollowers;

            var isFollowed = await _unitOfWork.Repository<UserConnections>().GetByAsync(prop => prop.FollowedID == UnfollowedId && prop.FollowerID == follower.ProfileID);
            if (isFollowed == null)
            {
                _logger.LogWarning("Failed in Unfollow you not follow this user!");
                throw new InvalidOperationException("Failed in Unfollow you not follow this user!");
            }
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var result = await _unitOfWork.Repository<UserConnections>().DeleteAsync(isFollowed);
                    await _unitOfWork.CompleteAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    // signalR real time
                    await _hubContext.Clients.All.SendAsync("ReceiveFollowingUpdate", UnfollowedUser.TotalFollowing, UnfollowedUser.TotalFollowers);
                    _logger.LogInformation("Unfollow Service Done Successully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("Error in Unfollow service UserConnections", ex);
                }
            }
        }
        private string GetCurrentUserName()
        {
            var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userName))
                throw new UnauthorizedAccessException("User is not authenticated");

            return userName;
        }
    }
}
