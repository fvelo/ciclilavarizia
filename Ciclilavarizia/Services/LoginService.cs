using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Models.Settings;
using Ciclilavarizia.Services.Interfaces;
using DataAccessLayer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ciclilavarizia.Services
{
    public class LoginService: ILoginService
    {
        public SecureDbService _secureDb { get; set; }
        private readonly IOptionsMonitor<JwtSettings> _jwtSettingsMonitor;

        public LoginService(SecureDbService secureDb, IOptionsMonitor<JwtSettings> jwtSettingsMonitor)
        {
            _secureDb = secureDb;
            _jwtSettingsMonitor = jwtSettingsMonitor;
        }

        public async Task<string> GenerateJwtTokenAsync(CredentialDto credentials, string role)
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
