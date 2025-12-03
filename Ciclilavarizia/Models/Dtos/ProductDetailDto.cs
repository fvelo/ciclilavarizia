using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ciclilavarizia.Models.Dtos
{
    public class ProductDetailDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string ProductNumber { get; set; } = null!;
        public string? Color { get; set; }
        [Column(TypeName = "money")]
        public decimal StandardCost { get; set; }
        [Column(TypeName = "money")]
        public decimal ListPrice { get; set; }
        public string? Size { get; set; }
        [Column(TypeName = "decimal(8, 2)")]
        public decimal? Weight { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime SellStartDate { get; set; } 
        //[Column(TypeName = "datetime")]
        //public DateTime? SellEndDate { get; set; } // se esiste non dovresti mostrare l'elementto nel frontend, romane nel backend
        //[Column(TypeName = "datetime")]
        //public DateTime? DiscontinuedDate { get; set; } // uguale, stessa cosa, se esiste non va nel frontend
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
//    "productId": 0,
//    "name": "string",
//    "productNumber": "string",
//    "color": "string",
//    "standardCost": 0,
//    "listPrice": 0,
//    "size": "string",
//    "weight": 0,
//    "sellStartDate": "2025-11-27T09:38:48.541Z",
//    "sellEndDate": "2025-11-27T09:38:48.541Z",
//    "discontinuedDate": "2025-11-27T09:38:48.541Z",
//    "productCategory": "string",
//    "ProductModel": "string",
//    "catalogDescription": "string",
//    "culture": "string",
//    "description": "string"