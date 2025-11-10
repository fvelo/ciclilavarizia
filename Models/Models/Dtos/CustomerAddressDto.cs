namespace Models.Models.Dtos
{
    public class CustomerAddressDto
    {
        public int AddressId { get; set; }
        public string AddressType { get; set; } = null!;
        public virtual AddressDto Address { get; set; } = null!;
    }
}