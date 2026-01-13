using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Ciclilavarizia.Models.Dtos
{
    public class CustomerDetailDto
    {
        public int CustomerId { get; set; }
        [StringLength(8)]
        public string? Title { get; set; }
        [StringLength(50)]
        public string FirstName { get; set; } = null!;
        [StringLength(50)]
        public string? MiddleName { get; set; }
        [StringLength(50)]
        public string LastName { get; set; } = null!;
        [StringLength(10)]
        public string? Suffix { get; set; }
        [StringLength(128)]
        public string? CompanyName { get; set; }
        [StringLength(256)]
        public string? SalesPerson { get; set; }
        [XmlArray("CustomerAddresses")]
        [XmlArrayItem("CustomerAddress")]
        public virtual List<CustomerAddressDto> CustomerAddresses { get; set; } = new List<CustomerAddressDto>();
        //public virtual ICollection<CustomerAddressDto> CustomerAddresses { get; set; } = new List<CustomerAddressDto>();
        //public virtual ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; } = new List<SalesOrderHeader>();
    }
}