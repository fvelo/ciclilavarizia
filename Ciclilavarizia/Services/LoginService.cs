using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;
using Ciclilavarizia.Models.Settings;
using Ciclilavarizia.Services.Interfaces;
using CommonCiclilavarizia;
using DataAccessLayer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ciclilavarizia.Services
{
    public class LoginService : ILoginService
    {
        public SecureDbService _secureDb { get; set; }
        private readonly IOptionsMonitor<JwtSettings> _jwtSettingsMonitor;
        private readonly Encryption _encryptionHandler;

        public LoginService(SecureDbService secureDb, IOptionsMonitor<JwtSettings> jwtSettingsMonitor, Encryption encryptionHandler)
        {
            _secureDb = secureDb;
            _jwtSettingsMonitor = jwtSettingsMonitor;
            _encryptionHandler = encryptionHandler;
        }

        public async Task<Result<UserLoginResultDto?>> ValidateUserAsync(CredentialDto credentials)
        {
            var areCredentialsExpired = await _secureDb.AreCredentialsExpiredAsync(credentials.EmailAddress);

            if (areCredentialsExpired) return Result<UserLoginResultDto?>.Failure("Credentials expired, create new ones!");

            var storedCreds = await _secureDb.GetCredentialsByEmailAsync(credentials.EmailAddress);

            if (storedCreds == null) return Result<UserLoginResultDto?>.Success(null);

            var enteredHash = _encryptionHandler.HashPassword(credentials.PlainPassword, storedCreds.PasswordSalt);

            if (enteredHash != storedCreds.PasswordHash) return Result<UserLoginResultDto?>.Success(null);

            return Result<UserLoginResultDto?>.Success(new UserLoginResultDto
            {
                CustomerId = (int)storedCreds.CustomerId,
                Role = storedCreds.Role,
                Email = storedCreds.EmailAddress
            });
        }

        public string GenerateJwtTokenAsync(string email, string role, int customerId)
        {

            var jwtSettings = _jwtSettingsMonitor.CurrentValue;
            var secretKey = jwtSettings.SecretKey;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescrition = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(ClaimTypes.Name, email.Trim()),
                    new Claim(ClaimTypes.Role, role.ToLower().Trim()),
                    new Claim("CustomerId", $"{customerId}"),
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
        }
    }
}
