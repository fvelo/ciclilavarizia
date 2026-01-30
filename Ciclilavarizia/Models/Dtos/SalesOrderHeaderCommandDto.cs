using Ciclilavarizia.Models.Dtos;
using System.ComponentModel.DataAnnotations;

namespace Ciclilavarizia.Models.Dtos
{
    // Secure Input for Header
    public class SalesOrderHeaderCommandDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "A valid CustomerID is required.")]
        public int CustomerId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "A valid SalesOrderID is required.")]
        public int SalesOrderId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string ShipMethod { get; set; } = string.Empty;

        [StringLength(25)]
        public string? PurchaseOrderNumber { get; set; }

        [StringLength(128)]
        public string? Comment { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "An order must contain at least one detail line.")]
        public List<SalesOrderDetailCommandDto> SalesOrderDetails { get; set; } = new();
    }
}