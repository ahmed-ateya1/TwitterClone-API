using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.DTO;
using SocialMediaApp.Core.ServicesContract;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="authenticationServices">The authentication services.</param>
        public AccountController(IAuthenticationServices authenticationServices)
        {
            _authenticationServices = authenticationServices;
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
