using System.ComponentModel.DataAnnotations;

namespace Ciclilavarizia.Models.Dtos
{
    public class CustomerAddressDto
    {
        public int AddressId { get; set; }
        [StringLength(50)]
        public string AddressType { get; set; } = null!;
        public virtual AddressDto Address { get; set; } = null!;
    }
}