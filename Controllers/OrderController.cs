#nullable disable
using FoodOrderingWeb.Data;
using FoodOrderingWeb.Models.Entities;
using FoodOrderingWeb.Models.ViewModels;
using FoodOrderingWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingWeb.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;
        private readonly IShippingService _shippingService;

        public OrderController(
            ApplicationDbContext context,
            ICartService cartService,
            IShippingService shippingService)
        {
            _context = context;
            _cartService = cartService;
            _shippingService = shippingService;
        }

        public IActionResult Checkout()
        {
            var cart = _cartService.GetCart();
            if (!cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                CartItems = cart.Items,
                TotalAmount = cart.TotalAmount,
                ShippingFee = _shippingService.GetDefaultShippingFee()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cartData = _cartService.GetCart();
                model.CartItems = cartData.Items;
                model.TotalAmount = cartData.TotalAmount;
                return View(model);
            }

            var cart = _cartService.GetCart();
            if (!cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "Cart");
            }

            // Tính phí ship dựa trên địa chỉ
            var shippingFee = await _shippingService.CalculateShippingFee(model.DeliveryAddress);

            // Kiểm tra miễn phí ship
            var freeShippingThreshold = 100000m; // 100,000đ
            if (cart.TotalAmount >= freeShippingThreshold)
            {
                shippingFee = 0;
            }

            var orderCode = GenerateOrderCode();

            var order = new Order
            {
                OrderCode = orderCode,
                CustomerName = model.CustomerName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,  // ✅ LƯU EMAIL
                DeliveryAddress = model.DeliveryAddress,
                Note = model.Note,
                TotalAmount = cart.TotalAmount,
                ShippingFee = shippingFee,
                FinalTotal = cart.TotalAmount + shippingFee,
                OrderDate = DateTime.Now,
                OrderStatus = OrderStatus.Pending
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cart.Items)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    FoodId = item.FoodId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                };
                _context.OrderDetails.Add(orderDetail);
            }

            await _context.SaveChangesAsync();
            _cartService.ClearCart();

            return RedirectToAction("Success", new { orderCode = orderCode });
        }

        public async Task<IActionResult> Success(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode))
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Food)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public IActionResult Track()
        {
            return View();
        }

        public async Task<IActionResult> TrackResult(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode))
            {
                return View(null);
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Food)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            return View(order);
        }

        private string GenerateOrderCode()
        {
            return $"DH{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }
    }
}