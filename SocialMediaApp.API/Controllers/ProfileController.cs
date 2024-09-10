using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.DTO;
using SocialMediaApp.Core.DTO.ProfileDTO;
using SocialMediaApp.Core.ServicesContract;
using System.Net;

namespace SocialMediaApp.API.Controllers
{
    /// <summary>
    /// Controller for managing user profiles.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileServices _profileService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IProfileServices profileService, ILogger<ProfileController> logger)
        {
            _profileService = profileService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new profile.
        /// </summary>
        /// <param name="profileAdd">The profile data.</param>
        /// <returns>The created profile.</returns>
        [HttpPost("createProfile")]
        public async Task<ActionResult<ApiResponse>> CreateProfile([FromForm] ProfileAddRequest profileAdd)
        {
            _logger.LogInformation("CreateProfile method called");

            if (profileAdd == null)
            {
                _logger.LogWarning("CreateProfile method: Profile data is null");
                return BadRequest(new ApiResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    Messages = "Profile data is required"
                });
            }

            try
            {
                _logger.LogInformation("CreateProfile method: Processing profile creation");

                var profileResponse = await _profileService.CreateAsync(profileAdd);

                if (profileResponse == null)
                {
                    _logger.LogWarning("CreateProfile method: Profile creation failed");
                    return BadRequest(new ApiResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Messages = "Profile creation failed"
                    });
                }

                _logger.LogInformation("CreateProfile method: Profile created successfully");
                return Ok(new ApiResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Result = profileResponse,
                    Messages = "Profile created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProfile method: An error occurred while creating the profile");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Messages = "An error occurred while creating the profile"
                });
            }
        }
    }
}
