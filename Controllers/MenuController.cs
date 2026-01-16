using FoodOrderingWeb.Data;
using FoodOrderingWeb.Models.Entities;
using FoodOrderingWeb.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingWeb.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Menu
        public async Task<IActionResult> Index(int? categoryId, string priceRange, bool? available, string searchString, int pageNumber = 1)
        {
            const int pageSize = 12; // 12 items per page

            var query = _context.Foods
                .Include(f => f.Category)
                .AsNoTracking();

            // Apply filters
            if (categoryId.HasValue)
            {
                query = query.Where(f => f.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(priceRange))
            {
                // Example: "0-30000" or "30000-50000" or "50000+"
                var prices = priceRange.Split('-');
                var minPrice = decimal.Parse(prices[0]);
                if (prices.Length > 1)
                {
                    var maxPrice = decimal.Parse(prices[1]);
                    query = query.Where(f => f.Price >= minPrice && f.Price <= maxPrice);
                }
                else
                {
                    // Handle "50000+" case
                    query = query.Where(f => f.Price >= minPrice);
                }
            }

            if (available.HasValue)
            {
                query = query.Where(f => f.IsAvailable == available.Value);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                // Sửa FoodName thành Name theo model
                query = query.Where(f => f.Name.Contains(searchString));
            }

            // Get categories for sidebar - bỏ OrderBy vì không có DisplayOrder
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            // Get total count and apply pagination
            var totalItems = await query.CountAsync();
            var foods = await query
                // Bỏ OrderByDescending vì không có CreatedDate
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new MenuViewModel
            {
                Foods = foods,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                CategoryId = categoryId,
                PriceRange = priceRange ?? string.Empty, // Thêm null check
                Available = available,
                SearchString = searchString ?? string.Empty, // Thêm null check
                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync() // Thêm Categories
            };

            return View(model);
        }

        // GET: /Menu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var food = await _context.Foods
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.FoodId == id);

            if (food == null)
            {
                return NotFound();
            }

            // Increment view count
            food.ViewCount++;
            await _context.SaveChangesAsync();

            // Get related foods (same category, exclude current)
            var relatedFoods = await _context.Foods
                .Where(f => f.CategoryId == food.CategoryId && f.FoodId != food.FoodId)
                .Take(4)
                .ToListAsync();

            var model = new FoodDetailsViewModel
            {
                Food = food,
                RelatedFoods = relatedFoods
            };

            return View(model);
        }
        // GET: Menu/GetFoodDetails/5
        [HttpGet]
        public async Task<IActionResult> GetFoodDetails(int id)
        {
            var food = await _context.Foods
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.FoodId == id);

            if (food == null)
            {
                return NotFound();
            }

            return Json(new
            {
                foodId = food.FoodId,
                name = food.Name,
                description = food.Description,
                price = food.Price,
                imageUrl = food.ImageUrl,
                categoryName = food.Category?.Name,
                isAvailable = food.IsAvailable
            });
        }
    }
}