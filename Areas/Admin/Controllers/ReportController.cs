using FoodOrderingWeb.Data;
using FoodOrderingWeb.Models.ViewModels;
using FoodOrderingWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportController> _logger;

        public ReportController(
            IReportService reportService,
            ApplicationDbContext context,
            ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _context = context;
            _logger = logger;
        }

        // ========================================
        // DASHBOARD - Trang tổng quan
        // ========================================
        public async Task<IActionResult> Index()
        {
            try
            {
                var statistics = await _reportService.GetDashboardStatisticsAsync();
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["Error"] = "Có lỗi khi tải trang tổng quan";
                return View(new DashboardStatistics());
            }
        }

        // ========================================
        // BÁO CÁO DOANH THU
        // ========================================
        public async Task<IActionResult> Revenue(ReportFilterViewModel? filter)
        {
            try
            {
                // Initialize filter if null
                filter ??= new ReportFilterViewModel
                {
                    FromDate = DateTime.Today.AddMonths(-1),
                    ToDate = DateTime.Today,
                    Period = ReportPeriod.Day,
                    Status = null
                };

                // Get report data
                var report = await _reportService.GetRevenueReportAsync(filter);

                // Load categories for filter
                ViewBag.Categories = await GetCategoriesForDropdown();

                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading revenue report");
                TempData["Error"] = "Có lỗi khi tải báo cáo doanh thu";
                return View(new RevenueReportViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportRevenueExcel(ReportFilterViewModel filter)
        {
            try
            {
                var excelData = await _reportService.ExportRevenueReportToExcelAsync(filter);
                var fileName = $"BaoCaoDoanhThu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting revenue report");
                TempData["Error"] = "Có lỗi khi xuất báo cáo Excel";
                return RedirectToAction(nameof(Revenue));
            }
        }

        // ========================================
        // BÁO CÁO MÓN ĂN BÁN CHẠY
        // ========================================
        public async Task<IActionResult> BestSelling(ReportFilterViewModel? filter)
        {
            try
            {
                // Initialize filter if null
                filter ??= new ReportFilterViewModel
                {
                    FromDate = DateTime.Today.AddMonths(-1),
                    ToDate = DateTime.Today,
                    Period = ReportPeriod.Month,
                    TopCount = 10
                };

                // Get report data
                var report = await _reportService.GetBestSellingReportAsync(filter);

                // Load categories for filter
                ViewBag.Categories = await GetCategoriesForDropdown();

                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading best selling report");
                TempData["Error"] = "Có lỗi khi tải báo cáo món bán chạy";
                return View(new BestSellingReportViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportBestSellingExcel(ReportFilterViewModel filter)
        {
            try
            {
                var excelData = await _reportService.ExportBestSellingReportToExcelAsync(filter);
                var fileName = $"BaoCaoMonBanChay_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting best selling report");
                TempData["Error"] = "Có lỗi khi xuất báo cáo Excel";
                return RedirectToAction(nameof(BestSelling));
            }
        }

        // ========================================
        // BÁO CÁO KHÁCH HÀNG THÂN THIẾT
        // ========================================
        public async Task<IActionResult> Customer(ReportFilterViewModel? filter)
        {
            try
            {
                // Initialize filter if null
                filter ??= new ReportFilterViewModel
                {
                    FromDate = DateTime.Today.AddMonths(-3),
                    ToDate = DateTime.Today,
                    TopCount = 20
                };

                // Get report data
                var report = await _reportService.GetCustomerReportAsync(filter);

                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer report");
                TempData["Error"] = "Có lỗi khi tải báo cáo khách hàng";
                return View(new CustomerReportViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportCustomerExcel(ReportFilterViewModel filter)
        {
            try
            {
                var excelData = await _reportService.ExportCustomerReportToExcelAsync(filter);
                var fileName = $"BaoCaoKhachHang_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting customer report");
                TempData["Error"] = "Có lỗi khi xuất báo cáo Excel";
                return RedirectToAction(nameof(Customer));
            }
        }

        // ========================================
        // HELPER METHODS
        // ========================================
        private async Task<SelectList> GetCategoriesForDropdown()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new { c.CategoryId, c.Name })
                .ToListAsync();

            // Add "All" option
            var allOption = new { CategoryId = 0, Name = "Tất cả danh mục" };
            var categoriesWithAll = new[] { allOption }.Concat(categories);

            return new SelectList(categoriesWithAll, "CategoryId", "Name");
        }
    }
}