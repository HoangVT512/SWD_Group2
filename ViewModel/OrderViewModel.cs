using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.ViewModel
{
    public class OrderViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "Số điện thoại liên hệ là bắt buộc")]
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneContact { get; set; }

        public string? Notes { get; set; }

        public string PaymentMethod { get; set; }

        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
    }

    public class OrderItemViewModel
    {
        public int VariantId { get; set; }
        public string VariantName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? Discount { get; set; }
    }
}
