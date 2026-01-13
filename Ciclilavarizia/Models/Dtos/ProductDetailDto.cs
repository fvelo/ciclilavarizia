using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ciclilavarizia.Models.Dtos
{
    public class ProductDetailDto
    {
        public int ProductId { get; set; }
        [StringLength(50)]
        public string Name { get; set; } = null!;
        [StringLength(25)]
        public string ProductNumber { get; set; } = null!;
        [StringLength(15)]
        public string? Color { get; set; }
        [Column(TypeName = "money")]
        public decimal StandardCost { get; set; }
        [Column(TypeName = "money")]
        public decimal ListPrice { get; set; }
        [StringLength(5)]
        public string? Size { get; set; }
        [Column(TypeName = "decimal(8, 2)")]
        public decimal? Weight { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime SellStartDate { get; set; } 
        [Column(TypeName = "xml")]
        public string? CatalogDescription { get; set; }
        [StringLength(6)]
        public string? Culture { get; set; }
        [StringLength(400)]
        public string? Description { get; set; }
        public string? ProductCategory { get; set; }
        public string? ProductModel { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}