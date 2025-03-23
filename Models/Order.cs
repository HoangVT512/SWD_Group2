using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public string ShippingAddress { get; set; } = null!;

    public string PhoneContact { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string? Status { get; set; }

    public int? PaymentId { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    public virtual Payment? Payment { get; set; }

    public virtual User? User { get; set; }
}
