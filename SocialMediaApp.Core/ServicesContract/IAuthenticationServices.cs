using SocialMediaApp.Core.DTO.AuthenticationDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.ServicesContract
{
    public interface IAuthenticationServices 
    {
        Task<AuthenticationResponse> RegisterAsync(RegisterDTO registerDTO);
        Task<AuthenticationResponse> LoginAsync(LoginDTO loginDTO);
        Task<AuthenticationResponse> RefreshTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);
        Task<string> AddRoleToUserAsync(AddRoleDTO model);
    }
}
