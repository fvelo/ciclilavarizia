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
        // TODO: Add an actual error logging for Problem() response in every ActionMethod
        // TODO: Implement the login controller for real and not FAKEEEEE

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
        public async Task<IActionResult> IsRegistered([FromBody] CredentialDto credentials)
        {
            if (credentials == null) return NotFound();
            string email = credentials.EmailAddress;
            //string password = credentials.PasswordHash; // da capire come fare

            email = email ?? string.Empty;
            email = email.ToLower().Replace(" ", "");
            bool isThereAUSer = await _secureDb.DoesCredentialExistsByEmail(email);
            if (!isThereAUSer) return NotFound();
            var role = "admin";

            var jwtToken = _loginService.GenerateJwtTokenAsync(credentials, role.ToLower());
            //Response.Cookies.Append("FlashMessage", "Sì il cannone", new CookieOptions
            //{
            //    Secure = true
            //});
            //Console.WriteLine($"Dentro IsRegistered ${email} as entered!");
            //return Ok(jwtToken);
            return Ok(new { token = jwtToken });
        }
    }
}
