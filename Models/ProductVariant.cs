using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class ProductVariant
{
    public int VariantId { get; set; }

    public int ProductId { get; set; }

    public int SizeId { get; set; }

    public int ColorId { get; set; }

    public int? MaterialId { get; set; }

    public string? Sku { get; set; }

    public int? Stock { get; set; }

    public string? Image { get; set; }

    public decimal? AdditionalPrice { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Color Color { get; set; } = null!;

    public virtual Material? Material { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Product Product { get; set; } = null!;

    public virtual Size Size { get; set; } = null!;
}
