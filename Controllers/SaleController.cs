using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using WebApplication1.Models;
using WebApplication1.Service;
using WebApplication1.ViewModel;

namespace WebApplication1.Controllers
{

    public class SaleController : Controller
    {
        private readonly ISaleService _saleService;

        public SaleController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        public async Task<IActionResult> Dashboard(DateTime? startDate = null, DateTime? endDate = null, string period = "week")
        {
            var dashboardData = await _saleService.GetDashboardDataAsync(startDate, endDate, period);
            return View(dashboardData);
        }

        [HttpPost]
        public async Task<IActionResult> FilterDashboard(DateTime startDate, DateTime endDate, string period = "week")
        {
            return RedirectToAction("Dashboard", new { startDate, endDate, period });
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesTrend(DateTime? startDate = null, DateTime? endDate = null, string period = "week")
        {
            var salesTrend = await _saleService.GetSalesTrendAsync(startDate, endDate, period);
            return Json(salesTrend);
        }

        [HttpGet]
        public async Task<IActionResult> GetStatusDistribution(DateTime? startDate = null, DateTime? endDate = null)
        {
            var statusDistribution = await _saleService.GetOrderStatusDistributionAsync(startDate, endDate);
            return Json(statusDistribution);
        }

        // Thêm action cho trang Orders
        [HttpGet]
        public async Task<IActionResult> Orders(
            string search = null,
            string status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string sortBy = "newest",
            int page = 1,
            int pageSize = 10)
        {
            var model = await _saleService.GetOrdersAsync(
                search, status, startDate, endDate, sortBy, page, pageSize);

            return View(model);
        }

        // Thêm action chi tiết đơn hàng
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _saleService.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var result = await _saleService.UpdateOrderStatusAsync(orderId, status,
                $"Status changed to {status} on {DateTime.Now}"); // Tự động thêm ghi chú đơn giản

            if (result)
            {
                TempData["SuccessMessage"] = $"Trạng thái đơn hàng đã được cập nhật thành {status}!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể cập nhật trạng thái đơn hàng!";
            }

            return RedirectToAction("OrderDetails", new { id = orderId });
        }

        public IActionResult ExportOrdersToExcel(string status, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                // Lấy dữ liệu đơn hàng dựa trên các tham số lọc
                var orders = _saleService.GetOrdersForExport(status, startDate, endDate);

                // Tạo file Excel với EPPlus
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Orders");

                    // Thiết lập style cho header
                    using (var range = worksheet.Cells["A1:I1"])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
                        range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Thiết lập header cho các cột
                    worksheet.Cells[1, 1].Value = "Order ID";
                    worksheet.Cells[1, 2].Value = "Order Date";
                    worksheet.Cells[1, 3].Value = "Customer";
                    worksheet.Cells[1, 4].Value = "Contact";
                    worksheet.Cells[1, 5].Value = "Shipping Address";
                    worksheet.Cells[1, 6].Value = "Total Amount";
                    worksheet.Cells[1, 7].Value = "Status";
                    worksheet.Cells[1, 8].Value = "Payment Method";
                    worksheet.Cells[1, 9].Value = "Payment Status";

                    // Điền dữ liệu
                    int row = 2;
                    foreach (var order in orders)
                    {
                        worksheet.Cells[row, 1].Value = order.OrderId;
                        worksheet.Cells[row, 2].Value = order.OrderDate;
                        worksheet.Cells[row, 2].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";

                        worksheet.Cells[row, 3].Value = order.CustomerName;
                        worksheet.Cells[row, 4].Value = order.CustomerPhone;
                        worksheet.Cells[row, 5].Value = order.ShippingAddress;

                        worksheet.Cells[row, 6].Value = order.TotalAmount;
                        worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";

                        worksheet.Cells[row, 7].Value = order.Status;

                        // Thêm màu sắc dựa trên trạng thái
                        switch (order.Status?.ToLower())
                        {
                            case "pending":
                                worksheet.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 235, 156));
                                break;
                            case "processing":
                                worksheet.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(189, 215, 238));
                                break;
                            case "shipped":
                                worksheet.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(169, 208, 142));
                                break;
                            case "completed":
                                worksheet.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(146, 208, 80));
                                break;
                            case "cancelled":
                                worksheet.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 199, 206));
                                break;
                        }

                        worksheet.Cells[row, 8].Value = order.PaymentMethod;
                        worksheet.Cells[row, 9].Value = order.PaymentStatus;

                        row++;
                    }

                    // Tự động điều chỉnh độ rộng cột
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Thêm bảng tổng hợp đơn hàng theo trạng thái
                    row += 2;
                    var summaryStartRow = row;

                    worksheet.Cells[row, 1].Value = "Order Status Summary";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    row++;

                    worksheet.Cells[row, 1].Value = "Status";
                    worksheet.Cells[row, 2].Value = "Count";
                    worksheet.Cells[row, 3].Value = "Total Value";

                    worksheet.Cells[row, 1, row, 3].Style.Font.Bold = true;
                    worksheet.Cells[row, 1, row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1, row, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
                    worksheet.Cells[row, 1, row, 3].Style.Font.Color.SetColor(System.Drawing.Color.White);

                    row++;

                    // Nhóm đơn hàng theo trạng thái và tính tổng
                    var statusGroups = orders.GroupBy(o => o.Status ?? "Unknown").ToList();

                    foreach (var group in statusGroups)
                    {
                        worksheet.Cells[row, 1].Value = group.Key;
                        worksheet.Cells[row, 2].Value = group.Count();
                        worksheet.Cells[row, 3].Value = group.Sum(o => o.TotalAmount);
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
                        row++;
                    }

                    // Tạo tên file dựa trên ngày giờ hiện tại
                    var fileName = $"Orders_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                    // Trả về file Excel
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    var fileContents = package.GetAsByteArray();

                    return File(fileContents, contentType, fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error exporting data: {ex.Message}";
                return RedirectToAction("Orders");
            }
        }
    }
}
