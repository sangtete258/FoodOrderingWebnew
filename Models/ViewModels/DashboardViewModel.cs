using FoodOrderingWeb.Models.Entities;

namespace FoodOrderingWeb.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Thống kê tổng quan
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalFoods { get; set; }
        public int TotalCategories { get; set; }
        public int TotalCustomers { get; set; }

        // So sánh với tháng trước
        public decimal RevenueGrowth { get; set; } // Phần trăm tăng/giảm
        public int OrderGrowth { get; set; }

        // Thống kê hôm nay
        public int TodayOrders { get; set; }
        public decimal TodayRevenue { get; set; }

        // Thống kê theo trạng thái
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }

        // Tỷ lệ hoàn thành
        public decimal CompletionRate { get; set; }

        // Đơn hàng gần đây
        public List<RecentOrderDto> RecentOrders { get; set; } = new();

        // Món ăn bán chạy
        public List<TopFoodDto> TopFoods { get; set; } = new();

        // Khách hàng thân thiết
        public List<TopCustomerDto> TopCustomers { get; set; } = new();

        // Dữ liệu biểu đồ doanh thu (7 ngày gần nhất)
        public List<RevenueChartDto> RevenueChart { get; set; } = new();

        // Dữ liệu biểu đồ theo trạng thái
        public Dictionary<string, int> OrderStatusChart { get; set; } = new();
    }

    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal FinalTotal { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
    }

    public class TopFoodDto
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    public class TopCustomerDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastOrderDate { get; set; }
    }

    public class RevenueChartDto
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
}