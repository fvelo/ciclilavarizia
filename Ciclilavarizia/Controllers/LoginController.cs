using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Models.Settings;
using Ciclilavarizia.Services;
using DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


namespace Ciclilavarizia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly SecureDbService _secureDb;
        private readonly IOptionsMonitor<JwtSettings> _jwtSettingsMonitor;
        private readonly LoginService _loginService;


        public LoginController(SecureDbService secureDb, IOptionsMonitor<JwtSettings> jwtSettingsMonitor, LoginService loginService)
        {
            _secureDb = secureDb;
            _jwtSettingsMonitor = jwtSettingsMonitor;
            _loginService = loginService;
        }

        /// <summary>
        /// Authenticates a user and generates a JWT access token.
        /// </summary>
        /// <param name="credentials">The user's email and password.</param>
        /// <returns>A JSON object containing the Bearer token.</returns>
        /// <response code="200">Authentication successful; returns the token.</response>
        /// <response code="400">If the request payload is malformed.</response>
        /// <response code="401">If the email or password is incorrect.</response>
        [HttpPost]
        public async Task<IActionResult> Login(CredentialDto credentials)
        {
            if (credentials == null || string.IsNullOrEmpty(credentials.EmailAddress))
                return BadRequest("Credentials required.");

            var userResult = await _loginService.ValidateUserAsync(credentials);

            if (!userResult.IsSuccess) return BadRequest(userResult.ErrorMessage);

            var user = userResult.Value;

            if (user == null)
            {
                // I will not tell if the email or the password is the problem, for security reasons 
                return Unauthorized("Invalid email or password.");
            }

            var token = _loginService.GenerateJwtTokenAsync(user.Email, user.Role, user.CustomerId);

            return Ok(new { Token = token });
        }
    }
}
