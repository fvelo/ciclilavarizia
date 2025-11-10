using System.ComponentModel.DataAnnotations;

namespace Models.Models.Dtos
{
    public class CredentialsDto
    {
        public long CustomerId { get; set; }
        public string EmailAddress { get; set; }
        public string PasswordHash { get; set; }
    }
}
