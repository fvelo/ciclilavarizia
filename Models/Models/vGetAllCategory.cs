using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Models;

[Keyless]
public partial class vGetAllCategory
{
    [StringLength(50)]
    public string ParentProductCategoryName { get; set; } = null!;

    [StringLength(50)]
    public string? ProductCategoryName { get; set; }

    public int? ProductCategoryID { get; set; }
}
