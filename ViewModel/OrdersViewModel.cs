using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.ViewModel
{
    public class OrdersViewModel
    {
        public List<Order> Orders { get; set; } = new List<Order>();
        public int TotalOrders { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SortBy { get; set; } = "newest";
        public List<string> OrderStatuses { get; set; } = new List<string>();

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
