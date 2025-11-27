using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ciclilavarizia.Models.Dtos
{
    public class ProductDetails
    {
        public int ProductId { get; set; }

        public string name { get; set; }
        
        public string ProductNumber { get; set; }
        
        public string Color { get; set; }
        
        //public int StandardCost { get; set; }
        
        public int ListPrice { get; set; }
        
        public string Size { get; set; }
        
        public int Weight { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime SellStartDate { get; set; } 
        
        [Column(TypeName = "datetime")]
        public DateTime? SellEndDate { get; set; } // se esiste non dovresti mostrare l'elementto nel frontend, romane nel backend
        
        //[Column(TypeName = "datetime")]
        //public DateTime? DiscontinuedDate { get; set; } // uguale, stessa cosa, se esiste non va nel front end

        [Column(TypeName = "xml")]
        public string? CatalogDescription { get; set; }

        [StringLength(6)]
        public string Culture { get; set; } = null!;

        [StringLength(400)]
        public string Description { get; set; } = null!;
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