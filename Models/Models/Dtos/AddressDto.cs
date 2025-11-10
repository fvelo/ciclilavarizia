namespace Models.Models.Dtos
{
    public class AddressDto
    {
        public string AddressLine1 { get; set; } = null!;
        public string City { get; set; } = null!;
        public string StateProvince { get; set; } = null!;
        public string CountryRegion { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
    }
}