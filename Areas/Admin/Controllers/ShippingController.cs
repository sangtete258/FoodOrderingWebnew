#nullable disable
using FoodOrderingWeb.Models.Entities;
using FoodOrderingWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ShippingController : Controller
    {
        private readonly IShippingService _shippingService;

        public ShippingController(IShippingService shippingService)
        {
            _shippingService = shippingService;
        }

        // GET: Admin/Shipping
        public async Task<IActionResult> Index()
        {
            var configs = await _shippingService.GetAllShippingConfigsAsync();
            return View(configs);
        }

        // GET: Admin/Shipping/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Shipping/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShippingConfig config)
        {
            if (ModelState.IsValid)
            {
                var result = await _shippingService.CreateShippingConfigAsync(config);
                if (result)
                {
                    TempData["Success"] = "Thêm cấu hình phí ship thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi thêm cấu hình!";
                }
            }
            return View(config);
        }

        // GET: Admin/Shipping/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var config = await _shippingService.GetShippingConfigByIdAsync(id.Value);
            if (config == null)
            {
                return NotFound();
            }

            return View(config);
        }

        // POST: Admin/Shipping/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ShippingConfig config)
        {
            if (id != config.ShippingConfigId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _shippingService.UpdateShippingConfigAsync(config);
                if (result)
                {
                    TempData["Success"] = "Cập nhật cấu hình phí ship thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật cấu hình!";
                }
            }
            return View(config);
        }

        // POST: Admin/Shipping/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _shippingService.DeleteShippingConfigAsync(id);
            if (result)
            {
                TempData["Success"] = "Xóa cấu hình phí ship thành công!";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa cấu hình!";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Shipping/ToggleActive/5 - AJAX
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var config = await _shippingService.GetShippingConfigByIdAsync(id);
            if (config == null)
            {
                return Json(new { success = false, message = "Không tìm thấy cấu hình!" });
            }

            config.IsActive = !config.IsActive;
            var result = await _shippingService.UpdateShippingConfigAsync(config);

            if (result)
            {
                return Json(new
                {
                    success = true,
                    isActive = config.IsActive,
                    message = config.IsActive ? "Đã kích hoạt!" : "Đã vô hiệu hóa!"
                });
            }

            return Json(new { success = false, message = "Có lỗi xảy ra!" });
        }
    }
}