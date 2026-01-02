namespace Ciclilavarizia.Models.Dtos
{
    public class PostCustomerDto
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string PlainPassword { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }
}
