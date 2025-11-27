using System.ComponentModel.DataAnnotations;

namespace Ciclilavarizia.Models.Dtos
{
    public class CredentialsDto
    {
        public string EmailAddress { get; set; }
        public string PlainPassword { get; set; }
    }
}
