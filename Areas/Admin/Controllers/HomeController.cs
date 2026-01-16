using FoodOrderingWeb.Data;
using FoodOrderingWeb.Models.Entities;
using FoodOrderingWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();

            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var endOfLastMonth = startOfMonth.AddDays(-1);

            // === THỐNG KÊ TỔNG QUAN ===
            model.TotalOrders = await _context.Orders.CountAsync();
            model.TotalRevenue = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.Completed)
                .SumAsync(o => o.FinalTotal);
            model.TotalFoods = await _context.Foods.CountAsync();
            model.TotalCategories = await _context.Categories.CountAsync();

            model.TotalCustomers = await _context.Orders
                .Select(o => o.PhoneNumber)
                .Distinct()
                .CountAsync();

            // === THỐNG KÊ HÔM NAY ===
            model.TodayOrders = await _context.Orders
                .Where(o => o.OrderDate.Date == today)
                .CountAsync();

            model.TodayRevenue = await _context.Orders
                .Where(o => o.OrderDate.Date == today && o.OrderStatus == OrderStatus.Completed)
                .SumAsync(o => o.FinalTotal);

            // === SO SÁNH THÁNG NÀY VS THÁNG TRƯỚC ===
            var thisMonthRevenue = await _context.Orders
                .Where(o => o.OrderDate >= startOfMonth && o.OrderStatus == OrderStatus.Completed)
                .SumAsync(o => o.FinalTotal);

            var lastMonthRevenue = await _context.Orders
                .Where(o => o.OrderDate >= startOfLastMonth && o.OrderDate <= endOfLastMonth
                    && o.OrderStatus == OrderStatus.Completed)
                .SumAsync(o => o.FinalTotal);

            if (lastMonthRevenue > 0)
            {
                model.RevenueGrowth = ((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100;
            }

            var thisMonthOrders = await _context.Orders
                .Where(o => o.OrderDate >= startOfMonth)
                .CountAsync();

            var lastMonthOrders = await _context.Orders
                .Where(o => o.OrderDate >= startOfLastMonth && o.OrderDate <= endOfLastMonth)
                .CountAsync();

            if (lastMonthOrders > 0)
            {
                model.OrderGrowth = ((thisMonthOrders - lastMonthOrders) * 100) / lastMonthOrders;
            }

            // === THỐNG KÊ THEO TRẠNG THÁI ===
            model.PendingOrders = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.Pending)
                .CountAsync();

            model.ProcessingOrders = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.Processing)
                .CountAsync();

            model.ShippingOrders = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.Shipping)
                .CountAsync();

            model.CompletedOrders = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.Completed)
                .CountAsync();

            model.CancelledOrders = await _context.Orders
                .Where(o => o.OrderStatus == OrderStatus.Cancelled)
                .CountAsync();

            if (model.TotalOrders > 0)
            {
                model.CompletionRate = ((decimal)model.CompletedOrders / model.TotalOrders) * 100;
            }

            // === ĐƠN HÀNG GẦN ĐÂY === (ĐÃ FIX)
            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderCode,
                    o.CustomerName,
                    o.FinalTotal,
                    o.OrderDate,
                    o.OrderStatus
                })
                .ToListAsync();

            model.RecentOrders = recentOrders.Select(o => new RecentOrderDto
            {
                OrderId = o.OrderId,
                OrderCode = o.OrderCode,
                CustomerName = o.CustomerName,
                FinalTotal = o.FinalTotal,
                OrderDate = o.OrderDate,
                OrderStatus = o.OrderStatus,
                StatusText = GetStatusText(o.OrderStatus),
                StatusColor = GetStatusColor(o.OrderStatus)
            }).ToList();

            // === TOP MÓN ĂN BÁN CHẠY ===
            var allOrderDetails = await _context.OrderDetails
                .Include(od => od.Food)
                    .ThenInclude(f => f!.Category)
                .ToListAsync();

            model.TopFoods = allOrderDetails
                .GroupBy(od => od.FoodId)
                .Select(g => new TopFoodDto
                {
                    FoodId = g.Key,
                    FoodName = g.First().Food?.Name ?? "",
                    ImageUrl = g.First().Food?.ImageUrl,
                    CategoryName = g.First().Food?.Category?.Name ?? "",
                    TotalSold = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.SubTotal)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .ToList();

            // === TOP KHÁCH HÀNG ===
            model.TopCustomers = await _context.Orders
                .GroupBy(o => new { o.CustomerName, o.PhoneNumber })
                .Select(g => new TopCustomerDto
                {
                    CustomerName = g.Key.CustomerName,
                    PhoneNumber = g.Key.PhoneNumber,
                    TotalOrders = g.Count(),
                    TotalSpent = g.Sum(o => o.FinalTotal),
                    LastOrderDate = g.Max(o => o.OrderDate)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(10)
                .ToListAsync();

            // === BIỂU ĐỒ DOANH THU 7 NGÀY GẦN NHẤT ===
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-6 + i))
                .ToList();

            model.RevenueChart = new List<RevenueChartDto>();
            foreach (var date in last7Days)
            {
                var revenue = await _context.Orders
                    .Where(o => o.OrderDate.Date == date && o.OrderStatus == OrderStatus.Completed)
                    .SumAsync(o => o.FinalTotal);

                var orderCount = await _context.Orders
                    .Where(o => o.OrderDate.Date == date)
                    .CountAsync();

                model.RevenueChart.Add(new RevenueChartDto
                {
                    Date = date.ToString("dd/MM"),
                    Revenue = revenue,
                    OrderCount = orderCount
                });
            }

            // === BIỂU ĐỒ THEO TRẠNG THÁI ===
            model.OrderStatusChart = new Dictionary<string, int>
            {
                { "Chờ xác nhận", model.PendingOrders },
                { "Đang chuẩn bị", model.ProcessingOrders },
                { "Đang giao", model.ShippingOrders },
                { "Hoàn thành", model.CompletedOrders },
                { "Đã hủy", model.CancelledOrders }
            };

            return View(model);
        }

        private string GetStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Chờ xác nhận",
                OrderStatus.Processing => "Đang chuẩn bị",
                OrderStatus.Shipping => "Đang giao",
                OrderStatus.Completed => "Hoàn thành",
                OrderStatus.Cancelled => "Đã hủy",
                _ => "Không xác định"
            };
        }

        private string GetStatusColor(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "warning",
                OrderStatus.Processing => "info",
                OrderStatus.Shipping => "primary",
                OrderStatus.Completed => "success",
                OrderStatus.Cancelled => "danger",
                _ => "secondary"
            };
        }
    }
}