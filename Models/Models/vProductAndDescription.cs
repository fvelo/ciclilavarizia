using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Models;

[Keyless]
public partial class vProductAndDescription
{
    public int ProductID { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    public string ProductModel { get; set; } = null!;

    [StringLength(6)]
    public string Culture { get; set; } = null!;

    [StringLength(400)]
    public string Description { get; set; } = null!;
}
