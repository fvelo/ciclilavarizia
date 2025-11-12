using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Models.Settings;
using DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Ciclilavarizia.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        public DbSecure _dbSecure { get; set; }
        private readonly IOptionsMonitor<JwtSettings> _jwtSettingsMonitor;

        public LoginController(DbSecure dbSecure, IOptionsMonitor<JwtSettings> jwtSettingsMonitor)
        {
            _dbSecure = dbSecure;
            _jwtSettingsMonitor = jwtSettingsMonitor;
        }

        [HttpPost]
        public async Task<IActionResult> IsRegistered([FromBody] CredentialsDto credentials)
        {
            if (credentials == null) return NotFound();
            string email = credentials.EmailAddress;
            //string password = credentials.PasswordHash; // da capire come fare

            email = email ?? string.Empty;
            email = email.ToLower().Replace(" ", "");
            bool isThereAUSer = await _dbSecure.FindUserByEmail(email);
            if (!isThereAUSer) return NotFound();
            var role = "admin";

            var jwtToken = GenerateJwtToken(credentials, role.ToLower());
            Response.Cookies.Append("FlashMessage", "Sì il cannone", new CookieOptions
            {
                Secure = true
            });
            Console.WriteLine($"Dentro IsRegistered ${email} as entered!");
            //return Ok(jwtToken);
            return Ok(new { token = jwtToken });
        }

        private string GenerateJwtToken(CredentialsDto credentials, string role)
        {
            var jwtSettings = _jwtSettingsMonitor.CurrentValue;
            var secretKey = jwtSettings.SecretKey;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            role = role.ToLower();

            var tokenDescrition = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(ClaimTypes.Name, credentials.EmailAddress),
                    new Claim(ClaimTypes.Role, role),
                    
                    //new Claim(ClaimTypes.Role, "Admin"),
                    //new Claim(ClaimTypes.Role, "Customer"),
                    //new Claim(ClaimTypes.Role, "MagazineManager"),

                    ]),
                Expires = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes),
                Issuer = jwtSettings.Issuer,
                Audience = jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescrition);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
            //return tokenHandler.CreateToken(tokenDescrition);
        }
    }
}
