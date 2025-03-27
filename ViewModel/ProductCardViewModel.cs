using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.ViewModel
{
    public class ProductCardViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountedPrice { get; set; }
        public string ImageUrl { get; set; }
    }
}

