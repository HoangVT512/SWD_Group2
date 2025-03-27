using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.ViewModel;

namespace WebApplication1.Service
{
    public interface ISaleService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(DateTime? startDate = null, DateTime? endDate = null, string period = "week");
        Task<List<Order>> GetRecentOrdersAsync(int count = 5);
        Task<OrderStatisticsViewModel> GetOrderStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<OrderStatusCountViewModel>> GetOrderStatusDistributionAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<DailySalesViewModel>> GetSalesTrendAsync(DateTime? startDate = null, DateTime? endDate = null, string period = "week");


        // Thêm các phương thức mới cho trang Order
        Task<OrdersViewModel> GetOrdersAsync(
            string searchTerm = null,
            string status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string sortBy = "newest",
            int page = 1,
            int pageSize = 10);

        Task<Order> GetOrderByIdAsync(int id);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status, string notes = null);
        IEnumerable<ExportOrderViewModel> GetOrdersForExport(string status, DateTime? fromDate, DateTime? toDate);

    }
}