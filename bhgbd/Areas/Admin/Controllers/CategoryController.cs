using bhgbd.Data;
using bhgbd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bhgbd.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Category");
        }

        public async Task<IActionResult> Category()
        {
            var categories = await _context.Categories.OrderBy(c => c.categoryName).ToListAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.ActiveCategories = await _context.Categories.CountAsync(c => c.isActive);
            ViewBag.InactiveCategories = await _context.Categories.CountAsync(c => !c.isActive);
            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                int maxId = await _context.Categories.MaxAsync(c => (int?)c.categoryId) ?? 0;
                category.categoryId = maxId + 1;
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm danh mục mới thành công!";
            }
            else
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin danh mục!";
            }
            return RedirectToAction(nameof(Category));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.Categories.FindAsync(category.categoryId);
                if (existing != null)
                {
                    existing.categoryName = category.categoryName;
                    existing.description = category.description;
                    existing.isActive = category.isActive;
                    _context.Categories.Update(existing);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật danh mục thành công!";
                }
            }
            else
            {
                TempData["Error"] = "Cập nhật không thành công. Dữ liệu không hợp lệ!";
            }
            return RedirectToAction(nameof(Category));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                category.isActive = !category.isActive;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã {(category.isActive ? "kích hoạt" : "ẩn")} danh mục '{category.categoryName}'!";
            }
            return RedirectToAction(nameof(Category));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                // Kiểm tra xem danh mục có chứa sản phẩm không
                var hasProducts = await _context.Products.AnyAsync(p => p.categoryId == id);
                if (hasProducts)
                {
                    TempData["Error"] = "Không thể xóa danh mục đang có chứa sản phẩm!";
                }
                else
                {
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Xóa danh mục thành công!";
                }
            }
            return RedirectToAction(nameof(Category));
        }
    }
}
