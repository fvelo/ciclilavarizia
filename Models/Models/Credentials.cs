using System.ComponentModel.DataAnnotations;

namespace Models.Models
{
    public class Credentials
    {
        [Required]
        [Key]
        public long CredentialId { get; set; }
        [Required]
        public long CustomerId { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public string PasswordSalt { get; set; }

        public Credentials() { }

    }
}
