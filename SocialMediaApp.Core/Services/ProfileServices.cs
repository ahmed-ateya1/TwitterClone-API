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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
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
            profile.ProfileBackgroundURL = await _fileServices.CreateFile(profileAddRequest.ProfileBackground);
            profile.ProfileImgURL = await _fileServices.CreateFile(profileAddRequest.ProfileImg);
            profile.ProfileID = Guid.NewGuid();

            await _profileRepository.CreateAsync(profile);

            user.ProfileID = profile.ProfileID;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Profile created successfully.");

            return _mapper.Map<ProfileResponse>(profile);
        }

        public async Task<bool> DeleteAsync(Guid? id)
        {
            if(id == null)
                throw new ArgumentNullException(nameof(id));

            var profile = await _profileRepository.GetByAsync(x => x.ProfileID == id);

            if (profile == null)
                return false;

            return await _profileRepository.DeleteAsync(profile);
        }

        public async Task<ProfileResponse> GetProfileByAsync(Expression<Func<SocialMediaApp.Core.Domain.Entites.Profile, bool>> expression, bool IsTracked = false)
        {
            var profile = await _profileRepository.GetByAsync(expression, IsTracked);
            return _mapper.Map<ProfileResponse>(profile);
        }

        public Task<ProfileResponse> UpdateAsync(ProfileUpdateRequest? profileUpdateRequest)
        {
            throw new NotImplementedException();
        }
    }
}
