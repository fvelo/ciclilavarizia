using DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Models.Models.Dtos;


namespace WebBetacomeDbFirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        public DbSecure _dbSecure { get; set; }
        public LoginController(DbSecure dbSecure)
        {
            _dbSecure = dbSecure;
        }
        [HttpPost]
        public async Task<IActionResult> IsRegistered([FromBody] CredentialsDto credentials)
        {
            if (credentials == null) return NotFound();
            string email = credentials.EmailAddress;
            string password = credentials.PasswordHash;

            email = email ?? string.Empty;
            email = email.ToLower().Replace(" ", "");
            bool isThereAUSer = await _dbSecure.FindUserByEmail(email);
            if (!isThereAUSer) return NotFound();
            return NoContent();
        }
    }
}
