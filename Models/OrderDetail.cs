using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class OrderDetail
{
    public int OrderDetailId { get; set; }

    public int OrderId { get; set; }

    public int VariantId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? Discount { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ProductVariant Variant { get; set; } = null!;
}
