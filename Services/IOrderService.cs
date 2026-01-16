using FoodOrderingWeb.Models.Entities;
using FoodOrderingWeb.Models.ViewModels;

namespace FoodOrderingWeb.Services
{
    public interface IOrderService
    {
        Task<(List<Order> Orders, int TotalCount)> GetFilteredOrdersAsync(OrderFilterViewModel filter);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string changedBy, string? note = null);
        Task<bool> CancelOrderAsync(int orderId, string reason, string cancelledBy);
        Task<List<OrderStatusHistory>> GetOrderStatusHistoryAsync(int orderId);
        Task<byte[]> ExportOrdersToExcelAsync(OrderFilterViewModel filter);
    }
}