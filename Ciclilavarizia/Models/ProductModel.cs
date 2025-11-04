using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ciclilavarizia.Models;

[Table("ProductModel", Schema = "SalesLT")]
[Index("Name", Name = "AK_ProductModel_Name", IsUnique = true)]
[Index("rowguid", Name = "AK_ProductModel_rowguid", IsUnique = true)]
[Index("CatalogDescription", Name = "PXML_ProductModel_CatalogDescription")]
public partial class ProductModel
{
    [Key]
    public int ProductModelID { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column(TypeName = "xml")]
    public string? CatalogDescription { get; set; }

    public Guid rowguid { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ModifiedDate { get; set; }

    [InverseProperty("ProductModel")]
    public virtual ICollection<ProductModelProductDescription> ProductModelProductDescriptions { get; set; } = new List<ProductModelProductDescription>();

    [InverseProperty("ProductModel")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
