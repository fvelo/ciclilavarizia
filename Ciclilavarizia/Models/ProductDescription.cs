using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ciclilavarizia.Models;

/// <summary>
/// Product descriptions in several languages.
/// </summary>
[Table("ProductDescription", Schema = "SalesLT")]
[Index("rowguid", Name = "AK_ProductDescription_rowguid", IsUnique = true)]
public partial class ProductDescription
{
    /// <summary>
    /// Primary key for ProductDescription records.
    /// </summary>
    [Key]
    public int ProductDescriptionID { get; set; }

    /// <summary>
    /// Description of the product.
    /// </summary>
    [StringLength(400)]
    public string Description { get; set; } = null!;

    /// <summary>
    /// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
    /// </summary>
    public Guid rowguid { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime ModifiedDate { get; set; }

    [InverseProperty("ProductDescription")]
    public virtual ICollection<ProductModelProductDescription> ProductModelProductDescriptions { get; set; } = new List<ProductModelProductDescription>();
}
