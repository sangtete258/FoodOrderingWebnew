using FoodOrderingWeb.Services;
using Microsoft.AspNetCore.Mvc;
using FoodOrderingWeb.Models.ViewModels;

namespace FoodOrderingWeb.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int foodId, int quantity = 1)
        {
            _cartService.AddToCart(foodId, quantity);
            var cart = _cartService.GetCart();

            return Json(new
            {
                success = true,
                message = "Đã thêm vào giỏ hàng!",
                cartCount = cart.TotalItems,
                cartTotal = cart.TotalAmount,
                cartFinalTotal = cart.FinalTotal,
                shippingFee = cart.ShippingFee
            });
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int foodId, int quantity)
        {
            _cartService.UpdateQuantity(foodId, quantity);
            var cart = _cartService.GetCart();

            var item = cart.Items.FirstOrDefault(i => i.FoodId == foodId);

            return Json(new
            {
                success = true,
                itemSubTotal = item?.SubTotal ?? 0,
                cartTotal = cart.TotalAmount,
                cartFinalTotal = cart.FinalTotal,
                cartCount = cart.TotalItems,
                shippingFee = cart.ShippingFee
            });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int foodId)
        {
            _cartService.RemoveFromCart(foodId);
            var cart = _cartService.GetCart();

            return Json(new
            {
                success = true,
                message = "Đã xóa khỏi giỏ hàng!",
                cartTotal = cart.TotalAmount,
                cartFinalTotal = cart.FinalTotal,
                cartCount = cart.TotalItems,
                shippingFee = cart.ShippingFee
            });
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            return Json(new
            {
                success = true,
                message = "Đã xóa toàn bộ giỏ hàng!"
            });
        }

        // ✅ GET Mini Cart Summary (for dropdown)
        [HttpGet]
        public IActionResult GetCartSummary()
        {
            var cart = _cartService.GetCart();
            return PartialView("_CartSummary", cart);
        }

        // ✅ GET Cart Count (for badge update)
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = _cartService.GetCart();
            return Json(new
            {
                success = true,
                cartCount = cart.TotalItems
            });
        }
    }
}