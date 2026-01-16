using FoodOrderingWeb.Models.Entities;

namespace FoodOrderingWeb.Services
{
    public interface IShippingService
    {
        // ✅ Tính phí ship mặc định
        decimal GetDefaultShippingFee();

        // ✅ Tính phí ship theo địa chỉ (tự động nhận diện khu vực)
        Task<decimal> CalculateShippingFee(string address);

        // ✅ Tính phí ship chi tiết (trả về thông tin đầy đủ)
        Task<ShippingCalculationResult> CalculateShippingFeeDetailedAsync(string address, decimal orderTotal);

        // ✅ Kiểm tra miễn phí ship
        bool IsFreeShipping(decimal orderTotal);

        // ✅ Lấy ngưỡng miễn phí ship
        decimal GetFreeShippingThreshold();

        // ✅ CRUD cho Admin
        Task<List<ShippingConfig>> GetAllShippingConfigsAsync();
        Task<ShippingConfig?> GetShippingConfigByIdAsync(int id);
        Task<bool> CreateShippingConfigAsync(ShippingConfig config);
        Task<bool> UpdateShippingConfigAsync(ShippingConfig config);
        Task<bool> DeleteShippingConfigAsync(int id);

        // ✅ Tìm kiếm config theo khu vực
        Task<ShippingConfig?> FindShippingConfigByAddressAsync(string address);
    }

    // DTO kết quả tính phí ship
    public class ShippingCalculationResult
    {
        public bool Success { get; set; }
        public decimal ShippingFee { get; set; }
        public bool IsFreeShipping { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AreaName { get; set; }
        public decimal? EstimatedDistance { get; set; }
    }
}