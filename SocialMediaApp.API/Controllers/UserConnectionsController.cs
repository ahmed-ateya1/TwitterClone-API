using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.DTO;
using SocialMediaApp.Core.ServicesContract;
using System.Net;

namespace SocialMediaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserConnectionsController : ControllerBase
    {
        private readonly IUserConnectionsServices _userConnectionsServices;
        private readonly ILogger<UserConnectionsController> _logger;
        /// <summary>
        /// Initializes a new instance of the <see cref="UserConnectionsController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="userConnectionsServices">The tweet services.</param>

        public UserConnectionsController(ILogger<UserConnectionsController> logger, IUserConnectionsServices userConnectionsServices)
        {
            _logger = logger;
            _userConnectionsServices = userConnectionsServices;
        }
        /// <summary>
        ///  Make follow to a specific profile.
        /// </summary>
        /// <param name="followedId">The Id of profile you want to follow.</param>
        /// <returns>The Follower profile Id Followed profile Id and creation time.</returns>
        [HttpPost("Follow/{followedId}")]
        public async Task<ActionResult<ApiResponse>> Follow(Guid followedId)
        {
            _logger.LogInformation($"Start Follow with followedId : {followedId}");
            if (followedId == null)
            {
                return BadRequest(new ApiResponse{
                    IsSuccess = false,
                    Messages = "FollowedId is Null, Enter valid value!",
                    StatusCode = HttpStatusCode.BadRequest
                }) ;
            }
            try
            {
                var result = await _userConnectionsServices.FollowAsync(followedId);
                _logger.LogInformation("Follow method executed successfully!");
                return StatusCode((int)HttpStatusCode.OK, new ApiResponse
                {
                    IsSuccess = true,
                    Messages = "Follow Done Successfully",
                    Result = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Follow Method : An error occurred while execute Follow!");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse
                {
                    IsSuccess = false,
                    Messages = ex.Message,
                    StatusCode = HttpStatusCode.InternalServerError
                });
            }
        }
        /// <summary>
        ///  Make Unfollow to a specific profile.
        /// </summary>
        /// <param name="UnfollowedId">The Id of profile you want to Unfollow.</param>
        /// <returns></returns>
        [HttpPost("Unfollow/{UnfollowedId}")]
        public async Task<ActionResult<ApiResponse>> Unfollow(Guid UnfollowedId)
        {
            _logger.LogInformation($"Start Unfollow with UnfollowedId : {UnfollowedId}");
            if (UnfollowedId == null)
            {
                return BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    Messages = "UnfollowedId is Null, Enter valid value!",
                    StatusCode = HttpStatusCode.BadRequest
                });
            }
            try
            {
                await _userConnectionsServices.UnfollowAsync(UnfollowedId);
                _logger.LogInformation("Unfollow method executed successfully!");
                return StatusCode((int)HttpStatusCode.OK, new ApiResponse
                {
                    IsSuccess = true,
                    Messages = "Unfollow Done Successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unfollow Method : An error occurred while execute Unfollow!");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse
                {
                    IsSuccess = false,
                    Messages = ex.Message,
                    StatusCode = HttpStatusCode.InternalServerError
                });
            }
        }
    }
}
