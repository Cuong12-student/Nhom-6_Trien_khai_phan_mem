using bhgbd.Data;
using bhgbd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bhgbd.Areas.Client.Controllers
{
    [Area("Client")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Lấy danh mục (nếu có isActive)
            ViewBag.Categories = await _context.Categories
                .Where(c => c.isActive)
                .ToListAsync();

            // 2. Lấy 8 sản phẩm mới nhất kèm biến thể (Bỏ điều kiện IsActive)
            var bestSellers = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                .OrderByDescending(p => p.createdAt)
                .Take(8)
                .Select(p => new ProductViewModel
                {
                    Id = p.productId,
                    Name = p.productName,
                    Brand = p.Category != null ? p.Category.categoryName : "BHGBD",
                    Sole = p.ProductVariants.FirstOrDefault() != null
                           ? p.ProductVariants.FirstOrDefault().soleType
                           : "TF",
                    // Lấy giá từ biến thể đầu tiên của sản phẩm
                    PriceValue = p.price,
                    OldPriceValue = null,
                    Image = p.imageUrl ?? "/client/images/products/default.webp",
                    Badge = "",
                    Sizes = string.Join(" ", p.ProductVariants.Select(v => v.size))
                })
                .ToListAsync();

            // 3. Truyền ViewModel sang View Index.cshtml
            return View(bestSellers);
        }
    }
}