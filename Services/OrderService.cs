using FoodOrderingWeb.Data;
using FoodOrderingWeb.Models.Entities;
using FoodOrderingWeb.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;

namespace FoodOrderingWeb.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<OrderService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<(List<Order> Orders, int TotalCount)> GetFilteredOrdersAsync(OrderFilterViewModel filter)
        {
            var query = _context.Orders.AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(filter.SearchString))
            {
                query = query.Where(o =>
                    o.OrderCode.Contains(filter.SearchString) ||
                    o.CustomerName.Contains(filter.SearchString) ||
                    o.PhoneNumber.Contains(filter.SearchString));
            }

            // Filter by status
            if (filter.Status.HasValue)
            {
                query = query.Where(o => o.OrderStatus == filter.Status.Value);
            }

            // Filter by date range
            if (filter.FromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                var toDateEnd = filter.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.OrderDate <= toDateEnd);
            }

            // Filter by price range
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(o => o.FinalTotal >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(o => o.FinalTotal <= filter.MaxPrice.Value);
            }

            var totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.Food)
                .Include(o => o.StatusHistories)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string changedBy, string? note = null)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return false;

                var oldStatus = order.OrderStatus;

                // Cập nhật trạng thái
                order.OrderStatus = newStatus;

                if (newStatus == OrderStatus.Completed)
                {
                    order.CompletedDate = DateTime.Now;
                }

                // Lưu lịch sử
                var history = new OrderStatusHistory
                {
                    OrderId = orderId,
                    FromStatus = oldStatus,
                    ToStatus = newStatus,
                    ChangedBy = changedBy,
                    Note = note,
                    ChangedDate = DateTime.Now
                };

                _context.OrderStatusHistory.Add(history);
                await _context.SaveChangesAsync();

                // Gửi email thông báo
                try
                {
                    await _emailService.SendOrderStatusChangeEmailAsync(order, oldStatus, newStatus);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send email for order {order.OrderCode}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order status for OrderId {orderId}");
                return false;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId, string reason, string cancelledBy)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return false;

                var oldStatus = order.OrderStatus;

                order.OrderStatus = OrderStatus.Cancelled;
                order.CancellationReason = reason;
                order.CancelledDate = DateTime.Now;

                // Lưu lịch sử
                var history = new OrderStatusHistory
                {
                    OrderId = orderId,
                    FromStatus = oldStatus,
                    ToStatus = OrderStatus.Cancelled,
                    ChangedBy = cancelledBy,
                    Note = $"Hủy đơn: {reason}",
                    ChangedDate = DateTime.Now
                };

                _context.OrderStatusHistory.Add(history);
                await _context.SaveChangesAsync();

                // Gửi email thông báo hủy
                try
                {
                    await _emailService.SendOrderCancellationEmailAsync(order, reason);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send cancellation email for order {order.OrderCode}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling order {orderId}");
                return false;
            }
        }

        public async Task<List<OrderStatusHistory>> GetOrderStatusHistoryAsync(int orderId)
        {
            return await _context.OrderStatusHistory
                .Where(h => h.OrderId == orderId)
                .OrderByDescending(h => h.ChangedDate)
                .ToListAsync();
        }

        public async Task<byte[]> ExportOrdersToExcelAsync(OrderFilterViewModel filter)
        {
            var (orders, _) = await GetFilteredOrdersAsync(new OrderFilterViewModel
            {
                SearchString = filter.SearchString,
                Status = filter.Status,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                MinPrice = filter.MinPrice,
                MaxPrice = filter.MaxPrice,
                Page = 1,
                PageSize = int.MaxValue // Lấy tất cả
            });

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Đơn hàng");

            // Header
            worksheet.Cell(1, 1).Value = "Mã đơn";
            worksheet.Cell(1, 2).Value = "Khách hàng";
            worksheet.Cell(1, 3).Value = "Số điện thoại";
            worksheet.Cell(1, 4).Value = "Email";
            worksheet.Cell(1, 5).Value = "Địa chỉ";
            worksheet.Cell(1, 6).Value = "Ngày đặt";
            worksheet.Cell(1, 7).Value = "Tổng tiền";
            worksheet.Cell(1, 8).Value = "Phí ship";
            worksheet.Cell(1, 9).Value = "Thành tiền";
            worksheet.Cell(1, 10).Value = "Trạng thái";

            // Style header
            var headerRow = worksheet.Range(1, 1, 1, 10);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Data
            int row = 2;
            foreach (var order in orders)
            {
                worksheet.Cell(row, 1).Value = order.OrderCode;
                worksheet.Cell(row, 2).Value = order.CustomerName;
                worksheet.Cell(row, 3).Value = order.PhoneNumber;
                worksheet.Cell(row, 4).Value = order.Email ?? "";
                worksheet.Cell(row, 5).Value = order.DeliveryAddress;
                worksheet.Cell(row, 6).Value = order.OrderDate.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(row, 7).Value = order.TotalAmount;
                worksheet.Cell(row, 8).Value = order.ShippingFee;
                worksheet.Cell(row, 9).Value = order.FinalTotal;
                worksheet.Cell(row, 10).Value = GetStatusText(order.OrderStatus);
                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
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
    }
}