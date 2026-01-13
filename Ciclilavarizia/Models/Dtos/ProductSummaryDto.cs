using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ciclilavarizia.Models.Dtos
{
    public class ProductSummaryDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string ThumbnailUrl { get; set; } = null!;
        public string? ProductCategory { get; set; }
        public string? ProductModel { get; set; }
        [Column(TypeName = "money")]
        public decimal ListPrice { get; set; }
        public string? Color { get; set; }
    }
}
