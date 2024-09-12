using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SocialMediaApp.Core.Domain.IdentityEntites;
using SocialMediaApp.Core.DTO.ProfileDTO;
using SocialMediaApp.Core.Helper;
using SocialMediaApp.Core.IUnitOfWorkConfig;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProfileRepository _profileRepository;
        private readonly IFileServices _fileServices;
        private readonly ILogger<ProfileServices> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfileServices(IUnitOfWork unitOfWork,
            IFileServices fileServices,
            ILogger<ProfileServices> logger,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IProfileRepository profileRepository)
        {
            _unitOfWork = unitOfWork;
            _fileServices = fileServices;
            _logger = logger;
            _userManager = userManager;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _profileRepository = profileRepository;
        }

        public async Task<ProfileResponse> CreateAsync(ProfileAddRequest profileAddRequest)
        {
            if (profileAddRequest == null)
                throw new ArgumentNullException(nameof(profileAddRequest));

            ValidationHelper.ValidateModel(profileAddRequest);

            var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userName))
                throw new UnauthorizedAccessException("User is not authenticated");

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var user = await _userManager.FindByNameAsync(userName);
                    if (user == null)
                        throw new InvalidOperationException("User not found");

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

                    await _unitOfWork.Repository<SocialMediaApp.Core.Domain.Entites.Profile>().CreateAsync(profile);

                    user.ProfileID = profile.ProfileID;
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("Profile created successfully for user ID {UserID}", user.Id);

                    await _unitOfWork.CommitTransactionAsync();
                    return _mapper.Map<ProfileResponse>(profile);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Profile creation failed for user ID {UserName}", userName);
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
        }

        public async Task<bool> DeleteAsync(Guid? id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var profile = await _unitOfWork.Repository<SocialMediaApp.Core.Domain.Entites.Profile>().GetByAsync(x => x.ProfileID == id, true, "User");
                    if (profile == null)
                    {
                        _logger.LogWarning("Profile with ID {ProfileID} not found for deletion.", id);
                        return false;
                    }

                    var user = profile.User;

                    try
                    {
                        await Task.WhenAll(
                            _fileServices.DeleteFile(profile.ProfileImgURL),
                            _fileServices.DeleteFile(profile.ProfileBackgroundURL)
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting files for profile ID {ProfileID}", id);
                    }

                    var profileDeleted = await _unitOfWork.Repository<SocialMediaApp.Core.Domain.Entites.Profile>().DeleteAsync(profile);
                    if (!profileDeleted)
                    {
                        _logger.LogError("Failed to delete profile with ID {ProfileID}.", id);
                        return false;
                    }

                    if (user != null)
                    {
                        var userResult = await _userManager.DeleteAsync(user);
                        if (!userResult.Succeeded)
                        {
                            _logger.LogError("Failed to delete user with ID {UserID}.", user.Id);
                            return false;
                        }
                    }

                    await _unitOfWork.CommitTransactionAsync();
                    _logger.LogInformation("Profile with ID {ProfileID} and associated user deleted successfully.", id);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Profile deletion failed");
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
        }

        public async Task<IEnumerable<ProfileResponse>> GetAllAsync(int pageIndex = 1, int pageSize = 10)
        {
            var profiles = await _unitOfWork.Repository<SocialMediaApp.Core.Domain.Entites.Profile>().GetAllAsync(null, "", null, pageIndex, pageSize);
            return _mapper.Map<IEnumerable<ProfileResponse>>(profiles);
        }


        public async Task<ProfileResponse> GetProfileByAsync(Expression<Func<SocialMediaApp.Core.Domain.Entites.Profile, bool>> expression, bool IsTracked = true)
        {
            var profile = await _unitOfWork.Repository<SocialMediaApp.Core.Domain.Entites.Profile>().GetByAsync(expression, IsTracked, "User");

            if (profile == null)
            {
                _logger.LogWarning("Profile not found.");
                return null;
            }

            return _mapper.Map<ProfileResponse>(profile);
        }

        public async Task<ProfileResponse> UpdateAsync(ProfileUpdateRequest profileUpdateRequest)
        {
            if (profileUpdateRequest == null)
                throw new ArgumentNullException(nameof(profileUpdateRequest));

            ValidationHelper.ValidateModel(profileUpdateRequest);

            var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var profile = await _unitOfWork.Repository<SocialMediaApp.Core.Domain.Entites.Profile>().GetByAsync(x => x.ProfileID == profileUpdateRequest.ProfileID)
                  ?? throw new InvalidOperationException("Profile not found");

                var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new UnauthorizedAccessException("User is not authenticated");

                var user = await _userManager.FindByNameAsync(userName)
                    ?? throw new InvalidOperationException("User not found");

                var updatedProfile = _mapper.Map(profileUpdateRequest, profile);

                string oldBackgroundUrl = updatedProfile.ProfileBackgroundURL;
                string oldImgUrl = updatedProfile.ProfileImgURL;

                updatedProfile.ProfileBackgroundURL = await _fileServices.CreateFile(profileUpdateRequest.ProfileBackground);
                updatedProfile.ProfileImgURL = await _fileServices.CreateFile(profileUpdateRequest.ProfileImg);

                await _profileRepository.UpdateAsync(updatedProfile);

                await Task.WhenAll(
                    _fileServices.DeleteFile(oldBackgroundUrl),
                    _fileServices.DeleteFile(oldImgUrl)
                );

                _logger.LogInformation("Profile updated successfully for user ID {UserID}", user.Id);

                await _unitOfWork.CommitTransactionAsync();
                return _mapper.Map<ProfileResponse>(updatedProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profile update failed");
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
