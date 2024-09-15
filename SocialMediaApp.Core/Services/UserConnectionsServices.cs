using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.Domain.IdentityEntites;
using SocialMediaApp.Core.DTO.UserConnectionsDTO;
using SocialMediaApp.Core.Hubs;
using SocialMediaApp.Core.IUnitOfWorkConfig;
using SocialMediaApp.Core.ServicesContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Services
{
    public class UserConnectionsServices : IUserConnectionsServices
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserConnectionsServices> _logger;
        private readonly IMapper _mapper;
        private readonly IHubContext<TotalFollowingUnfollowingHub> _hubContext;
        public UserConnectionsServices(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, ILogger<UserConnectionsServices> logger, IMapper mapper, IHubContext<TotalFollowingUnfollowingHub> hubContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<UserConnectionsResponse> FollowAsync(Guid followedId)
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

            var follower = await _unitOfWork.Repository<Domain.Entites.Profile>().GetByAsync(prop => prop.ProfileID == user.ProfileID);
            if (follower == null)
                throw new InvalidOperationException("follower not found");

            var isFollowed = await _unitOfWork.Repository<UserConnections>().GetByAsync(prop => prop.FollowedID == followedId && prop.FollowerID == follower.ProfileID);
            if (isFollowed != null)
            {
                _logger.LogWarning("Faild in Follow service: The followed Id was followed before by the same user!");
                throw new InvalidOperationException("You followed this profile before, so you can't follow it again!");
            }
            ++follower.TotalFollowing;
            ++followedUser.TotalFollowers;

            var newUserConnection = new UserConnections()
            {
                FollowedID = followedId,
                FollowerID = follower.ProfileID,
                CreatedAt = DateTime.Now,
                UserConnectionID = Guid.NewGuid()
            };
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var result = await _unitOfWork.Repository<UserConnections>().CreateAsync(newUserConnection);
                    await _unitOfWork.CompleteAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    // signalR real time
                    await _hubContext.Clients.All.SendAsync("ReceiveFollowingUpdate",followedUser.TotalFollowing,followedUser.TotalFollowers);
                    _logger.LogInformation("Follow Service Done Successully.");
                    return _mapper.Map<UserConnectionsResponse>(result);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw new InvalidOperationException("Error in Create UserConnections", ex);
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
    }
}
