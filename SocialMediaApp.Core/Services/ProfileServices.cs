using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SocialMediaApp.Core.Domain.IdentityEntites;
using SocialMediaApp.Core.DTO.ProfileDTO;
using SocialMediaApp.Core.Helper;
using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Core.ServicesContract;
using System;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Services
{
    public class ProfileServices : IProfileServices
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IFileServices _fileServices;
        private readonly ILogger<ProfileServices> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfileServices(IProfileRepository profileRepository,
            IFileServices fileServices,
            ILogger<ProfileServices> logger,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _profileRepository = profileRepository;
            _fileServices = fileServices;
            _logger = logger;
            _userManager = userManager;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ProfileResponse> CreateAsync(ProfileAddRequest profileAddRequest)
        {
            if (profileAddRequest == null)
                throw new ArgumentNullException(nameof(profileAddRequest));

            ValidationHelper.ValidateModel(profileAddRequest);

            var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName), "User is not authenticated");

            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
                throw new ArgumentNullException(nameof(user), "User not found");

            var profile = _mapper.Map<SocialMediaApp.Core.Domain.Entites.Profile>(profileAddRequest);

            profile.UserID = user.Id;
            profile.User = user;
            profile.ProfileID = Guid.NewGuid();

            try
            {
                profile.ProfileBackgroundURL = await _fileServices.CreateFile(profileAddRequest.ProfileBackground);
                profile.ProfileImgURL = await _fileServices.CreateFile(profileAddRequest.ProfileImg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload files for profile.");
                throw new InvalidOperationException("File upload failed", ex);
            }

            await _profileRepository.CreateAsync(profile);

            user.ProfileID = profile.ProfileID;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Profile created successfully for user ID {UserID}", user.Id);

            return _mapper.Map<ProfileResponse>(profile);
        }

        public async Task<bool> DeleteAsync(Guid? id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var profile = await _profileRepository.GetByAsync(x => x.ProfileID == id, true, "User");

            if (profile == null)
            {
                _logger.LogWarning("Profile with ID {ProfileID} not found for deletion.", id);
                return false;
            }
            var user = profile.User;
            if (user != null)
            {
                var userResult = await _userManager.DeleteAsync(user);
                if (!userResult.Succeeded)
                {
                    _logger.LogError("Failed to delete user with ID {UserID}.", user.Id);
                    return false;
                }
            }
            await Task.WhenAll(
               _fileServices.DeleteFile(profile.ProfileImgURL),
               _fileServices.DeleteFile(profile.ProfileBackgroundURL)
           );
            var result = await _profileRepository.DeleteAsync(profile);

            if (result)
            {
                _logger.LogInformation("Profile with ID {ProfileID} deleted successfully.", id);
            }
            else
            {
                _logger.LogError("Failed to delete profile with ID {ProfileID}.", id);
            }

            return result;
        }


        public async Task<ProfileResponse> GetProfileByAsync(Expression<Func<SocialMediaApp.Core.Domain.Entites.Profile, bool>> expression, bool IsTracked = true)
        {
            var profile = await _profileRepository.GetByAsync(expression, IsTracked , "User");
            return _mapper.Map<ProfileResponse>(profile);
        }

        public async Task<ProfileResponse> UpdateAsync(ProfileUpdateRequest profileUpdateRequest)
        {
            if (profileUpdateRequest == null)
                throw new ArgumentNullException(nameof(profileUpdateRequest));

            ValidationHelper.ValidateModel(profileUpdateRequest);

            var profile = await _profileRepository.GetByAsync(x => x.ProfileID == profileUpdateRequest.ProfileID)
                ?? throw new ArgumentNullException("Profile not found");

            var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User is not authenticated");

            var user = await _userManager.FindByNameAsync(userName)
                ?? throw new ArgumentNullException("User not found");

            var Updateprofile = _mapper.Map(profileUpdateRequest , profile);

            string oldBackgroundUrl = Updateprofile.ProfileBackgroundURL;
            string oldImgUrl = Updateprofile.ProfileImgURL;

            try
            {
                Updateprofile.ProfileBackgroundURL = await _fileServices.CreateFile(profileUpdateRequest.ProfileBackground);
                Updateprofile.ProfileImgURL = await _fileServices.CreateFile(profileUpdateRequest.ProfileImg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload new files for profile.");
                throw new InvalidOperationException("File upload failed", ex);
            }

            await _profileRepository.UpdateAsync(Updateprofile);

            await Task.WhenAll(
                _fileServices.DeleteFile(oldBackgroundUrl),
                _fileServices.DeleteFile(oldImgUrl)
            );

            _logger.LogInformation("Profile updated successfully for user ID {UserID}", user.Id);

            return _mapper.Map<ProfileResponse>(Updateprofile);
        }
    }
}
