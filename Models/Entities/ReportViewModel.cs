using FoodOrderingWeb.Models.Entities;

namespace FoodOrderingWeb.Models.ViewModels
{
    // ========================================
    // FILTER CHO BÁO CÁO
    // ========================================
    public class ReportFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public ReportPeriod Period { get; set; } = ReportPeriod.Day;
        public OrderStatus? Status { get; set; }
        public int? CategoryId { get; set; }
        public int TopCount { get; set; } = 10;
    }

    public enum ReportPeriod
    {
        Day,
        Week,
        Month,
        Year
    }

    // ========================================
    // BÁO CÁO DOANH THU
    // ========================================
    public class RevenueReportViewModel
    {
        public ReportFilterViewModel Filter { get; set; } = new();
        public List<RevenueByPeriod> RevenueData { get; set; } = new();
        public RevenueSummary Summary { get; set; } = new();
        public List<RevenueByStatus> RevenueByStatus { get; set; } = new();
    }

    public class RevenueByPeriod
    {
        public string Period { get; set; } = string.Empty;
        public string PeriodDisplay { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalShippingFee { get; set; }
        public decimal NetRevenue { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
    }

    public class RevenueSummary
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalShippingFee { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class RevenueByStatus
    {
        public OrderStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }

    // ========================================
    // BÁO CÁO MÓN ĂN BÁN CHẠY
    // ========================================
    public class BestSellingReportViewModel
    {
        public ReportFilterViewModel Filter { get; set; } = new();
        public List<BestSellingFood> BestSellingFoods { get; set; } = new();
        public List<CategoryRevenue> CategoryRevenue { get; set; } = new();
    }

    public class BestSellingFood
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
        public decimal AverageQuantityPerOrder { get; set; }
    }

    public class CategoryRevenue
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ProductCount { get; set; }
        public decimal Percentage { get; set; }
    }

    // ========================================
    // BÁO CÁO KHÁCH HÀNG
    // ========================================
    public class CustomerReportViewModel
    {
        public ReportFilterViewModel Filter { get; set; } = new();
        public List<LoyalCustomer> LoyalCustomers { get; set; } = new();
        public CustomerStatistics Statistics { get; set; } = new();
    }

    public class LoyalCustomer
    {
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime LastOrderDate { get; set; }
        public int DaysSinceLastOrder { get; set; }
        public string CustomerLevel { get; set; } = string.Empty;
    }

    public class CustomerStatistics
    {
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int ReturningCustomers { get; set; }
        public decimal AverageOrdersPerCustomer { get; set; }
        public decimal AverageSpentPerCustomer { get; set; }
        public int VIPCustomers { get; set; }
        public int LoyalCustomers { get; set; }
        public int RegularCustomers { get; set; }
    }

    // ========================================
    // DASHBOARD STATISTICS - FIXED
    // ========================================
    public class DashboardStatistics
    {
        // Today
        public decimal TodayRevenue { get; set; }
        public int TodayOrders { get; set; }

        // This Month
        public decimal ThisMonthRevenue { get; set; }
        public int ThisMonthOrders { get; set; }

        // Growth
        public decimal RevenueGrowth { get; set; }
        public decimal OrderGrowth { get; set; }

        // Overall
        public int TotalCustomers { get; set; }
        public int PendingOrders { get; set; }

        // Charts & Lists
        public List<BestSellingFood> BestSellingProducts { get; set; } = new();
        public List<RevenueChartData> RevenueChart { get; set; } = new();
        public List<RecentOrderInfo> RecentOrders { get; set; } = new();
    }

    // ========================================
    // HELPER CLASSES - ADDED
    // ========================================
    public class RevenueChartData
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
    }

    public class RecentOrderInfo
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
    }
}