using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SocialMediaApp.Core.Domain.Entites;
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

        private async Task SetFollowedStatus(IEnumerable<ProfileResponse> profiles, Guid profileID)
        {
            foreach (var profile in profiles)
            {
                profile.IsFollowed = await _unitOfWork.Repository<UserConnections>()
                    .GetByAsync(x => x.FollowerID == profileID && x.FollowedID == profile.ProfileID) != null;
            }
        }

        private string GetCurrentUserName()
        {
            var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userName))
                throw new UnauthorizedAccessException("User is not authenticated");

            return userName;
        }
        private async Task<SocialMediaApp.Core.Domain.Entites.Profile?> GetProfileIfAvailable()
        {
            var userName = GetCurrentUserName();
            if (userName == null) return null;

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return null;

            return await _unitOfWork.Repository<SocialMediaApp.Core.Domain.Entites.Profile>().GetByAsync(x => x.User.Id == user.Id);
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

                    var profile = _mapper.Map<Domain.Entites.Profile>(profileAddRequest);
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

                    await _unitOfWork.Repository<Domain.Entites.Profile>().CreateAsync(profile);

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
                    var profile = await _unitOfWork.Repository<Domain.Entites.Profile>().GetByAsync(x => x.ProfileID == id, true, "User");

                    if (profile == null)
                    {
                        _logger.LogWarning("Profile with ID {ProfileID} not found for deletion.", id);
                        return false;
                    }
                    
                    var tweets = await _unitOfWork.Repository<Tweet>().GetAllAsync(x => x.ProfileID == id);

                    if(tweets != null)
                    {
                        await _unitOfWork.Repository<Tweet>().RemoveRangeAsync(tweets);
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

                    var profileDeleted = await _unitOfWork.Repository<Domain.Entites.Profile>().DeleteAsync(profile);
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
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            var profiles = await _unitOfWork.Repository<Domain.Entites.Profile>().GetAllAsync(includeProperties:"User",pageIndex:pageIndex,pageSize:pageSize);
            var result =  _mapper.Map<IEnumerable<ProfileResponse>>(profiles);
            var currentProfile = await GetProfileIfAvailable();
            if (currentProfile != null)
            {
                await SetFollowedStatus(result, currentProfile.ProfileID);
            }
            return result;
        }


        public async Task<ProfileResponse> GetProfileByAsync(Expression<Func<Domain.Entites.Profile, bool>> expression, bool IsTracked = true)
        {
            var profile = await _unitOfWork.Repository<Domain.Entites.Profile>().GetByAsync(expression, IsTracked, "User");

            if (profile == null)
            {
                _logger.LogWarning("Profile not found.");
                return null;
            }

            var result = _mapper.Map<ProfileResponse>(profile);
            var CurrentProfile = await GetProfileIfAvailable();
            if(CurrentProfile!=null)
            {
                result.IsFollowed = await _unitOfWork.Repository<UserConnections>()
                .GetByAsync(x => x.FollowedID == CurrentProfile.ProfileID 
                && x.FollowerID == result.ProfileID) != null;
            }
            
            return result;
        }

        public async Task<ProfileResponse> UpdateAsync(ProfileUpdateRequest profileUpdateRequest)
        {
            if (profileUpdateRequest == null)
                throw new ArgumentNullException(nameof(profileUpdateRequest));

            ValidationHelper.ValidateModel(profileUpdateRequest);

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var profile = await _unitOfWork.Repository<Domain.Entites.Profile>().GetByAsync(x => x.ProfileID == profileUpdateRequest.ProfileID)
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
}
