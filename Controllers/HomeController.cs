using FoodOrderingWeb.Data;
using FoodOrderingWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            var popularFoods = await _context.Foods
                .Include(f => f.Category)
                .Where(f => f.IsAvailable)
                .OrderByDescending(f => f.ViewCount)
                .Take(6)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                Categories = categories,
                PopularFoods = popularFoods
            };

            return View(viewModel);
        }
    }
}