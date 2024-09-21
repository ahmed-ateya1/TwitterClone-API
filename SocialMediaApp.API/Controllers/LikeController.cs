using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.DTO;
using SocialMediaApp.Core.ServicesContract;
using System.Net;

namespace SocialMediaApp.API.Controllers
{
    /// <summary>
    /// Controller for handling likes on tweets and comments.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LikeController : ControllerBase
    {
        private readonly ILikeServices _likeServices;
        private readonly ILogger<LikeController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LikeController"/> class.
        /// </summary>
        /// <param name="likeServices">The like services.</param>
        /// <param name="logger">The logger.</param>
        public LikeController(ILikeServices likeServices, ILogger<LikeController> logger)
        {
            _likeServices = likeServices;
            _logger = logger;
        }

        /// <summary>
        /// Likes a tweet or comment.
        /// </summary>
        /// <param name="id">The ID of the tweet or comment to like.</param>
        /// <returns>The response containing the result of the like operation.</returns>
        [HttpPost("likeTweetOrComment/{id}")]
        public async Task<ActionResult<ApiResponse>> LikeTweetOrComment(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse()
                {
                    IsSuccess = false,
                    Messages = "Invalid ID",
                });
            }
            try
            {
                var response = await _likeServices.LikeAsync(id);
                if (response == null)
                {
                    return BadRequest(new ApiResponse()
                    {
                        IsSuccess = false,
                        Messages = "Invalid ID",
                    });
                }
                return Ok(new ApiResponse()
                {
                    IsSuccess = true,
                    Messages = "Liked Successfully",
                    Result = response,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse()
                {
                    IsSuccess = false,
                    Messages = "Internal Server Error",
                });
            }
        }

        /// <summary>
        /// Unlikes a tweet or comment.
        /// </summary>
        /// <param name="id">The ID of the tweet or comment to like.</param>
        /// <returns>The response containing the result of the unlike operation.</returns>
        [HttpDelete("unlikeTweetOrComment/{id}")]
        public async Task<ActionResult<ApiResponse>> UnlikeTweetOrComment(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse()
                {
                    IsSuccess = false,
                    Messages = "Invalid ID",
                });
            }
            try
            {
                var response = await _likeServices.UnLikeAsync(id);
                if (!response)
                {
                    return BadRequest(new ApiResponse()
                    {
                        IsSuccess = false,
                        Messages = "Invalid ID",
                    });
                }
                return Ok(new ApiResponse()
                {
                    IsSuccess = true,
                    Messages = "Unliked Successfully",
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse()
                {
                    IsSuccess = false,
                    Messages = "Internal Server Error",
                });
            }
        }

        /// <summary>
        /// Gets the likes for a tweet or comment.
        /// </summary>
        /// <param name="id">The ID of the tweet or comment to get the likes for.</param>
        /// <returns>The response containing the likes for the tweet or comment.</returns>
        [HttpGet("getLikesFortweetOrComment/{id}")]
        public async Task<ActionResult<ApiResponse>> getLikestweetOrComment(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse()
                {
                    IsSuccess = false,
                    Messages = "Invalid ID",
                });
            }
            try
            {
                var likes = await _likeServices.GetAllAsync(x => x.TweetID == id || x.CommentID == id);
                return Ok(new ApiResponse()
                {
                    IsSuccess = true,
                    Messages = "Likes found",
                    Result = likes,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse()
                {
                    IsSuccess = false,
                    Messages = "Internal Server Error",
                });
            }
        }

        /// <summary>
        /// Checks if the user has liked a tweet or comment.
        /// </summary>
        /// <param name="id">The ID of the tweet or comment to check.</param>
        /// <returns>The response containing the result of the check.</returns>
        [HttpGet("userIsLiked/{id}")]
        public async Task<ActionResult<ApiResponse>> UserIsLiked(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    Messages = "Invalid ID",
                });
            }
            try
            {
                var response = await _likeServices.IsLikeToAsync(id);
                return Ok(new ApiResponse()
                {
                    IsSuccess = true,
                    Result = response,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse()
                {
                    IsSuccess = false,
                    Messages = "Internal Server Error",
                });
            }
        }
    }
}
