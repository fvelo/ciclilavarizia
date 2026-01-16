using Ciclilavarizia.Models;
using Ciclilavarizia.Models.Dtos;

namespace Ciclilavarizia.Services.Interfaces
{
    public interface ILoginService
    {
        /// <summary>
        /// Generate Jwt Token
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="role"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        string GenerateJwtTokenAsync(string email, string role, int customerId);

        Task<Result<UserLoginResultDto?>> ValidateUserAsync(CredentialDto credentials);

    }
}
