using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.DTO;
using SocialMediaApp.Core.DTO.CommentDTO;
using SocialMediaApp.Core.ServicesContract;
using System.Net;

namespace SocialMediaApp.API.Controllers
{
    /// <summary>
    /// Controller for managing comments on tweets.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentServices _commentServices;
        private readonly ILogger<CommentController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentController"/> class.
        /// </summary>
        /// <param name="commentServices">The comment services.</param>
        /// <param name="logger">The logger instance for logging.</param>
        public CommentController(ICommentServices commentServices, ILogger<CommentController> logger)
        {
            _commentServices = commentServices;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new comment.
        /// </summary>
        /// <param name="commentAdd">The comment details to be added.</param>
        /// <returns>An API response indicating success or failure.</returns>
        [HttpPost("createComment")]
        public async Task<ActionResult<ApiResponse>> CreateComment([FromForm] CommentAddRequest commentAdd)
        {
            if (commentAdd == null)
            {
                return BadRequest(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        Messages = "Comment is required"
                    }
                );
            }
            try
            {
                var comment = await _commentServices.CreateAsync(commentAdd);
                if (comment == null)
                {
                    return BadRequest(
                        new ApiResponse
                        {
                            IsSuccess = false,
                            StatusCode = HttpStatusCode.BadRequest,
                            Messages = "Comment not created"
                        }
                    );
                }
                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        StatusCode = HttpStatusCode.OK,
                        Messages = "Comment created successfully",
                        Result = comment
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500,
                    new ApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.InternalServerError,
                        Messages = "Internal Server Error"
                    }
                );
            }
        }

        /// <summary>
        /// Retrieves comments with pagination.
        /// </summary>
        /// <param name="pageIndex">The index of the page to retrieve (default is 1).</param>
        /// <param name="pageSize">The number of comments per page (default is 5).</param>
        /// <returns>An API response containing the list of comments.</returns>
        [HttpGet("getComments")]
        public async Task<ActionResult<ApiResponse>> GetComments(int pageIndex = 1, int pageSize = 5)
        {
            try
            {
                var comments = await _commentServices.GetAllAsync(x => x.ParentComment == null, pageIndex, pageSize);

                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        StatusCode = HttpStatusCode.OK,
                        Messages = "Comments found successfully",
                        Result = comments
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500,
                    new ApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.InternalServerError,
                        Messages = "Internal Server Error"
                    }
                );
            }
        }

        /// <summary>
        /// Retrieves comments for a specific tweet.
        /// </summary>
        /// <param name="tweetID">The ID of the tweet.</param>
        /// <returns>An API response containing the list of comments for the tweet.</returns>
        [HttpGet("getCommentsForTweet/{tweetID}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse>> GetCommentsForTweet(Guid tweetID)
        {
            if (tweetID == Guid.Empty)
            {
                return BadRequest(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        Messages = "Tweet ID is required"
                    }
                );
            }
            try
            {
                var comments = await _commentServices.GetAllAsync(x => x.TweetID == tweetID && x.ParentCommentID == null);
                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        StatusCode = HttpStatusCode.OK,
                        Messages = "Comments found successfully",
                        Result = comments
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500,
                    new ApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.InternalServerError,
                        Messages = "Internal Server Error"
                    }
                );
            }
        }

        /// <summary>
        /// Retrieves replies for a specific comment.
        /// </summary>
        /// <param name="commentId">The ID of the comment.</param>
        /// <returns>An API response containing the list of replies for the comment.</returns>
        [HttpGet("getRepliesForComment/{commentId}")]
        public async Task<ActionResult<ApiResponse>> GetRepliesForComment(Guid commentId)
        {
            if (commentId == Guid.Empty)
            {
                return BadRequest(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        Messages = "Comment ID is required"
                    }
                );
            }
            try
            {
                var comments = await _commentServices.GetAllAsync(x => x.ParentCommentID == commentId);
                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        StatusCode = HttpStatusCode.OK,
                        Messages = "Replies found successfully",
                        Result = comments
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500,
                    new ApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.InternalServerError,
                        Messages = "Internal Server Error"
                    }
                );
            }
        }

        /// <summary>
        /// Retrieves a comment by its ID.
        /// </summary>
        /// <param name="commentId">The ID of the comment.</param>
        /// <returns>An API response containing the comment.</returns>
        [HttpGet("getComment/{commentId}")]
        public async Task<ActionResult<ApiResponse>> GetComment(Guid commentId)
        {
            if (commentId == Guid.Empty)
            {
                return BadRequest(new ApiResponse()
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Messages = "Comment ID is required"
                });
            }
            try
            {
                var comment = await _commentServices.GetByAsync(x => x.CommentID == commentId);
                if (comment == null)
                {
                    return BadRequest(new ApiResponse()
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        Messages = "Comment not found"
                    });
                }
                return Ok(new ApiResponse()
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Messages = "Comment found successfully",
                    Result = comment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500,
                    new ApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.InternalServerError,
                        Messages = "Internal Server Error"
                    }
                );
            }
        }

        /// <summary>
        /// Updates an existing comment.
        /// </summary>
        /// <param name="commentUpdate">The updated comment details.</param>
        /// <returns>An API response indicating the success of the update operation.</returns>
        [HttpPut("updateComment")]
        public async Task<ActionResult<ApiResponse>> UpdateComment([FromForm] CommentUpdateRequest commentUpdate)
        {
            try
            {
                var commentFind = await _commentServices.GetByAsync(x => x.CommentID == commentUpdate.CommentId);
                if (commentFind == null)
                {
                    return BadRequest(
                        new ApiResponse
                        {
                            IsSuccess = false,
                            StatusCode = HttpStatusCode.BadRequest,
                            Messages = "Comment not found"
                        }
                    );
                }
                var comment = await _commentServices.UpdateAsync(commentUpdate);
                if (comment == null)
                {
                    return BadRequest(
                        new ApiResponse
                        {
                            IsSuccess = false,
                            StatusCode = HttpStatusCode.BadRequest,
                            Messages = "Comment not updated"
                        }
                    );
                }
                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        StatusCode = HttpStatusCode.OK,
                        Messages = "Comment updated successfully",
                        Result = comment
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500,
                    new ApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.InternalServerError,
                        Messages = "Internal Server Error"
                    }
                );
            }
        }

        /// <summary>
        /// Deletes a comment by its ID.
        /// </summary>
        /// <param name="commentID">The ID of the comment to delete.</param>
        /// <returns>An API response indicating the success or failure of the delete operation.</returns>
        [HttpDelete("deleteComment/{commentID}")]
        public async Task<ActionResult<ApiResponse>> DeleteComment(Guid commentID)
        {
            if (commentID == Guid.Empty)
            {
                return BadRequest(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        Messages = "Comment ID is required"
                    }
                );
            }
            try
            {
                var comment = await _commentServices.GetByAsync(x => x.CommentID == commentID);
                if (comment == null)
                {
                    return BadRequest(
                        new ApiResponse
                        {
                            IsSuccess = false,
                            StatusCode = HttpStatusCode.BadRequest,
                            Messages = "Comment not found"
                        }
                    );
                }
                var isDeleted = await _commentServices.DeleteAsync(commentID);
                if (!isDeleted)
                {
                    return BadRequest(
                        new ApiResponse
                        {
                            IsSuccess = false,
                            StatusCode = HttpStatusCode.BadRequest,
                            Messages = "Comment not deleted"
                        }
                    );
                }
                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        StatusCode = HttpStatusCode.OK,
                        Messages = "Comment deleted successfully"
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500,
                    new ApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.InternalServerError,
                        Messages = "Internal Server Error"
                    }
                );
            }
        }
    }
}
