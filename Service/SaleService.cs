using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.ViewModel;

namespace WebApplication1.Service
{
    public class SaleService : ISaleService
    {
        private readonly ClothingShopDbContext _context;

        public SaleService(ClothingShopDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(DateTime? startDate = null, DateTime? endDate = null, string period = "week")
        {
            // Set default dates if not provided
            if (startDate == null || endDate == null)
            {
                if (period.ToLower() == "week")
                {
                    endDate = DateTime.Now;
                    startDate = endDate.Value.AddDays(-7);
                }
                else if (period.ToLower() == "month")
                {
                    endDate = DateTime.Now;
                    startDate = endDate.Value.AddMonths(-1);
                }
                else
                {
                    endDate = DateTime.Now;
                    startDate = endDate.Value.AddDays(-30);
                }
            }

            var dashboardViewModel = new DashboardViewModel
            {
                Period = period,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Statistics = await GetOrderStatisticsAsync(startDate, endDate),
                RecentOrders = await GetRecentOrdersAsync(),
                OrderStatusDistribution = await GetOrderStatusDistributionAsync(startDate, endDate),
                SalesTrend = await GetSalesTrendAsync(startDate, endDate, period)
            };

            return dashboardViewModel;
        }

        public async Task<List<Order>> GetRecentOrdersAsync(int count = 5)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Variant)
                        .ThenInclude(v => v.Product)
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<OrderStatisticsViewModel> GetOrderStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate <= endDate.Value.AddDays(1));

            var completedOrders = await query.Where(o => o.Status == "Completed").ToListAsync();
            var pendingOrders = await query.Where(o => o.Status == "Pending" || o.Status == "Processing").ToListAsync();
            var cancelledOrders = await query.Where(o => o.Status == "Cancelled").ToListAsync();

            // Calculate previous period for comparison
            var currentPeriodDays = (endDate.Value - startDate.Value).TotalDays;
            var previousStartDate = startDate.Value.AddDays(-currentPeriodDays);
            var previousEndDate = startDate.Value.AddDays(-1);

            var previousQuery = _context.Orders.Where(o =>
                o.OrderDate >= previousStartDate &&
                o.OrderDate <= previousEndDate);

            var previousTotalSales = await previousQuery.SumAsync(o => o.TotalAmount);
            var previousOrderCount = await previousQuery.CountAsync();

            var totalSales = await query.SumAsync(o => o.TotalAmount);
            var totalOrders = await query.CountAsync();

            // Calculate growth percentages
            decimal salesGrowth = previousTotalSales > 0
                ? (totalSales - previousTotalSales) / previousTotalSales * 100
                : 100;

            decimal orderGrowth = previousOrderCount > 0
                ? ((decimal)totalOrders - previousOrderCount) / previousOrderCount * 100
                : 100;

