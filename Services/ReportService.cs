using FoodOrderingWeb.Data;
using FoodOrderingWeb.Models.Entities;
using FoodOrderingWeb.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.Globalization;

namespace FoodOrderingWeb.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportService> _logger;

        public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ========================================
        // BÁO CÁO DOANH THU
        // ========================================
        public async Task<RevenueReportViewModel> GetRevenueReportAsync(ReportFilterViewModel filter)
        {
            try
            {
                var query = _context.Orders.AsQueryable();

                // Apply date filter
                if (filter.FromDate.HasValue)
                    query = query.Where(o => o.OrderDate >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                {
                    var toDateEnd = filter.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(o => o.OrderDate <= toDateEnd);
                }

                // Apply status filter
                if (filter.Status.HasValue)
                    query = query.Where(o => o.OrderStatus == filter.Status.Value);

                var orders = await query.ToListAsync();

                // Group by period
                var revenueData = GroupOrdersByPeriod(orders, filter.Period);

                // Calculate summary
                var summary = new RevenueSummary
                {
                    TotalOrders = orders.Count,
                    TotalRevenue = orders.Sum(o => o.TotalAmount),
                    TotalShippingFee = orders.Sum(o => o.ShippingFee),
                    NetRevenue = orders.Sum(o => o.FinalTotal),
                    CompletedOrders = orders.Count(o => o.OrderStatus == OrderStatus.Completed),
                    CancelledOrders = orders.Count(o => o.OrderStatus == OrderStatus.Cancelled)
                };

                summary.AverageOrderValue = summary.TotalOrders > 0
                    ? summary.NetRevenue / summary.TotalOrders
                    : 0;

                summary.CompletionRate = summary.TotalOrders > 0
                    ? (decimal)summary.CompletedOrders / summary.TotalOrders * 100
                    : 0;

                // Revenue by status
                var revenueByStatus = orders
                    .GroupBy(o => o.OrderStatus)
                    .Select(g => new RevenueByStatus
                    {
                        Status = g.Key,
                        StatusText = GetStatusText(g.Key),
                        Count = g.Count(),
                        TotalAmount = g.Sum(o => o.FinalTotal),
                        Percentage = summary.TotalOrders > 0
                            ? (decimal)g.Count() / summary.TotalOrders * 100
                            : 0
                    })
                    .OrderByDescending(r => r.Count)
                    .ToList();

                return new RevenueReportViewModel
                {
                    Filter = filter,
                    RevenueData = revenueData,
                    Summary = summary,
                    RevenueByStatus = revenueByStatus
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating revenue report");
                return new RevenueReportViewModel();
            }
        }

        private List<RevenueByPeriod> GroupOrdersByPeriod(List<Order> orders, ReportPeriod period)
        {
            return period switch
            {
                ReportPeriod.Day => orders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new RevenueByPeriod
                    {
                        Period = g.Key.ToString("yyyy-MM-dd"),
                        PeriodDisplay = g.Key.ToString("dd/MM/yyyy"),
                        TotalOrders = g.Count(),
                        TotalRevenue = g.Sum(o => o.TotalAmount),
                        TotalShippingFee = g.Sum(o => o.ShippingFee),
                        NetRevenue = g.Sum(o => o.FinalTotal),
                        CompletedOrders = g.Count(o => o.OrderStatus == OrderStatus.Completed),
                        CancelledOrders = g.Count(o => o.OrderStatus == OrderStatus.Cancelled)
                    })
                    .OrderBy(r => r.Period)
                    .ToList(),

                ReportPeriod.Week => orders
                    .GroupBy(o => GetWeekOfYear(o.OrderDate))
                    .Select(g => new RevenueByPeriod
                    {
                        Period = g.Key.weekYear,
                        PeriodDisplay = g.Key.weekDisplay,
                        TotalOrders = g.Count(),
                        TotalRevenue = g.Sum(o => o.TotalAmount),
                        TotalShippingFee = g.Sum(o => o.ShippingFee),
                        NetRevenue = g.Sum(o => o.FinalTotal),
                        CompletedOrders = g.Count(o => o.OrderStatus == OrderStatus.Completed),
                        CancelledOrders = g.Count(o => o.OrderStatus == OrderStatus.Cancelled)
                    })
                    .OrderBy(r => r.Period)
                    .ToList(),

                ReportPeriod.Month => orders
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .Select(g => new RevenueByPeriod
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                        PeriodDisplay = $"Tháng {g.Key.Month}/{g.Key.Year}",
                        TotalOrders = g.Count(),
                        TotalRevenue = g.Sum(o => o.TotalAmount),
                        TotalShippingFee = g.Sum(o => o.ShippingFee),
                        NetRevenue = g.Sum(o => o.FinalTotal),
                        CompletedOrders = g.Count(o => o.OrderStatus == OrderStatus.Completed),
                        CancelledOrders = g.Count(o => o.OrderStatus == OrderStatus.Cancelled)
                    })
                    .OrderBy(r => r.Period)
                    .ToList(),

                ReportPeriod.Year => orders
                    .GroupBy(o => o.OrderDate.Year)
                    .Select(g => new RevenueByPeriod
                    {
                        Period = g.Key.ToString(),
                        PeriodDisplay = $"Năm {g.Key}",
                        TotalOrders = g.Count(),
                        TotalRevenue = g.Sum(o => o.TotalAmount),
                        TotalShippingFee = g.Sum(o => o.ShippingFee),
                        NetRevenue = g.Sum(o => o.FinalTotal),
                        CompletedOrders = g.Count(o => o.OrderStatus == OrderStatus.Completed),
                        CancelledOrders = g.Count(o => o.OrderStatus == OrderStatus.Cancelled)
                    })
                    .OrderBy(r => r.Period)
                    .ToList(),

                _ => new List<RevenueByPeriod>()
            };
        }

        private (string weekYear, string weekDisplay) GetWeekOfYear(DateTime date)
        {
            var calendar = CultureInfo.CurrentCulture.Calendar;
            var week = calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            return ($"{date.Year}-W{week:D2}", $"Tuần {week}/{date.Year}");
        }

        // ========================================
        // BÁO CÁO MÓN ĂN BÁN CHẠY
        // ========================================
        public async Task<BestSellingReportViewModel> GetBestSellingReportAsync(ReportFilterViewModel filter)
        {
            try
            {
                var query = _context.OrderDetails
                    .Include(od => od.Order)
                    .Include(od => od.Food)
                        .ThenInclude(f => f!.Category)
                    .Where(od => od.Order != null && od.Food != null);

                // Apply date filter
                if (filter.FromDate.HasValue)
                    query = query.Where(od => od.Order!.OrderDate >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                {
                    var toDateEnd = filter.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(od => od.Order!.OrderDate <= toDateEnd);
                }

                // Apply status filter (only completed orders)
                query = query.Where(od => od.Order!.OrderStatus == OrderStatus.Completed);

                // Apply category filter
                if (filter.CategoryId.HasValue && filter.CategoryId > 0)
                    query = query.Where(od => od.Food!.CategoryId == filter.CategoryId.Value);

                var orderDetails = await query.ToListAsync();

                // Group by food
                var bestSellingFoods = orderDetails
                    .GroupBy(od => new
                    {
                        od.FoodId,
                        od.Food!.Name,
                        CategoryName = od.Food.Category!.Name,
                        od.Food.ImageUrl,
                        od.Food.Price
                    })
                    .Select(g => new BestSellingFood
                    {
                        FoodId = g.Key.FoodId,
                        FoodName = g.Key.Name,
                        CategoryName = g.Key.CategoryName,
                        ImageUrl = g.Key.ImageUrl,
                        Price = g.Key.Price,
                        TotalQuantitySold = g.Sum(od => od.Quantity),
                        TotalRevenue = g.Sum(od => od.SubTotal),
                        OrderCount = g.Select(od => od.OrderId).Distinct().Count(),
                        AverageQuantityPerOrder = (decimal)g.Average(od => od.Quantity)
                    })
                    .OrderByDescending(f => f.TotalQuantitySold)
                    .Take(filter.TopCount)
                    .ToList();

                // Revenue by category
                var categoryRevenue = orderDetails
                    .GroupBy(od => new
                    {
                        od.Food!.CategoryId,
                        CategoryName = od.Food.Category!.Name
                    })
                    .Select(g => new CategoryRevenue
                    {
                        CategoryId = g.Key.CategoryId,
                        CategoryName = g.Key.CategoryName,
                        TotalQuantitySold = g.Sum(od => od.Quantity),
                        TotalRevenue = g.Sum(od => od.SubTotal),
                        ProductCount = g.Select(od => od.FoodId).Distinct().Count()
                    })
                    .OrderByDescending(c => c.TotalRevenue)
                    .ToList();

                var totalRevenue = categoryRevenue.Sum(c => c.TotalRevenue);
                foreach (var cat in categoryRevenue)
                {
                    cat.Percentage = totalRevenue > 0 ? (cat.TotalRevenue / totalRevenue * 100) : 0;
                }

                return new BestSellingReportViewModel
                {
                    Filter = filter,
                    BestSellingFoods = bestSellingFoods,
                    CategoryRevenue = categoryRevenue
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating best selling report");
                return new BestSellingReportViewModel();
            }
        }

        // ========================================
        // BÁO CÁO KHÁCH HÀNG
        // ========================================
        public async Task<CustomerReportViewModel> GetCustomerReportAsync(ReportFilterViewModel filter)
        {
            try
            {
                var query = _context.Orders.AsQueryable();

                // Apply date filter
                if (filter.FromDate.HasValue)
                    query = query.Where(o => o.OrderDate >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                {
                    var toDateEnd = filter.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(o => o.OrderDate <= toDateEnd);
                }

                var orders = await query.ToListAsync();

                // Group by customer
                var loyalCustomers = orders
                    .GroupBy(o => new { o.PhoneNumber, o.CustomerName, o.Email })
                    .Select(g => new
                    {
                        g.Key.CustomerName,
                        g.Key.PhoneNumber,
                        g.Key.Email,
                        Orders = g.ToList()
                    })
                    .Select(c => new LoyalCustomer
                    {
                        CustomerName = c.CustomerName,
                        PhoneNumber = c.PhoneNumber,
                        Email = c.Email,
                        TotalOrders = c.Orders.Count,
                        TotalSpent = c.Orders
                            .Where(o => o.OrderStatus == OrderStatus.Completed)
                            .Sum(o => o.FinalTotal),
                        AverageOrderValue = c.Orders.Any()
                            ? c.Orders.Average(o => o.FinalTotal)
                            : 0,
                        LastOrderDate = c.Orders.Max(o => o.OrderDate),
                        DaysSinceLastOrder = (DateTime.Now - c.Orders.Max(o => o.OrderDate)).Days
                    })
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(filter.TopCount)
                    .ToList();

                // Assign customer level
                foreach (var customer in loyalCustomers)
                {
                    customer.CustomerLevel = GetCustomerLevel(customer.TotalOrders, customer.TotalSpent);
                }

                // Statistics
                var allCustomers = orders
                    .GroupBy(o => o.PhoneNumber)
                    .Select(g => new
                    {
                        PhoneNumber = g.Key,
                        OrderCount = g.Count(),
                        FirstOrderDate = g.Min(o => o.OrderDate)
                    })
                    .ToList();

                var statistics = new CustomerStatistics
                {
                    TotalCustomers = allCustomers.Count,
                    NewCustomers = allCustomers.Count(c => c.OrderCount == 1),
                    ReturningCustomers = allCustomers.Count(c => c.OrderCount > 1),
                    AverageOrdersPerCustomer = allCustomers.Any()
                        ? (decimal)allCustomers.Average(c => c.OrderCount)
                        : 0,
                    AverageSpentPerCustomer = allCustomers.Any()
                        ? orders.Sum(o => o.FinalTotal) / allCustomers.Count
                        : 0,
                    VIPCustomers = loyalCustomers.Count(c => c.CustomerLevel == "VIP"),
                    LoyalCustomers = loyalCustomers.Count(c => c.CustomerLevel == "Thân thiết"),
                    RegularCustomers = loyalCustomers.Count(c => c.CustomerLevel == "Thường")
                };

                return new CustomerReportViewModel
                {
                    Filter = filter,
                    LoyalCustomers = loyalCustomers,
                    Statistics = statistics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating customer report");
                return new CustomerReportViewModel();
            }
        }

        private string GetCustomerLevel(int totalOrders, decimal totalSpent)
        {
            if (totalOrders >= 10 && totalSpent >= 1000000) // 10 đơn và 1 triệu
                return "VIP";
            if (totalOrders >= 5 && totalSpent >= 500000) // 5 đơn và 500k
                return "Thân thiết";
            return "Thường";
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
                _ => status.ToString()
            };
        }
        // ========================================
        // EXPORT EXCEL - BÁO CÁO DOANH THU
        // ========================================
        public async Task<byte[]> ExportRevenueReportToExcelAsync(ReportFilterViewModel filter)
        {
            try
            {
                var report = await GetRevenueReportAsync(filter);

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Báo cáo doanh thu");

                // Title
                worksheet.Cell(1, 1).Value = "BÁO CÁO DOANH THU";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Range(1, 1, 1, 7).Merge();

                // Filter info
                int row = 2;
                worksheet.Cell(row, 1).Value = $"Từ ngày: {filter.FromDate:dd/MM/yyyy} - Đến ngày: {filter.ToDate:dd/MM/yyyy}";
                worksheet.Cell(row, 1).Style.Font.Italic = true;
                worksheet.Range(row, 1, row, 7).Merge();

                row += 2;

                // Summary section
                worksheet.Cell(row, 1).Value = "TỔNG QUAN";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Range(row, 1, row, 2).Merge();
                row++;

                worksheet.Cell(row, 1).Value = "Tổng đơn hàng:";
                worksheet.Cell(row, 2).Value = report.Summary.TotalOrders;
                row++;

                worksheet.Cell(row, 1).Value = "Tổng doanh thu:";
                worksheet.Cell(row, 2).Value = report.Summary.TotalRevenue;
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0 ₫";
                row++;

                worksheet.Cell(row, 1).Value = "Phí vận chuyển:";
                worksheet.Cell(row, 2).Value = report.Summary.TotalShippingFee;
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0 ₫";
                row++;

                worksheet.Cell(row, 1).Value = "Doanh thu thuần:";
                worksheet.Cell(row, 2).Value = report.Summary.NetRevenue;
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0 ₫";
                worksheet.Cell(row, 2).Style.Font.Bold = true;
                row++;

                worksheet.Cell(row, 1).Value = "Giá trị TB/đơn:";
                worksheet.Cell(row, 2).Value = report.Summary.AverageOrderValue;
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0 ₫";
                row++;

                worksheet.Cell(row, 1).Value = "Tỷ lệ hoàn thành:";
                worksheet.Cell(row, 2).Value = report.Summary.CompletionRate / 100;
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "0.00%";
                row += 2;

                // Detail table header
                worksheet.Cell(row, 1).Value = "CHI TIẾT THEO THỜI GIAN";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Range(row, 1, row, 7).Merge();
                row++;

                // Headers
                var headers = new[] { "Thời gian", "Tổng đơn", "Doanh thu", "Phí ship", "Doanh thu thuần", "Hoàn thành", "Đã hủy" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(row, i + 1).Value = headers[i];
                    worksheet.Cell(row, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row++;

                // Data rows
                foreach (var item in report.RevenueData)
                {
                    worksheet.Cell(row, 1).Value = item.PeriodDisplay;
                    worksheet.Cell(row, 2).Value = item.TotalOrders;
                    worksheet.Cell(row, 3).Value = item.TotalRevenue;
                    worksheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0 ₫";
                    worksheet.Cell(row, 4).Value = item.TotalShippingFee;
                    worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0 ₫";
                    worksheet.Cell(row, 5).Value = item.NetRevenue;
                    worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0 ₫";
                    worksheet.Cell(row, 6).Value = item.CompletedOrders;
                    worksheet.Cell(row, 7).Value = item.CancelledOrders;

                    for (int i = 1; i <= 7; i++)
                    {
                        worksheet.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting revenue report to Excel");
                throw;
            }
        }

        // ========================================
        // EXPORT EXCEL - BÁO CÁO MÓN ĂN BÁN CHẠY
        // ========================================
        public async Task<byte[]> ExportBestSellingReportToExcelAsync(ReportFilterViewModel filter)
        {
            try
            {
                var report = await GetBestSellingReportAsync(filter);

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Món ăn bán chạy");

                // Title
                worksheet.Cell(1, 1).Value = "BÁO CÁO MÓN ĂN BÁN CHẠY";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Range(1, 1, 1, 7).Merge();

                // Filter info
                int row = 2;
                worksheet.Cell(row, 1).Value = $"Từ ngày: {filter.FromDate:dd/MM/yyyy} - Đến ngày: {filter.ToDate:dd/MM/yyyy}";
                worksheet.Cell(row, 1).Style.Font.Italic = true;
                worksheet.Range(row, 1, row, 7).Merge();

                row += 2;

                // Category revenue section
                worksheet.Cell(row, 1).Value = "DOANH THU THEO DANH MỤC";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Range(row, 1, row, 5).Merge();
                row++;

                // Category headers
                var catHeaders = new[] { "Danh mục", "Số lượng bán", "Doanh thu", "Số món", "Tỷ lệ %" };
                for (int i = 0; i < catHeaders.Length; i++)
                {
                    worksheet.Cell(row, i + 1).Value = catHeaders[i];
                    worksheet.Cell(row, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row++;

                // Category data
                foreach (var cat in report.CategoryRevenue)
                {
                    worksheet.Cell(row, 1).Value = cat.CategoryName;
                    worksheet.Cell(row, 2).Value = cat.TotalQuantitySold;
                    worksheet.Cell(row, 3).Value = cat.TotalRevenue;
                    worksheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0 ₫";
                    worksheet.Cell(row, 4).Value = cat.ProductCount;
                    worksheet.Cell(row, 5).Value = cat.Percentage / 100;
                    worksheet.Cell(row, 5).Style.NumberFormat.Format = "0.00%";

                    for (int i = 1; i <= 5; i++)
                    {
                        worksheet.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;
                }

                row += 2;

                // Best selling foods section
                worksheet.Cell(row, 1).Value = $"TOP {filter.TopCount} MÓN ĂN BÁN CHẠY";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Range(row, 1, row, 7).Merge();
                row++;

                // Food headers
                var foodHeaders = new[] { "#", "Tên món", "Danh mục", "Giá", "Số lượng bán", "Doanh thu", "Số đơn" };
                for (int i = 0; i < foodHeaders.Length; i++)
                {
                    worksheet.Cell(row, i + 1).Value = foodHeaders[i];
                    worksheet.Cell(row, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row++;

                // Food data
                int rank = 1;
                foreach (var food in report.BestSellingFoods)
                {
                    worksheet.Cell(row, 1).Value = rank++;
                    worksheet.Cell(row, 2).Value = food.FoodName;
                    worksheet.Cell(row, 3).Value = food.CategoryName;
                    worksheet.Cell(row, 4).Value = food.Price;
                    worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0 ₫";
                    worksheet.Cell(row, 5).Value = food.TotalQuantitySold;
                    worksheet.Cell(row, 6).Value = food.TotalRevenue;
                    worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0 ₫";
                    worksheet.Cell(row, 7).Value = food.OrderCount;

                    for (int i = 1; i <= 7; i++)
                    {
                        worksheet.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting best selling report to Excel");
                throw;
            }
        }

        // ========================================
        // EXPORT EXCEL - BÁO CÁO KHÁCH HÀNG
        // ========================================
        public async Task<byte[]> ExportCustomerReportToExcelAsync(ReportFilterViewModel filter)
        {
            try
            {
                var report = await GetCustomerReportAsync(filter);

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Khách hàng thân thiết");

                // Title
                worksheet.Cell(1, 1).Value = "BÁO CÁO KHÁCH HÀNG THÂN THIẾT";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Range(1, 1, 1, 8).Merge();

                // Filter info
                int row = 2;
                worksheet.Cell(row, 1).Value = $"Từ ngày: {filter.FromDate:dd/MM/yyyy} - Đến ngày: {filter.ToDate:dd/MM/yyyy}";
                worksheet.Cell(row, 1).Style.Font.Italic = true;
                worksheet.Range(row, 1, row, 8).Merge();

                row += 2;

                // Statistics section
                worksheet.Cell(row, 1).Value = "THỐNG KÊ TỔNG QUAN";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Range(row, 1, row, 2).Merge();
                row++;

                worksheet.Cell(row, 1).Value = "Tổng khách hàng:";
                worksheet.Cell(row, 2).Value = report.Statistics.TotalCustomers;
                row++;

                worksheet.Cell(row, 1).Value = "Khách hàng mới:";
                worksheet.Cell(row, 2).Value = report.Statistics.NewCustomers;
                row++;

                worksheet.Cell(row, 1).Value = "Khách quay lại:";
                worksheet.Cell(row, 2).Value = report.Statistics.ReturningCustomers;
                row++;

                worksheet.Cell(row, 1).Value = "Khách VIP:";
                worksheet.Cell(row, 2).Value = report.Statistics.VIPCustomers;
                row++;

                worksheet.Cell(row, 1).Value = "TB đơn/khách:";
                worksheet.Cell(row, 2).Value = report.Statistics.AverageOrdersPerCustomer;
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "0.00";
                row++;

                worksheet.Cell(row, 1).Value = "TB chi tiêu/khách:";
                worksheet.Cell(row, 2).Value = report.Statistics.AverageSpentPerCustomer;
                worksheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0 ₫";
                row += 2;

                // Customer list section
                worksheet.Cell(row, 1).Value = $"TOP {filter.TopCount} KHÁCH HÀNG THÂN THIẾT";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Range(row, 1, row, 8).Merge();
                row++;

                // Headers
                var headers = new[] { "#", "Tên khách hàng", "Số điện thoại", "Email", "Hạng", "Tổng đơn", "Tổng chi tiêu", "Đơn gần nhất" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(row, i + 1).Value = headers[i];
                    worksheet.Cell(row, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(row, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(row, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row++;

                // Data rows
                int rank = 1;
                foreach (var customer in report.LoyalCustomers)
                {
                    worksheet.Cell(row, 1).Value = rank++;
                    worksheet.Cell(row, 2).Value = customer.CustomerName;
                    worksheet.Cell(row, 3).Value = customer.PhoneNumber;
                    worksheet.Cell(row, 4).Value = customer.Email ?? "";
                    worksheet.Cell(row, 5).Value = customer.CustomerLevel;
                    worksheet.Cell(row, 6).Value = customer.TotalOrders;
                    worksheet.Cell(row, 7).Value = customer.TotalSpent;
                    worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0 ₫";
                    worksheet.Cell(row, 8).Value = customer.LastOrderDate.ToString("dd/MM/yyyy");

                    for (int i = 1; i <= 8; i++)
                    {
                        worksheet.Cell(row, i).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting customer report to Excel");
                throw;
            }
        }

        // ========================================
        // DASHBOARD STATISTICS
        // ========================================
        public async Task<DashboardStatistics> GetDashboardStatisticsAsync()
        {
            try
            {
                var today = DateTime.Today;
                var thisMonth = new DateTime(today.Year, today.Month, 1);
                var lastMonth = thisMonth.AddMonths(-1);

                // Today's statistics
                var todayOrders = await _context.Orders
                    .Where(o => o.OrderDate.Date == today)
                    .ToListAsync();

                var todayRevenue = todayOrders
                    .Where(o => o.OrderStatus == OrderStatus.Completed)
                    .Sum(o => o.FinalTotal);

                // This month's statistics
                var thisMonthOrders = await _context.Orders
                    .Where(o => o.OrderDate >= thisMonth)
                    .ToListAsync();

                var thisMonthRevenue = thisMonthOrders
                    .Where(o => o.OrderStatus == OrderStatus.Completed)
                    .Sum(o => o.FinalTotal);

                // Last month's statistics for comparison
                var lastMonthOrders = await _context.Orders
                    .Where(o => o.OrderDate >= lastMonth && o.OrderDate < thisMonth)
                    .ToListAsync();

                var lastMonthRevenue = lastMonthOrders
                    .Where(o => o.OrderStatus == OrderStatus.Completed)
                    .Sum(o => o.FinalTotal);

                // Calculate growth
                var revenueGrowth = lastMonthRevenue > 0
                    ? ((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue * 100)
                    : 0;

                var orderGrowth = lastMonthOrders.Count > 0
                    ? ((decimal)(thisMonthOrders.Count - lastMonthOrders.Count) / lastMonthOrders.Count * 100)
                    : 0;

                // Total customers
                var totalCustomers = await _context.Orders
                    .Select(o => o.PhoneNumber)
                    .Distinct()
                    .CountAsync();

                // Pending orders
                var pendingOrders = await _context.Orders
                    .CountAsync(o => o.OrderStatus == OrderStatus.Pending);

                // Best selling products this month
                var bestSellingThisMonth = await _context.OrderDetails
                    .Include(od => od.Order)
                    .Include(od => od.Food)
                    .Where(od => od.Order!.OrderDate >= thisMonth
                              && od.Order.OrderStatus == OrderStatus.Completed)
                    .GroupBy(od => new { od.FoodId, od.Food!.Name, od.Food.ImageUrl })
                    .Select(g => new BestSellingFood
                    {
                        FoodId = g.Key.FoodId,
                        FoodName = g.Key.Name,
                        ImageUrl = g.Key.ImageUrl,
                        TotalQuantitySold = g.Sum(od => od.Quantity),
                        TotalRevenue = g.Sum(od => od.SubTotal)
                    })
                    .OrderByDescending(f => f.TotalQuantitySold)
                    .Take(5)
                    .ToListAsync();

                // Revenue chart data (last 7 days)
                var last7Days = Enumerable.Range(0, 7)
                    .Select(i => today.AddDays(-i))
                    .Reverse()
                    .ToList();

                var revenueChart = new List<RevenueChartData>();
                foreach (var day in last7Days)
                {
                    var dayRevenue = await _context.Orders
                        .Where(o => o.OrderDate.Date == day && o.OrderStatus == OrderStatus.Completed)
                        .SumAsync(o => o.FinalTotal);

                    revenueChart.Add(new RevenueChartData
                    {
                        Date = day,
                        Revenue = dayRevenue
                    });
                }

                // Recent orders
                var recentOrders = await _context.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .Select(o => new RecentOrderInfo
                    {
                        OrderId = o.OrderId,
                        OrderCode = o.OrderCode,
                        CustomerName = o.CustomerName,
                        TotalAmount = o.FinalTotal,
                        OrderStatus = o.OrderStatus,
                        OrderDate = o.OrderDate
                    })
                    .ToListAsync();

                return new DashboardStatistics
                {
                    // Today
                    TodayOrders = todayOrders.Count,
                    TodayRevenue = todayRevenue,

                    // This month
                    ThisMonthOrders = thisMonthOrders.Count,
                    ThisMonthRevenue = thisMonthRevenue,

                    // Growth
                    RevenueGrowth = revenueGrowth,
                    OrderGrowth = orderGrowth,

                    // Overall
                    TotalCustomers = totalCustomers,
                    PendingOrders = pendingOrders,

                    // Charts and lists
                    BestSellingProducts = bestSellingThisMonth,
                    RevenueChart = revenueChart,
                    RecentOrders = recentOrders
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard statistics");
                return new DashboardStatistics();
            }
        }  

    }       
}

