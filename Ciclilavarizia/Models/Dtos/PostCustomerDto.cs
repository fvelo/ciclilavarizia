using System.ComponentModel.DataAnnotations;

namespace Ciclilavarizia.Models.Dtos
{
    public class PostCustomerDto
    {
        public int CustomerId { get; set; }
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        [StringLength(50)]
        public string EmailAddress { get; set; } = string.Empty;
        [StringLength(20)]
        public string PlainPassword { get; set; } = string.Empty;
        [StringLength(20)]
        public string Role { get; set; } = "User";
    }
}
