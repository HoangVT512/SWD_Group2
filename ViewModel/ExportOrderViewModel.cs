using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.ViewModel
{
    public class ExportOrderViewModel
    {
        // Thông tin chính của đơn hàng
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }

        // Thông tin khách hàng
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string ShippingAddress { get; set; }

        // Thông tin thanh toán
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }

        // Thông tin bổ sung
        public string Notes { get; set; }
        public int ItemCount { get; set; }
    }
}
