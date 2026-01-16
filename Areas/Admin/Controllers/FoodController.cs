using FoodOrderingWeb.Data;
using FoodOrderingWeb.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FoodOrderingWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FoodController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FoodController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Food
        public async Task<IActionResult> Index(string searchString, int? categoryId, int page = 1)
        {
            int pageSize = 10;
            var foods = _context.Foods.Include(f => f.Category).AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(searchString))
            {
                foods = foods.Where(f => (f.Name != null && f.Name.Contains(searchString)) ||
                         (f.Description != null && f.Description.Contains(searchString)));
                ViewBag.SearchString = searchString;
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                foods = foods.Where(f => f.CategoryId == categoryId);
                ViewBag.CategoryId = categoryId;
            }

            // Pagination
            var totalItems = await foods.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var foodList = await foods
                .OrderByDescending(f => f.FoodId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            // Categories for filter
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");

            return View(foodList);
        }

        // GET: Admin/Food/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
            return View();
        }

        // POST: Admin/Food/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Food food)
        {
            if (ModelState.IsValid)
            {
                _context.Foods.Add(food);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm món ăn thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name", food.CategoryId);
            return View(food);
        }

        // GET: Admin/Food/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var food = await _context.Foods.FindAsync(id);
            if (food == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name", food.CategoryId);
            return View(food);
        }

        // POST: Admin/Food/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Food food)
        {
            if (id != food.FoodId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(food);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật món ăn thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FoodExists(food.FoodId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name", food.CategoryId);
            return View(food);
        }

        // GET: Admin/Food/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var food = await _context.Foods
                .Include(f => f.Category)
                .FirstOrDefaultAsync(m => m.FoodId == id);

            if (food == null)
            {
                return NotFound();
            }

            return View(food);
        }

        // POST: Admin/Food/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food != null)
            {
                _context.Foods.Remove(food);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa món ăn thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FoodExists(int id)
        {
            return _context.Foods.Any(e => e.FoodId == id);
        }
    }
}