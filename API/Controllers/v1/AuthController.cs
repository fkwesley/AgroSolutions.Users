using API.Models;
using Application.DTO.Auth;
using Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = Application.DTO.Auth.LoginRequest;

namespace API.Controllers.v1
{
    /// <summary>
    /// Authentication Controller V1
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/auth")]
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public AuthController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        /// <summary>
        /// Returns a JWT token for authentication.
        /// </summary>
        /// <returns>JWT Token</returns>
        [HttpPost("Login")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest login)
        {
            var user = await _userService.ValidateCredentialsAsync(login.UserId, login.Password);

            if (user == null)
                return Unauthorized(new { Error = "User or password invalid." });

            var token = await _authService.GenerateTokenAsync(user);

            return Ok(token);
        }
    }
}
