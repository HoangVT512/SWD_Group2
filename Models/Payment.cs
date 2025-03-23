using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public DateTime? PaymentDate { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public string? TransactionId { get; set; }

    public decimal Amount { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
