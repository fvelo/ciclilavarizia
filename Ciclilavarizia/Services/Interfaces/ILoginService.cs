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
        /// <returns></returns>
        Task<string> GenerateJwtTokenAsync(CredentialDto credentials, string role);
    }
}
