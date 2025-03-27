using System;
using System.Collections.Generic;
using WebApplication1.Models;

namespace WebApplication1.ViewModel
{
    public class DashboardViewModel
    {
        public string Period { get; set; } = "week";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public OrderStatisticsViewModel Statistics { get; set; } = new OrderStatisticsViewModel();
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<OrderStatusCountViewModel> OrderStatusDistribution { get; set; } = new List<OrderStatusCountViewModel>();
        public List<DailySalesViewModel> SalesTrend { get; set; } = new List<DailySalesViewModel>();
    }

    public class OrderStatisticsViewModel
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal SalesGrowth { get; set; }
        public decimal OrderGrowth { get; set; }
    }

    public class OrderStatusCountViewModel
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }

    public class DailySalesViewModel
    {
        public DateTime Date { get; set; }
        public int? WeekNumber { get; set; }
        public decimal Sales { get; set; }
        public int OrderCount { get; set; }
    }
}