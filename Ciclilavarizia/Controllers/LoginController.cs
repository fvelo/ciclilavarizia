using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Models.Settings;
using Ciclilavarizia.Services;
using DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] CredentialDto credentials)
        {
            if (credentials == null || string.IsNullOrEmpty(credentials.EmailAddress))
                return BadRequest("Credentials required.");

            var user = await _loginService.ValidateUserAsync(credentials);

            if (user == null)
            {
                // I will not tell if the email or the password is the problem, for security reasons 
                return Unauthorized("Invalid email or password.");
            }

            var token = _loginService.GenerateJwtTokenAsync(user.Email, user.Role, user.CustomerId);

            return Ok(new { Token = token });
        }

        //[HttpPost]
        //public async Task<IActionResult> IsRegistered([FromBody] CredentialDto credentials)
        //{
        //    if (credentials == null) return BadRequest();
        //    string email = credentials.EmailAddress;
        //    string plainPassword = credentials.PlainPassword; // da capire come fare

        //    email = email ?? string.Empty;
        //    email = email.ToLower().Replace(" ", "");
        //    bool isThereAUSer = await _secureDb.DoesCredentialExistsByEmail(email);
        //    if (!isThereAUSer) return NotFound();
        //    var role = "user";

        //    var customerId = await _secureDb.GetCustomerIdByEmailAddressAsync(email);
        //    if (customerId == null) return BadRequest();

        //    var jwtToken = _loginService.GenerateJwtTokenAsync(credentials, role.ToLower(), (int)customerId);
        //    return Ok(new { token = jwtToken });
        //}
    }
}
