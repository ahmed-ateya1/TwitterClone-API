using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SocialMediaApp.Core.Domain.IdentityEntites;
using SocialMediaApp.Core.DTO;
using SocialMediaApp.Core.Enumeration;
using SocialMediaApp.Core.ServicesContract;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Services
{
    /// <summary>
    /// Service for handling authentication-related operations such as user registration, login, JWT generation, and token management.
    /// </summary>
    public class AuthenticationServices : IAuthenticationServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly JwtDTO _jwt;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationServices"/> class.
        /// </summary>
        /// <param name="userManager">User manager for handling user-related operations.</param>
        /// <param name="roleManager">Role manager for handling role-related operations.</param>
        /// <param name="jwt">JWT configuration options.</param>
        public AuthenticationServices(UserManager<ApplicationUser> userManager,
                                      RoleManager<ApplicationRole> roleManager,
                                      IOptions<JwtDTO> jwt)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwt = jwt.Value;
        }

        /// <summary>
        /// Registers a new user with the provided registration details.
        /// </summary>
        /// <param name="registerDTO">Data transfer object containing the user's registration details.</param>
        /// <returns>An <see cref="AuthenticationResponse"/> containing the result of the registration process.</returns>
        public async Task<AuthenticationResponse> RegisterAsync(RegisterDTO registerDTO)
        {
            if (await _userManager.FindByEmailAsync(registerDTO.Email) != null)
                return new AuthenticationResponse { Message = "Email is already registered!" };

            if (await _userManager.FindByNameAsync(registerDTO.UserName) != null)
                return new AuthenticationResponse { Message = "Username is already registered!" };

            var user = new ApplicationUser
            {
                UserName = registerDTO.UserName,
                Email = registerDTO.Email,
            };

            var result = await _userManager.CreateAsync(user, registerDTO.Password);
            if (!result.Succeeded)
                return CreateErrorResponse(result.Errors);

            var roleName = registerDTO.RolesOption == RolesOption.ADMIN ? "ADMIN" : "USER";
            var addRoleResult = await AddRoleToUserAsync(new AddRoleDTO { RoleName = roleName, UserID = user.Id });

            if (!string.IsNullOrEmpty(addRoleResult))
                return new AuthenticationResponse { Message = addRoleResult };


            var authenticationUser = await GenerateJwtToken(user);

            if (user.RefreshTokens.Any(x => x.IsActive))
            {
                var activeRefreshToken = user.RefreshTokens.FirstOrDefault(x => x.IsActive);

                authenticationUser.RefreshToken = activeRefreshToken.Token;
                authenticationUser.RefreshTokenExpiration = activeRefreshToken.ExpiredOn;
            }
            else
            {
                var newRefreshToken = GenerateRefreshToken();

                authenticationUser.RefreshToken = newRefreshToken.Token;
                authenticationUser.RefreshTokenExpiration = newRefreshToken.ExpiredOn;
                user.RefreshTokens.Add(newRefreshToken);
                await _userManager.UpdateAsync(user);
            }
            return authenticationUser;
        }

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The user for whom the token is being generated.</param>
        /// <returns>An <see cref="AuthenticationResponse"/> containing the JWT token and associated information.</returns>
        public async Task<AuthenticationResponse> GenerateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id.ToString())
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(_jwt.DurationInDays),
                signingCredentials: signingCredentials);

            return new AuthenticationResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Email = user.Email,
                Username = user.UserName,
                Roles = roles.ToList(),
                Message = "Success Operation",
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(_jwt.DurationInDays),
                IsAuthenticated = true
            };
        }

        /// <summary>
        /// Adds a role to a user based on the provided role and user information.
        /// </summary>
        /// <param name="model">The data transfer object containing the role name and user ID.</param>
        /// <returns>A string message indicating the result of the operation, or null if successful.</returns>
        public async Task<string> AddRoleToUserAsync(AddRoleDTO model)
        {
            var role = await _roleManager.FindByNameAsync(model.RoleName);
            if (role == null)
            {
                var createRoleResult = await _roleManager.CreateAsync(new ApplicationRole { Name = model.RoleName });
                if (!createRoleResult.Succeeded)
                    return string.Join(", ", createRoleResult.Errors.Select(e => e.Description));
            }

            var user = await _userManager.FindByIdAsync(model.UserID.ToString());
            if (user == null)
                return "User not found.";

            if (await _userManager.IsInRoleAsync(user, model.RoleName))
                return "User is already assigned to this role.";

            var addRoleResult = await _userManager.AddToRoleAsync(user, model.RoleName);
            if (!addRoleResult.Succeeded)
                return string.Join(", ", addRoleResult.Errors.Select(e => e.Description));

            return null;
        }

        /// <summary>
        /// Logs in a user with the provided credentials.
        /// </summary>
        /// <param name="loginDTO">Data transfer object containing the user's login credentials.</param>
        /// <returns>An <see cref="AuthenticationResponse"/> containing the result of the login process.</returns>
        public async Task<AuthenticationResponse> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByNameAsync(loginDTO.UserName);

            if (user is null || !await _userManager.CheckPasswordAsync(user, loginDTO.Password))
            {
                return new AuthenticationResponse() { Message = "Email or Password is incorrect!" };
            }

            var authenticationUser = await GenerateJwtToken(user);

            if (user.RefreshTokens.Any(x => x.IsActive))
            {
                var activeRefreshToken = user.RefreshTokens.FirstOrDefault(x => x.IsActive);

                authenticationUser.RefreshToken = activeRefreshToken.Token;
                authenticationUser.RefreshTokenExpiration = activeRefreshToken.ExpiredOn;
            }
            else
            {
                var newRefreshToken = GenerateRefreshToken();

                authenticationUser.RefreshToken = newRefreshToken.Token;
                authenticationUser.RefreshTokenExpiration = newRefreshToken.ExpiredOn;
                user.RefreshTokens.Add(newRefreshToken);
                await _userManager.UpdateAsync(user);
            }
            return authenticationUser;
        }

        /// <summary>
        /// Creates an error response based on a collection of identity errors.
        /// </summary>
        /// <param name="errors">The collection of identity errors.</param>
        /// <returns>An <see cref="AuthenticationResponse"/> containing the error messages.</returns>
        private AuthenticationResponse CreateErrorResponse(IEnumerable<IdentityError> errors)
        {
            var errorMessages = string.Join(", ", errors.Select(e => e.Description));
            return new AuthenticationResponse { Message = errorMessages };
        }

        /// <summary>
        /// Generates a new refresh token.
        /// </summary>
        /// <returns>A <see cref="RefreshToken"/> containing the new refresh token.</returns>
        private RefreshToken GenerateRefreshToken()
        {
            byte[] bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            return new RefreshToken()
            {
                CreatedOn = DateTime.UtcNow,
                ExpiredOn = DateTime.UtcNow.AddDays(10),
                Token = Convert.ToBase64String(bytes)
            };
        }

        /// <summary>
        /// Refreshes the JWT token using a provided refresh token.
        /// </summary>
        /// <param name="token">The refresh token to use for generating a new JWT token.</param>
        /// <returns>An <see cref="AuthenticationResponse"/> containing the new JWT token and associated information.</returns>
        public async Task<AuthenticationResponse> RefreshTokenAsync(string token)
        {
            var user = _userManager.Users
                 .SingleOrDefault(x => x.RefreshTokens.Any(rt => rt.Token == token));

            if (user == null)
            {
                return new AuthenticationResponse() { Message = "Invalid token" };
            }

            var refreshToken = user.RefreshTokens.Single(rt => rt.Token == token);

            if (!refreshToken.IsActive)
            {
                return new AuthenticationResponse() { Message = "Inactive token" };
            }

            refreshToken.RevokedOn = DateTime.UtcNow;

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);

            await _userManager.UpdateAsync(user);

            var authenticationUser = await GenerateJwtToken(user);

            authenticationUser.RefreshToken = newRefreshToken.Token;
            authenticationUser.RefreshTokenExpiration = newRefreshToken.ExpiredOn;

            return authenticationUser;
        }

        /// <summary>
        /// Revokes a refresh token, making it inactive.
        /// </summary>
        /// <param name="token">The refresh token to revoke.</param>
        /// <returns>A boolean indicating whether the token was successfully revoked.</returns>
        public async Task<bool> RevokeTokenAsync(string token)
        {
            var user = _userManager.Users
                .SingleOrDefault(x => x.RefreshTokens.Any(x => x.Token == token));

            if (user == null)
                return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive)
                return false;

            refreshToken.RevokedOn = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return true;
        }
    }
}
