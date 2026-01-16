using FoodOrderingWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingWeb.Controllers.Api
{
    [Route("api/shipping")]
    [ApiController]
    public class ShippingApiController : ControllerBase
    {
        private readonly IShippingService _shippingService;

        public ShippingApiController(IShippingService shippingService)
        {
            _shippingService = shippingService;
        }

        // POST: api/shipping/calculate
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateShippingFee([FromBody] ShippingCalculateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Address))
            {
                return BadRequest(new { success = false, message = "Vui lòng nhập địa chỉ giao hàng" });
            }

            var result = await _shippingService.CalculateShippingFeeDetailedAsync(
                request.Address,
                request.OrderTotal
            );

            return Ok(new
            {
                success = result.Success,
                shippingFee = result.ShippingFee,
                isFreeShipping = result.IsFreeShipping,
                message = result.Message,
                areaName = result.AreaName,
                estimatedDistance = result.EstimatedDistance
            });
        }

        // GET: api/shipping/threshold
        [HttpGet("threshold")]
        public IActionResult GetFreeShippingThreshold()
        {
            var threshold = _shippingService.GetFreeShippingThreshold();
            return Ok(new { threshold });
        }
    }

    // DTO cho request
    public class ShippingCalculateRequest
    {
        public string Address { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
    }
}