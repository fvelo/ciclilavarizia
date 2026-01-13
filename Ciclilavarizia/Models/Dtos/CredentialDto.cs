using System.ComponentModel.DataAnnotations;

namespace Ciclilavarizia.Models.Dtos
{
    public class CredentialDto
    {
        [StringLength(50)]
        public string EmailAddress { get; set; }
        [StringLength(20)]
        public string PlainPassword { get; set; }
    }
}
