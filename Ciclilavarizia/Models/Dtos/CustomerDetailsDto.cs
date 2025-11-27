namespace Ciclilavarizia.Models.Dtos
{
    public class CustomerDetailsDto
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty; // get it from SecureDb
    }
}
