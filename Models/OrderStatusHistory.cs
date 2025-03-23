using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class OrderStatusHistory
{
    public int HistoryId { get; set; }

    public int OrderId { get; set; }

    public string Status { get; set; } = null!;

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Notes { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual User? UpdatedByNavigation { get; set; }
}
