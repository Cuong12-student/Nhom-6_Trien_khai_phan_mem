using bhgbd.Data;
using bhgbd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bhgbd.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            return RedirectToAction("Product", new { searchString, categoryId });
        }

        public async Task<IActionResult> Product(string searchString, int? categoryId)
        {
            var query = _context.Products.Include(p => p.Category).Include(p => p.ProductVariants).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(p => p.productName.Contains(searchString) || p.description.Contains(searchString));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.categoryId == categoryId.Value);
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.isActive).ToListAsync();
            ViewBag.SearchString = searchString;
            ViewBag.SelectedCategoryId = categoryId;

            var products = await query.OrderByDescending(p => p.createdAt).ToListAsync();
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                int maxId = await _context.Products.MaxAsync(p => (int?)p.productId) ?? 0;
                product.productId = maxId + 1;
                if (string.IsNullOrWhiteSpace(product.imageUrl))
                {
                    product.imageUrl = "/client/images/products/default-product.webp";
                }
                else if (!product.imageUrl.StartsWith("http") && !product.imageUrl.StartsWith("/client"))
                {
                    product.imageUrl = "/client" + (product.imageUrl.StartsWith("/") ? "" : "/") + product.imageUrl;
                }
                product.createdAt = DateTime.Now;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm sản phẩm mới thành công!";
            }
            else
            {
                TempData["Error"] = "Thêm sản phẩm không thành công. Dữ liệu nhập không hợp lệ!";
            }
            return RedirectToAction(nameof(Product));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.Products.FindAsync(product.productId);
                if (existing != null)
                {
                    existing.productName = product.productName;
                    existing.description = product.description;
                    existing.price = product.price;
                    existing.categoryId = product.categoryId;
                    if (!string.IsNullOrWhiteSpace(product.imageUrl))
                    {
                        existing.imageUrl = product.imageUrl;
                    }
                    _context.Products.Update(existing);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật sản phẩm thành công!";
                }
            }
            else
            {
                TempData["Error"] = "Cập nhật sản phẩm thất bại!";
            }
            return RedirectToAction(nameof(Product));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa sản phẩm thành công!";
            }
            return RedirectToAction(nameof(Product));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVariant(int productId, string name, int size, string soleType, int quantity)
        {
            int maxId = await _context.ProductVariants.MaxAsync(v => (int?)v.id) ?? 0;
            var variant = new ProductVariant
            {
                productId = productId,
                name = string.IsNullOrWhiteSpace(name) ? $"Size {size} - {soleType}" : name,
                size = size,
                soleType = soleType,
                quantity = quantity
            };

            _context.ProductVariants.Add(variant);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã thêm biến thể (Size {size}) cho sản phẩm!";
            return RedirectToAction(nameof(Product));
        }
    }
}