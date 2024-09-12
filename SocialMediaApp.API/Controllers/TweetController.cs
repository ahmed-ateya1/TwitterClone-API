using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.DTO.TweetDTO;
using SocialMediaApp.Core.DTO;
using SocialMediaApp.Core.ServicesContract;
using System.Net;

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
}
