using System.ComponentModel.DataAnnotations;

namespace Ciclilavarizia.Models.Dtos
{
    public class AddressDto
    {
        [StringLength(60)]
        public string AddressLine1 { get; set; } = null!;
        [StringLength(30)]
        public string City { get; set; } = null!;
        [StringLength(50)]
        public string StateProvince { get; set; } = null!;
        [StringLength(50)]
        public string CountryRegion { get; set; } = null!;
        [StringLength(15)]
        public string PostalCode { get; set; } = null!;
    }
}