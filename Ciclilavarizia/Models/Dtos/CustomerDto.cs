using System.Xml.Serialization;

namespace Ciclilavarizia.Models.Dtos
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string? Title { get; set; }
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = null!;
        public string? Suffix { get; set; }
        public string? CompanyName { get; set; }
        public string? SalesPerson { get; set; }
        [XmlArray("CustomerAddresses")]
        [XmlArrayItem("CustomerAddress")]
        public virtual List<CustomerAddressDto> CustomerAddresses { get; set; } = new List<CustomerAddressDto>();
        //public virtual ICollection<CustomerAddressDto> CustomerAddresses { get; set; } = new List<CustomerAddressDto>();
        //public virtual ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; } = new List<SalesOrderHeader>();
    }
}