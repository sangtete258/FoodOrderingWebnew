using FoodOrderingWeb.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingWeb.Data
{
    public class ShippingSeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Kiểm tra đã có dữ liệu chưa
            if (await context.ShippingConfigs.AnyAsync())
            {
                return; // Đã có dữ liệu rồi
            }

            var shippingConfigs = new List<ShippingConfig>
            {
                // 1. NỘI THÀNH NHA TRANG
                new ShippingConfig
                {
                    AreaName = "Nội thành Nha Trang",
                    Description = "Các phường trung tâm thành phố Nha Trang",
                    ShippingFee = 15000,
                    EstimatedDistance = 5,
                    IsActive = true,
                    SearchKeywords = "nha trang,phuong loc tho,phuong phuoc hai,phuong phuoc hoa,phuong phuoc long,phuong phuoc tan,phuong phuoc tien,phuong tan lap,phuong vinh hai,phuong vinh hoa,phuong vinh nguyen,phuong vinh phu,phuong vinh phuoc,phuong vinh truong,phuong xuan an,phuong phuoc dong,phuong van thanh,phuong van thang,phuong ngoc hiep",
                    CreatedDate = DateTime.Now
                },

                // 2. KHU VỰC NINH HÒA
                new ShippingConfig
                {
                    AreaName = "Ninh Hòa",
                    Description = "Thị xã Ninh Hòa và các xã lân cận",
                    ShippingFee = 25000,
                    EstimatedDistance = 25,
                    IsActive = true,
                    SearchKeywords = "ninh hoa,ninh loc,ninh da,ninh ich,ninh sim,ninh an,ninh binh,ninh quoi,ninh phuoc,ninh ha,ninh trung,ninh diem,ninh huan,ninh phu,ninh tay,ninh phu",
                    CreatedDate = DateTime.Now
                },

                // 3. KHU VỰC CAM RANH
                new ShippingConfig
                {
                    AreaName = "Cam Ranh",
                    Description = "Thành phố Cam Ranh, sân bay Cam Ranh",
                    ShippingFee = 30000,
                    EstimatedDistance = 30,
                    IsActive = true,
                    SearchKeywords = "cam ranh,cam lap,cam phuoc dong,cam phuoc tay,cam lien hoa,cam nghia an,cam thuan an,cam thuan bac,cam hai dong,cam hai tay,cam phu",
                    CreatedDate = DateTime.Now
                },

                // 4. KHU VỰC DIÊN KHÁNH
                new ShippingConfig
                {
                    AreaName = "Diên Khánh",
                    Description = "Huyện Diên Khánh và các xã thuộc huyện",
                    ShippingFee = 35000,
                    EstimatedDistance = 20,
                    IsActive = true,
                    SearchKeywords = "dien khanh,dien dien,dien phu,dien lam,dien thang,dien toan,dien trung,dien xuan,dien lac,dien tan,dien thai,dien tho,dien phuoc,dien son,dien binh,dien hoa,dien an,dien phuong",
                    CreatedDate = DateTime.Now
                },

                // 5. KHU VỰC VẠN NINH
                new ShippingConfig
                {
                    AreaName = "Vạn Ninh",
                    Description = "Huyện Vạn Ninh và các xã xa trung tâm",
                    ShippingFee = 50000,
                    EstimatedDistance = 60,
                    IsActive = true,
                    SearchKeywords = "van ninh,van long,van phuoc,van hung,van thang,van thinh,van hai,van lam,van thanh,van thoi,van binh,van phuc,van gia",
                    CreatedDate = DateTime.Now
                },

                // 6. KHU VỰC NINH HẢI (XA)
                new ShippingConfig
                {
                    AreaName = "Ninh Hải và vùng xa",
                    Description = "Huyện Ninh Hải và các vùng xa trung tâm",
                    ShippingFee = 60000,
                    EstimatedDistance = 80,
                    IsActive = true,
                    SearchKeywords = "ninh hai,ninh son,ninh van,ninh phu,ninh binh,ninh dieu,ninh dong,ninh hoa,ninh kich,ninh my,ninh phuoc,ninh que,ninh tho,ninh thai,ninh thuy",
                    CreatedDate = DateTime.Now
                },

                // 7. KHU VỰC VINPEARL / BẢI DÀI
                new ShippingConfig
                {
                    AreaName = "Vinpearl / Bãi Dài / Sân bay",
                    Description = "Khu vực resort, Vinpearl, Bãi Dài, Sân bay Cam Ranh",
                    ShippingFee = 40000,
                    EstimatedDistance = 35,
                    IsActive = true,
                    SearchKeywords = "vinpearl,bai dai,hon tre,san bay cam ranh,cam ranh airport,resort,khu nghi duong",
                    CreatedDate = DateTime.Now
                },

                // 8. ĐẢO (GIÁ ĐẶC BIỆT)
                new ShippingConfig
                {
                    AreaName = "Các đảo (Hòn Tằm, Hòn Mun...)",
                    Description = "Giao hàng ra đảo (cần xác nhận trước)",
                    ShippingFee = 100000,
                    EstimatedDistance = 10,
                    IsActive = true,
                    SearchKeywords = "hon tam,hon mun,hon mot,hon mieu,dao,island",
                    CreatedDate = DateTime.Now
                }
            };

            await context.ShippingConfigs.AddRangeAsync(shippingConfigs);
            await context.SaveChangesAsync();
        }
    }
}