using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.DTO.TweetDTO;
using SocialMediaApp.Core.DTO;
using SocialMediaApp.Core.ServicesContract;
using System.Net;
using SocialMediaApp.Core.Domain.Entites;
using AutoMapper;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TweetController : ControllerBase
{
    private readonly ITweetServices _tweetServices;
    private readonly ILogger<TweetController> _logger;

    public TweetController(ITweetServices tweetServices, ILogger<TweetController> logger)
    {
        _tweetServices = tweetServices;
        _logger = logger;
    }

    [HttpPost("createTweet")]
    public async Task<ActionResult<ApiResponse>> CreateTweet([FromForm] TweetAddRequest tweetAddRequest)
    {
        _logger.LogInformation("CreateTweet method called");

        if (tweetAddRequest == null)
        {
            _logger.LogWarning("CreateTweet method: Tweet data is null");
            return BadRequest(new ApiResponse
            {
                IsSuccess = false,
                Messages = "Tweet data is required",
                StatusCode = HttpStatusCode.BadRequest
            });
        }

        try
        {
            var tweet = await _tweetServices.CreateAsync(tweetAddRequest);
            if (tweet == null)
            {
                _logger.LogWarning("CreateTweet method: Tweet data is null");
                return BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    Messages = "Tweet data is required",
                    StatusCode = HttpStatusCode.BadRequest
                });
            }

            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Messages = "Tweet created successfully",
                StatusCode = HttpStatusCode.OK,
                Result = tweet
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateTweet method: An error occurred while creating tweet");
            return StatusCode(500, new ApiResponse
            {
                IsSuccess = false,
                Messages = "An error occurred while creating tweet",
                StatusCode = HttpStatusCode.InternalServerError
            });
        }
    }

    [HttpPut("updateTweet")]
    public async Task<ActionResult<ApiResponse>> UpdateTweet([FromForm] TweetUpdateRequest tweetUpdateRequest)
    {
        if (tweetUpdateRequest.TweetID == Guid.Empty)
        {
            _logger.LogWarning("UpdateTweet method: Tweet ID is null");
            return BadRequest(new ApiResponse
            {
                IsSuccess = false,
                Messages = "Tweet ID is required",
                StatusCode = HttpStatusCode.BadRequest
            });
        }
        var tweet = await _tweetServices.GetByAsync(x => x.TweetID == tweetUpdateRequest.TweetID);
        if (tweet == null)
        {
            _logger.LogWarning("UpdateTweet method: Tweet not found");
            return NotFound(new ApiResponse
            {
                IsSuccess = false,
                Messages = "Tweet not found",
                StatusCode = HttpStatusCode.NotFound
            });
        }
        try
        {
            var updateTweet = await _tweetServices.UpdateAsync(tweetUpdateRequest);
            if (updateTweet == null)
            {
                _logger.LogWarning("UpdateTweet method: Tweet data is null");
                return BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    Messages = "Tweet data is required",
                    StatusCode = HttpStatusCode.BadRequest
                });
            }
            return Ok(new ApiResponse
            {
                IsSuccess = true,
                Messages = "Tweet updated successfully",
                StatusCode = HttpStatusCode.OK,
                Result = updateTweet
            });
        }
        catch(Exception ex)
        {
            return StatusCode(500, new ApiResponse
            {
                IsSuccess = false,
                Messages = "An error occurred while updating tweet",
                StatusCode = HttpStatusCode.InternalServerError
            });
        }
    }

        [HttpDelete("deleteTweet/{tweetID}")]
    public async Task<ActionResult<ApiResponse>> DeleteTweet(Guid tweetID)
    {
        if (tweetID == Guid.Empty)
        {
            _logger.LogWarning("DeleteTweet method: Tweet ID is null");
            return BadRequest(new ApiResponse
            {
                IsSuccess = false,
                Messages = "Tweet ID is required",
                StatusCode = HttpStatusCode.BadRequest
            });
        }
       
        try
        {
            var tweet = await _tweetServices.GetByAsync(x => x.TweetID == tweetID);
            if (tweet == null)
            {
                _logger.LogWarning("DeleteTweet method: Tweet not found");
                return NotFound(new ApiResponse
                {
                    IsSuccess = false,
                    Messages = "Tweet not found",
                    StatusCode = HttpStatusCode.NotFound
                });
            }
            _logger.LogInformation("DeleteTweet method: Processing profile deletion");
            var result = await _tweetServices.DeleteAsync(tweetID);

            if (!result)
            {
                _logger.LogWarning("DeleteTweet method: Tweet deletion failed");
                return Accepted(new ApiResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    Messages = "Tweet deletion failed"
                });
            }

            _logger.LogInformation("DeleteTweet method: Tweet deleted successfully");
            return Accepted(new ApiResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Messages = "Tweet deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteTweet method: An error occurred while deleting tweet");
            return StatusCode(500, new ApiResponse
            {
                IsSuccess = false,
                Messages = "An error occurred while deleting tweet",
                StatusCode = HttpStatusCode.InternalServerError
            });
        }
    }

    [HttpGet("getTweets")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> GetTweets(int pageIndex = 1 , int pageSize =10 )
    {
        var tweets = await _tweetServices.GetAllAsync(null, x=>x.CreatedAt, pageIndex, pageSize);
        return Ok(new ApiResponse
        {
            StatusCode = HttpStatusCode.OK,
            IsSuccess = true,
            Result = tweets,
            Messages = "Tweets found"
        });
    }

    [HttpGet("getTweet/{tweetID}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> GetTweet(Guid tweetID)
    {
        var tweet = await _tweetServices.GetByAsync(x => x.TweetID == tweetID);
        if (tweet == null)
        {
            return NotFound(new ApiResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                IsSuccess = false,
                Messages = "Tweet not found"
            });
        }

        return Ok(new ApiResponse
        {
            StatusCode = HttpStatusCode.OK,
            IsSuccess = true,
            Result = tweet,
            Messages = "Tweet found"
        });
    }
    [HttpGet("getTweetstoSpecificProfile/{profileID}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> GetTweetstoSpecificProfile(Guid profileID , int pageIndex = 1, int pageSize = 10)
    {
        var tweets = await _tweetServices.GetAllAsync(x => x.ProfileID == profileID, x => x.CreatedAt, pageIndex, pageSize);
        return Ok(new ApiResponse
        {
            StatusCode = HttpStatusCode.OK,
            IsSuccess = true,
            Result = tweets,
            Messages = "Tweets found"
        });
    }
}
