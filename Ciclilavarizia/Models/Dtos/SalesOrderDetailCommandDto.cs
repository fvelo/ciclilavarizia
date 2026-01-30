using System.ComponentModel.DataAnnotations;

namespace Ciclilavarizia.Models.Dtos
{
    // Secure Input for creating/updating details
    public class SalesOrderDetailCommandDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProductID must be a positive integer.")]
        public int ProductId { get; set; }

        [Required]
        [Range(1, short.MaxValue, ErrorMessage = "Quantity must be between 1 and 32767.")]
        public short OrderQty { get; set; }

        [Required]
        [Range(0.00, 1.00, ErrorMessage = "Discount must be a decimal between 0 and 1.")]
        public decimal UnitPriceDiscount { get; set; }
    }
}