using FoodOrderingWeb.Models.ViewModels;

namespace FoodOrderingWeb.Services
{
    public interface IReportService
    {
        // ========================================
        // BÁO CÁO DOANH THU
        // ========================================
        Task<RevenueReportViewModel> GetRevenueReportAsync(ReportFilterViewModel filter);
        Task<byte[]> ExportRevenueReportToExcelAsync(ReportFilterViewModel filter);

        // ========================================
        // BÁO CÁO MÓN ĂN BÁN CHẠY
        // ========================================
        Task<BestSellingReportViewModel> GetBestSellingReportAsync(ReportFilterViewModel filter);
        Task<byte[]> ExportBestSellingReportToExcelAsync(ReportFilterViewModel filter);

        // ========================================
        // BÁO CÁO KHÁCH HÀNG
        // ========================================
        Task<CustomerReportViewModel> GetCustomerReportAsync(ReportFilterViewModel filter);
        Task<byte[]> ExportCustomerReportToExcelAsync(ReportFilterViewModel filter);

        // ========================================
        // DASHBOARD
        // ========================================
        Task<DashboardStatistics> GetDashboardStatisticsAsync();
    }
}