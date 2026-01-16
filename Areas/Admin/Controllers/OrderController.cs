#nullable disable
using FoodOrderingWeb.Models.Entities;
using FoodOrderingWeb.Models.ViewModels;
using FoodOrderingWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: Admin/Order
        public async Task<IActionResult> Index(OrderFilterViewModel filter)
        {
            var (orders, totalCount) = await _orderService.GetFilteredOrdersAsync(filter);

            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            ViewBag.CurrentPage = filter.Page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalCount;
            ViewBag.Filter = filter;

            return View(orders);
        }

        // GET: Admin/Order/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _orderService.GetOrderByIdAsync(id.Value);
            if (order == null)
            {
                return NotFound();
            }

            // Lấy lịch sử trạng thái
            var history = await _orderService.GetOrderStatusHistoryAsync(id.Value);
            ViewBag.StatusHistory = history;

            return View(order);
        }

        // POST: Admin/Order/UpdateStatus - AJAX
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus newStatus, string note)
        {
            var username = User.Identity?.Name ?? "Admin";
            var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus, username, note);

            if (result)
            {
                return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
            }

            return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái" });
        }

        // POST: Admin/Order/CancelOrder - AJAX
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return Json(new { success = false, message = "Vui lòng nhập lý do hủy đơn" });
            }

            var username = User.Identity?.Name ?? "Admin";
            var result = await _orderService.CancelOrderAsync(orderId, reason, username);

            if (result)
            {
                return Json(new { success = true, message = "Đã hủy đơn hàng thành công!" });
            }

            return Json(new { success = false, message = "Có lỗi xảy ra khi hủy đơn hàng" });
        }

        // GET: Admin/Order/Invoice/5
        public async Task<IActionResult> Invoice(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _orderService.GetOrderByIdAsync(id.Value);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Admin/Order/ExportExcel
        public async Task<IActionResult> ExportExcel(OrderFilterViewModel filter)
        {
            var excelData = await _orderService.ExportOrdersToExcelAsync(filter);

            var fileName = $"DonHang_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // GET: Admin/Order/GetStatusHistory - AJAX
        [HttpGet]
        public async Task<IActionResult> GetStatusHistory(int orderId)
        {
            var history = await _orderService.GetOrderStatusHistoryAsync(orderId);

            var result = history.Select(h => new
            {
                fromStatus = GetStatusText(h.FromStatus),
                toStatus = GetStatusText(h.ToStatus),
                changedBy = h.ChangedBy,
                changedDate = h.ChangedDate.ToString("dd/MM/yyyy HH:mm:ss"),
                note = h.Note
            });

            return Json(new { success = true, data = result });
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