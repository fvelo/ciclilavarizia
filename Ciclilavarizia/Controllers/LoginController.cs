using DataAccessLayer;
using Microsoft.AspNetCore.Mvc;

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

        public record LoginRecord(string email, string password);
        [HttpPost]
        public async Task<IActionResult> IsRegistered([FromBody] LoginRecord loginRecord)
        {
            if (loginRecord == null) return NotFound();
            string email = loginRecord.email;
            string password = loginRecord.password;

            email = email ?? string.Empty;
            email = email.ToLower().Replace(" ", "");
            bool isThereAUSer = await _dbSecure.FindUserByEmail(email);
            if (!isThereAUSer) return NotFound();
            return NoContent();
        }
    }
}
