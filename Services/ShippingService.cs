using FoodOrderingWeb.Data;
using FoodOrderingWeb.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FoodOrderingWeb.Services
{
    public class ShippingService : IShippingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ShippingService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ✅ Lấy phí ship mặc định từ appsettings.json
        public decimal GetDefaultShippingFee()
        {
            return _configuration.GetValue<decimal>("ShippingSettings:DefaultFee", 15000);
        }

        // ✅ Lấy ngưỡng miễn phí ship
        public decimal GetFreeShippingThreshold()
        {
            return _configuration.GetValue<decimal>("ShippingSettings:FreeShippingThreshold", 200000);
        }

        // ✅ Kiểm tra miễn phí ship
        public bool IsFreeShipping(decimal orderTotal)
        {
            var threshold = GetFreeShippingThreshold();
            return orderTotal >= threshold;
        }

        // ✅ Tính phí ship đơn giản (chỉ trả về số tiền)
        public async Task<decimal> CalculateShippingFee(string address)
        {
            // Tìm config dựa trên địa chỉ
            var config = await FindShippingConfigByAddressAsync(address);

            if (config != null && config.IsActive)
            {
                return config.ShippingFee;
            }

            // Nếu không tìm thấy, dùng phí mặc định
            return GetDefaultShippingFee();
        }

        // ✅ Tính phí ship chi tiết (có kiểm tra miễn phí)
        public async Task<ShippingCalculationResult> CalculateShippingFeeDetailedAsync(string address, decimal orderTotal)
        {
            try
            {
                // Kiểm tra miễn phí ship
                var freeShippingThreshold = GetFreeShippingThreshold();
                if (orderTotal >= freeShippingThreshold)
                {
                    return new ShippingCalculationResult
                    {
                        Success = true,
                        ShippingFee = 0,
                        IsFreeShipping = true,
                        Message = $"🎉 Miễn phí ship cho đơn hàng từ {freeShippingThreshold:N0}đ"
                    };
                }

                // Tìm config dựa trên địa chỉ
                var config = await FindShippingConfigByAddressAsync(address);

                if (config != null && config.IsActive)
                {
                    return new ShippingCalculationResult
                    {
                        Success = true,
                        ShippingFee = config.ShippingFee,
                        IsFreeShipping = false,
                        Message = $"Phí ship: {config.ShippingFee:N0}đ",
                        AreaName = config.AreaName,
                        EstimatedDistance = config.EstimatedDistance
                    };
                }

                // Không tìm thấy config, dùng phí mặc định
                var defaultFee = GetDefaultShippingFee();
                return new ShippingCalculationResult
                {
                    Success = true,
                    ShippingFee = defaultFee,
                    IsFreeShipping = false,
                    Message = $"Phí ship: {defaultFee:N0}đ",
                    AreaName = "Khu vực khác"
                };
            }
            catch (Exception)
            {
                // Log error nếu cần
                return new ShippingCalculationResult
                {
                    Success = false,
                    ShippingFee = GetDefaultShippingFee(),
                    IsFreeShipping = false,
                    Message = "Có lỗi khi tính phí ship. Áp dụng phí mặc định."
                };
            }
        }

        // ✅ Tìm config dựa trên địa chỉ (tìm theo từ khóa)
        public async Task<ShippingConfig?> FindShippingConfigByAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            var addressLower = address.ToLower().Trim();

            // Lấy tất cả config đang active
            var configs = await _context.ShippingConfigs
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.ShippingFee) // Ưu tiên config có phí cao hơn (thường là khu vực xa hơn)
                .ToListAsync();

            // Tìm config phù hợp nhất
            foreach (var config in configs)
            {
                // Kiểm tra tên khu vực
                if (addressLower.Contains(config.AreaName.ToLower()))
                {
                    return config;
                }

                // Kiểm tra từ khóa tìm kiếm (nếu có)
                if (!string.IsNullOrWhiteSpace(config.SearchKeywords))
                {
                    var keywords = config.SearchKeywords.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var keyword in keywords)
                    {
                        if (addressLower.Contains(keyword.Trim().ToLower()))
                        {
                            return config;
                        }
                    }
                }
            }

            return null;
        }

        // ========================================
        // CRUD CHO ADMIN
        // ========================================

        public async Task<List<ShippingConfig>> GetAllShippingConfigsAsync()
        {
            return await _context.ShippingConfigs
                .OrderBy(c => c.AreaName)
                .ToListAsync();
        }

        public async Task<ShippingConfig?> GetShippingConfigByIdAsync(int id)
        {
            return await _context.ShippingConfigs.FindAsync(id);
        }

        public async Task<bool> CreateShippingConfigAsync(ShippingConfig config)
        {
            try
            {
                config.CreatedDate = DateTime.Now;
                _context.ShippingConfigs.Add(config);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateShippingConfigAsync(ShippingConfig config)
        {
            try
            {
                config.ModifiedDate = DateTime.Now;
                _context.ShippingConfigs.Update(config);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteShippingConfigAsync(int id)
        {
            try
            {
                var config = await _context.ShippingConfigs.FindAsync(id);
                if (config == null) return false;

                _context.ShippingConfigs.Remove(config);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}