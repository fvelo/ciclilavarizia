using System.ComponentModel.DataAnnotations;

namespace Ciclilavarizia.Models.Dtos
{
    public class ProductModelDto
    {
        public int ProductModelId { get; set; }
        [StringLength(50)]
        public string Name { get; set; } = null!;
        public string? CatalogDescription { get; set; }
    }
}