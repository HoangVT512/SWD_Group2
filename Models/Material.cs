using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Material
{
    public int MaterialId { get; set; }

    public string MaterialName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
}
