using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using SocialMediaApp.Core.Domain.IdentityEntites;
using SocialMediaApp.Core.DTO;
using SocialMediaApp.Core.ServicesContract;
using SocialMediaApp.Infrastructure.Data;

namespace SocialMediaApp.API.Controllers
{
    /// <summary>
    /// Controller for managing user account operations such as registration, login, role assignment, token refreshing, and token revocation.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthenticationServices _authenticationServices;
        private readonly IMailingService _mailingService;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="authenticationServices">The authentication services.</param>
        public AccountController(IAuthenticationServices authenticationServices , IMailingService mailingService,UserManager<ApplicationUser> userManager )
        {
            _authenticationServices = authenticationServices;
            _mailingService = mailingService;
            _userManager = userManager;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="registerDTO">The registration data transfer object containing user details.</param>
        /// <returns>An <see cref="ActionResult"/> containing the authentication response.</returns>
        [HttpPost("register")]
        public async Task<ActionResult<AuthenticationResponse>> RegisterAsync([FromBody] RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("|", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return BadRequest(errors);
            }

            var result = await _authenticationServices.RegisterAsync(registerDTO);
            if (!result.IsAuthenticated)
                return Problem(result.Message);

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshToken(result.RefreshToken, result.RefreshTokenExpiration);
            }

            return Ok(result);
        }


        /// <summary>
        /// Logs in an existing user.
        /// </summary>
        /// <param name="loginDTO">The login data transfer object containing user credentials.</param>
        /// <returns>An <see cref="ActionResult"/> containing the authentication response.</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> LoginAsync([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("|", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                return BadRequest(errors);
            }

            var result = await _authenticationServices.LoginAsync(loginDTO);
            if (!result.IsAuthenticated)
                return Problem(result.Message);

            // Only set the refresh token if it is not null or empty
            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshToken(result.RefreshToken, result.RefreshTokenExpiration);
            }
            return Ok(result);
        }


        /// <summary>
        /// Adds a role to an existing user.
        /// </summary>
        /// <param name="model">The data transfer object containing role and user information.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPost("addrole")]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authenticationServices.AddRoleToUserAsync(model);

            if (!string.IsNullOrEmpty(result))
                return BadRequest(result);

            return Ok(model);
        }

        /// <summary>
        /// Refreshes the JWT token using a valid refresh token.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the new authentication response.</returns>
        [HttpGet("refreshtoken")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest("Refresh token is missing or invalid.");

            var result = await _authenticationServices.RefreshTokenAsync(refreshToken);
            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            SetRefreshToken(result.RefreshToken, result.RefreshTokenExpiration);
            return Ok(result);
        }

        /// <summary>
        /// Revokes a specific refresh token or the one stored in the request cookies.
        /// </summary>
        /// <param name="revokTokenDTO">The data transfer object containing the token to revoke.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the revocation.</returns>
        [HttpPost("revokeToken")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokTokenDTO revokTokenDTO)
        {
            var token = revokTokenDTO.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required!");

            var result = await _authenticationServices.RevokeTokenAsync(token);

            if (!result)
                return BadRequest("Invalid token");

            return Ok();
        }
        /// <summary>
        /// Initiates the forgot password process by generating a reset token and sending it to the user's email.
        /// </summary>
        /// <param name="forgotPassword">The data transfer object containing the user's email and client URI.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns 200 OK if the email is sent successfully, otherwise returns 400 BadRequest.</returns>
        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPassword)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByEmailAsync(forgotPassword.Email!);
            if (user == null)
                return BadRequest("Invalid Request");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var param = new Dictionary<string, string?>
            {
                {"token", token},
                {"email", forgotPassword.Email}
            };
            var callback = QueryHelpers.AddQueryString(forgotPassword.ClientUri!, param);

            await _mailingService.SendMessageAsync(user.Email!, "Reset password", callback, null);

            return Ok();
        }

        /// <summary>
        /// Resets the user's password using the provided reset token and new password.
        /// </summary>
        /// <param name="resetPassword">The data transfer object containing the user's email, reset token, and new password.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns 200 OK if the password is reset successfully, otherwise returns 400 BadRequest with the errors.</returns>
        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPassword)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (user == null)
                return BadRequest("Invalid Request");

            var result = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password!);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(x => x.Description);
                return BadRequest(new { Errors = errors });
            }
            return Ok();
        }

        /// <summary>
        /// Sets the refresh token in an HTTP-only cookie.
        /// </summary>
        /// <param name="refreshToken">The refresh token to store.</param>
        /// <param name="expires">The expiration date and time of the refresh token.</param>
        private void SetRefreshToken(string refreshToken, DateTime expires)
        {
            var cookieOption = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = expires
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOption);
        }
    }
}
