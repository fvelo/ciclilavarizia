using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ciclilavarizia.Models.Dtos
{
    public class ProductDto
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
        public virtual ProductModelDto? ProductModel { get; set; }

    }
}