using System.ComponentModel.DataAnnotations;

namespace Ciclilavarizia.Models.Dtos
{
    public class CustomerSummaryDto
    {
        public int CustomerId { get; set; }
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        [StringLength(50)]
        public string EmailAddress { get; set; } = string.Empty; // get it from SecureDb
    }
}
