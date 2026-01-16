using FoodOrderingWeb.Models;
using FoodOrderingWeb.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingWeb.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Đảm bảo database đã được tạo
            await context.Database.MigrateAsync();

            // Tạo roles
            await SeedRoles(roleManager);

            // Tạo admin user
            await SeedAdminUser(userManager);

            // Tạo categories và foods
            await SeedCategories(context);
            await SeedFoods(context);

            // ✅ THÊM MỚI: Seed Shipping Config
            await ShippingSeedData.Initialize(serviceProvider);
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedAdminUser(UserManager<IdentityUser> userManager)
        {
            string adminUsername = "sangxz123";
            string adminPassword = "Sang@23112005";
            string adminEmail = "anhbaga@foodordering.com";

            // Tìm theo username thay vì email
            if (await userManager.FindByNameAsync(adminUsername) == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = adminUsername,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        private static async Task SeedCategories(ApplicationDbContext context)
        {
            if (await context.Categories.AnyAsync())
            {
                return;
            }

            var categories = new List<Category>
            {
                new Category { Name = "Món khai vị", Description = "Các món ăn khai vị" },
                new Category { Name = "Món chính", Description = "Các món ăn chính" },
                new Category { Name = "Món tráng miệng", Description = "Các món tráng miệng" },
                new Category { Name = "Đồ uống", Description = "Các loại đồ uống" },
                new Category { Name = "Món Á", Description = "Các món ăn Châu Á" },
                new Category { Name = "Món Âu", Description = "Các món ăn Châu Âu" }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        private static async Task SeedFoods(ApplicationDbContext context)
        {
            if (await context.Foods.AnyAsync())
            {
                return;
            }

            var categories = await context.Categories.ToListAsync();

            var foods = new List<Food>
            {
                new Food
                {
                    Name = "Gỏi cuốn",
                    Description = "Gỏi cuốn tôm thịt tươi ngon",
                    Price = 25000,
                    ImageUrl = "https://via.placeholder.com/300x200/FF6B6B/FFFFFF?text=Goi+Cuon",
                    CategoryId = categories.First(c => c.Name == "Món khai vị").CategoryId,
                    IsAvailable = true
                },
                new Food
                {
                    Name = "Chả giò",
                    Description = "Chả giò giòn rụm truyền thống",
                    Price = 30000,
                    ImageUrl = "https://via.placeholder.com/300x200/4ECDC4/FFFFFF?text=Cha+Gio",
                    CategoryId = categories.First(c => c.Name == "Món khai vị").CategoryId,
                    IsAvailable = true
                },
                new Food
                {
                    Name = "Phở bò",
                    Description = "Phở bò truyền thống Hà Nội",
                    Price = 50000,
                    ImageUrl = "https://via.placeholder.com/300x200/FFE66D/333333?text=Pho+Bo",
                    CategoryId = categories.First(c => c.Name == "Món chính").CategoryId,
                    IsAvailable = true
                },
                new Food
                {
                    Name = "Bún chả",
                    Description = "Bún chả Hà Nội đặc sản",
                    Price = 45000,
                    ImageUrl = "https://via.placeholder.com/300x200/A8E6CF/333333?text=Bun+Cha",
                    CategoryId = categories.First(c => c.Name == "Món chính").CategoryId,
                    IsAvailable = true
                },
                new Food
                {
                    Name = "Cơm tấm sườn",
                    Description = "Cơm tấm sườn nướng Sài Gòn",
                    Price = 40000,
                    ImageUrl = "https://via.placeholder.com/300x200/FF8B94/FFFFFF?text=Com+Tam",
                    CategoryId = categories.First(c => c.Name == "Món chính").CategoryId,
                    IsAvailable = true
                },
                new Food
                {
                    Name = "Bún bò Huế",
                    Description = "Bún bò Huế cay nồng đậm đà",
                    Price = 48000,
                    ImageUrl = "https://via.placeholder.com/300x200/C7CEEA/333333?text=Bun+Bo+Hue",
                    CategoryId = categories.First(c => c.Name == "Món chính").CategoryId,
                    IsAvailable = true
                },
                new Food
                {
                    Name = "Trà đá",
                    Description = "Trà đá mát lạnh",
                    Price = 5000,
                    ImageUrl = "https://via.placeholder.com/300x200/B4E7CE/333333?text=Tra+Da",
                    CategoryId = categories.First(c => c.Name == "Đồ uống").CategoryId,
                    IsAvailable = true
                },
                new Food
                {
                    Name = "Cà phê sữa đá",
                    Description = "Cà phê sữa đá truyền thống",
                    Price = 20000,
                    ImageUrl = "https://via.placeholder.com/300x200/8B4513/FFFFFF?text=Ca+Phe",
                    CategoryId = categories.First(c => c.Name == "Đồ uống").CategoryId,
                    IsAvailable = true
                },
                new Food
                {
                    Name = "Nước chanh",
                    Description = "Nước chanh tươi mát lạnh",
                    Price = 15000,
                    ImageUrl = "https://via.placeholder.com/300x200/FFFF00/333333?text=Nuoc+Chanh",
                    CategoryId = categories.First(c => c.Name == "Đồ uống").CategoryId,
                    IsAvailable = true
                },
                new Food
                {
                    Name = "Chè ba màu",
                    Description = "Chè ba màu truyền thống",
                    Price = 18000,
                    ImageUrl = "https://via.placeholder.com/300x200/DDA0DD/FFFFFF?text=Che+Ba+Mau",
                    CategoryId = categories.First(c => c.Name == "Món tráng miệng").CategoryId,
                    IsAvailable = true
                },
                new Food
                {
                    Name = "Kem dừa",
                    Description = "Kem dừa mát lạnh",
                    Price = 22000,
                    ImageUrl = "https://via.placeholder.com/300x200/F5F5DC/333333?text=Kem+Dua",
                    CategoryId = categories.First(c => c.Name == "Món tráng miệng").CategoryId,
                    IsAvailable = true
                }
            };

            context.Foods.AddRange(foods);
            await context.SaveChangesAsync();
        }
    }
}