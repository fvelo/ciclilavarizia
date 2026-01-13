using System.ComponentModel.DataAnnotations;

namespace CommonCiclilavarizia
{
    public class Credentials
    {
        [Required]
        [Key]
        public long CredentialId { get; set; }
        [Required]
        public long CustomerId { get; set; }
        [Required]
        [StringLength(50)]
        public string EmailAddress { get; set; }
        [Required]
        [StringLength(128)]
        public string PasswordHash { get; set; }
        [Required]
        [StringLength(10)]
        public string PasswordSalt { get; set; }
        [Required]
        [StringLength(20)]
        public string Role { get; set; }

        public Credentials() { }

    }
}
