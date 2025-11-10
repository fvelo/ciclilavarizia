using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ciclilavarizia.Models;

/// <summary>
/// Cross-reference table mapping product descriptions and the language the description is written in.
/// </summary>
[PrimaryKey("ProductModelID", "ProductDescriptionID", "Culture")]
[Table("ProductModelProductDescription", Schema = "SalesLT")]
[Index("rowguid", Name = "AK_ProductModelProductDescription_rowguid", IsUnique = true)]
public partial class ProductModelProductDescription
{
    /// <summary>
    /// Primary key. Foreign key to ProductModel.ProductModelID.
    /// </summary>
    [Key]
    public int ProductModelID { get; set; }

    /// <summary>
    /// Primary key. Foreign key to ProductDescription.ProductDescriptionID.
    /// </summary>
    [Key]
    public int ProductDescriptionID { get; set; }

    /// <summary>
    /// The culture for which the description is written
    /// </summary>
    [Key]
    [StringLength(6)]
    public string Culture { get; set; } = null!;

    public Guid rowguid { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime ModifiedDate { get; set; }

    [ForeignKey("ProductDescriptionID")]
    [InverseProperty("ProductModelProductDescriptions")]
    public virtual ProductDescription ProductDescription { get; set; } = null!;

    [ForeignKey("ProductModelID")]
    [InverseProperty("ProductModelProductDescriptions")]
    public virtual ProductModel ProductModel { get; set; } = null!;
}