            return new OrderStatisticsViewModel
            {
                TotalSales = totalSales,
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders.Count,
                PendingOrders = pendingOrders.Count,
                CancelledOrders = cancelledOrders.Count,
                AverageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0,
                SalesGrowth = salesGrowth,
                OrderGrowth = orderGrowth
            };
        }

        public async Task<List<OrderStatusCountViewModel>> GetOrderStatusDistributionAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate <= endDate.Value.AddDays(1));

            var statusCounts = await query
                .GroupBy(o => o.Status)
                .Select(g => new OrderStatusCountViewModel
                {
                    Status = g.Key ?? "Unknown",
                    Count = g.Count()
                })
                .ToListAsync();

            return statusCounts;
        }

        public async Task<List<DailySalesViewModel>> GetSalesTrendAsync(DateTime? startDate = null, DateTime? endDate = null, string period = "week")
        {
            var query = _context.Orders.Where(o => o.OrderDate.HasValue);

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate <= endDate.Value.AddDays(1));

            var orders = await query.ToListAsync();

            var salesTrend = new List<DailySalesViewModel>();

            if (period.ToLower() == "week")
            {
                // Group by day for week view
                salesTrend = orders
                    .GroupBy(o => o.OrderDate.Value.Date)
                    .Select(g => new DailySalesViewModel
                    {
                        Date = g.Key,
                        Sales = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
            }
            else if (period.ToLower() == "month")
            {
                // Group by day but might want to consider grouping by week for better visualization
                salesTrend = orders
                    .GroupBy(o => new { Year = o.OrderDate.Value.Year, Week = GetIso8601WeekOfYear(o.OrderDate.Value) })
                    .Select(g => new DailySalesViewModel
                    {
                        Date = FirstDateOfWeek(g.Key.Year, g.Key.Week),
                        WeekNumber = g.Key.Week,
                        Sales = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
            }

            return salesTrend;
        }

        // Helper methods for date calculations
        private int GetIso8601WeekOfYear(DateTime date)
        {
            var day = (int)System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            return System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                date.AddDays(4 - (day == 0 ? 7 : day)),
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
        }

        private DateTime FirstDateOfWeek(int year, int weekOfYear)
        {
            var jan1 = new DateTime(year, 1, 1);
            var daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            var firstThursday = jan1.AddDays(daysOffset);
            var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;
            var firstWeek = calendar.GetWeekOfYear(firstThursday, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
                weekNum -= 1;

            var result = firstThursday.AddDays(7 * weekNum - 3);
            return result;
        }

        // Thêm phương thức mới vào SaleService để xử lý trang Order
        public async Task<OrdersViewModel> GetOrdersAsync(
            string searchTerm = null,
            string status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string sortBy = "newest",
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Variant)
                        .ThenInclude(v => v.Product)
                .AsQueryable();

            // Áp dụng bộ lọc tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(o =>
                    o.OrderId.ToString().Contains(searchTerm) ||
                    o.User.FullName.Contains(searchTerm) ||
                    o.ShippingAddress.Contains(searchTerm) ||
                    o.PhoneContact.Contains(searchTerm)
                );
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                query = query.Where(o => o.Status == status);
            }

            // Lọc theo ngày bắt đầu
            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }

            // Lọc theo ngày kết thúc
            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endDate.Value.AddDays(1));
            }

            // Áp dụng sắp xếp
            switch (sortBy)
            {
                case "oldest":
                    query = query.OrderBy(o => o.OrderDate);
                    break;
                case "highest":
                    query = query.OrderByDescending(o => o.TotalAmount);
                    break;
                case "lowest":
                    query = query.OrderBy(o => o.TotalAmount);
                    break;
                case "newest":
                default:
                    query = query.OrderByDescending(o => o.OrderDate);
                    break;
            }

            // Lấy tổng số đơn hàng (đếm tổng)
            var totalOrders = await query.CountAsync();

            // Áp dụng phân trang
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lấy tất cả các trạng thái đơn hàng có trong hệ thống
            var orderStatuses = await _context.Orders
                .Where(o => o.Status != null)
                .Select(o => o.Status)
                .Distinct()
                .ToListAsync();

            // Tổng hợp kết quả
            var result = new OrdersViewModel
            {
                Orders = orders,
                TotalOrders = totalOrders,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize),
                SearchTerm = searchTerm,
                Status = status,
                StartDate = startDate,
                EndDate = endDate,
                SortBy = sortBy,
                OrderStatuses = orderStatuses
            };

            return result;
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Variant)
                        .ThenInclude(v => v.Product)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Variant)
                        .ThenInclude(v => v.Color)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Variant)
                        .ThenInclude(v => v.Size)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Variant)
                        .ThenInclude(v => v.Material)
                .Include(o => o.OrderStatusHistories)
                    .ThenInclude(h => h.UpdatedByNavigation)
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status, string notes = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders.FindAsync(orderId);

                if (order == null)
                    return false;

                // Cập nhật trạng thái đơn hàng
                order.Status = status;
                if (!string.IsNullOrEmpty(notes))
                {
                    order.Notes = notes;
                }

                // Thêm vào lịch sử trạng thái
                var statusHistory = new OrderStatusHistory
                {
                    OrderId = orderId,
                    Status = status,
                    UpdatedAt = DateTime.Now,
                    // UpdatedBy sẽ được thiết lập ở Controller từ người dùng hiện tại
                    Notes = notes
                };

                await _context.OrderStatusHistories.AddAsync(statusHistory);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public IEnumerable<ExportOrderViewModel> GetOrdersForExport(string status, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails) // Sử dụng OrderDetails thay vì OrderItems
                .Include(o => o.Payment)
                .AsQueryable();

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                query = query.Where(o => o.Status == status);
            }

            // Lọc theo khoảng ngày
            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                endDate = endDate.Value.AddDays(1).AddTicks(-1); // Đến cuối ngày
                query = query.Where(o => o.OrderDate <= endDate.Value);
            }

            // Sắp xếp theo ngày đặt hàng mới nhất
            query = query.OrderByDescending(o => o.OrderDate);

            // Chuyển đổi sang ExportOrderViewModel
            return query.Select(o => new ExportOrderViewModel
            {
                OrderId = o.OrderId,
                OrderDate = o.OrderDate,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                CustomerName = o.User != null ? o.User.FullName : "N/A", // Điều chỉnh theo thuộc tính thực tế của User
                CustomerPhone = o.PhoneContact,
                ShippingAddress = o.ShippingAddress,
                PaymentMethod = o.Payment != null ? o.Payment.PaymentMethod : "N/A", // Điều chỉnh dựa trên cấu trúc Payment
                PaymentStatus = o.Payment != null ? o.Payment.PaymentStatus : "N/A", // Điều chỉnh dựa trên cấu trúc Payment
                Notes = o.Notes,
                ItemCount = o.OrderDetails.Count
            }).ToList();
        }
    }
}