using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
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

        /// <summary>
        /// Updates a profile.
        /// </summary>
        /// <param name="profileUpdate">The updated profile data.</param>
        /// <returns>The updated profile.</returns>
        [HttpPost("updateProfile")]
        public async Task<ActionResult<ApiResponse>> UpdateProfile([FromForm] ProfileUpdateRequest profileUpdate)
        {
            _logger.LogInformation("UpdateProfile method called");

            if (profileUpdate == null)
            {
                _logger.LogWarning("UpdateProfile method: Profile data is null");
                return BadRequest(new ApiResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    Messages = "Profile data is required"
                });
            }

            try
            {
                var profile = await _profileService.GetProfileByAsync(x => x.ProfileID == profileUpdate.ProfileID);
                if (profile == null)
                {
                    return BadRequest("Profile not found");
                }

                _logger.LogInformation("UpdateProfile method: Processing profile update");

                var profileResponse = await _profileService.UpdateAsync(profileUpdate);

                if (profileResponse == null)
                {
                    _logger.LogWarning("UpdateProfile method: Profile update failed");
                    return BadRequest(new ApiResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Messages = "Profile update failed"
                    });
                }

                _logger.LogInformation("UpdateProfile method: Profile updated successfully");
                return Ok(new ApiResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Result = profileResponse,
                    Messages = "Profile updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProfile method: An error occurred while updating the profile");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Messages = "An error occurred while updating the profile"
                });
            }
        }

        /// <summary>
        /// Deletes a profile.
        /// </summary>
        /// <param name="id">The ID of the profile to delete.</param>
        /// <returns>The result of the profile deletion.</returns>
        [HttpDelete("deleteProfile/{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteProfile(Guid id)
        {
            _logger.LogInformation("DeleteProfile method called");

            if (id == Guid.Empty)
            {
                _logger.LogWarning("DeleteProfile method: Profile ID is required");
                return Accepted(new ApiResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    Messages = "Profile ID is required"
                });
            }

            try
            {
                var profile = await _profileService.GetProfileByAsync(x => x.ProfileID == id);
                if(profile == null)
                {
                    return BadRequest(new ApiResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Messages = "Profile not found"
                    });
                }
                _logger.LogInformation("DeleteProfile method: Processing profile deletion");

                var result = await _profileService.DeleteAsync(id);

                if (!result)
                {
                    _logger.LogWarning("DeleteProfile method: Profile deletion failed");
                    return Accepted(new ApiResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Messages = "Profile deletion failed"
                    });
                }

                _logger.LogInformation("DeleteProfile method: Profile deleted successfully");
                return Accepted(new ApiResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Messages = "Profile deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteProfile method: An error occurred while deleting the profile");
                return Accepted(new ApiResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Messages = "An error occurred while deleting the profile"
                });
            }
        }

        /// <summary>
        /// Gets a profile by ID.
        /// </summary>
        /// <param name="id">The ID of the profile to get.</param>
        /// <returns>The profile.</returns>
        [HttpGet("getProfile/{id}")]
        public async Task<ActionResult<ApiResponse>> GetProfile(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new ApiResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    Messages = "Profile ID is required"
                });
            }
            try
            {
                var profile = await _profileService.GetProfileByAsync(x => x.ProfileID == id);

                if (profile == null)
                {
                    return BadRequest(new ApiResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Messages = "Profile not found"
                    });
                }
                return Ok(new ApiResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Result = profile,
                    Messages = "Profile found"
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Messages = "An error occurred while getting the profile"
                });
            }

        }
    }
}
