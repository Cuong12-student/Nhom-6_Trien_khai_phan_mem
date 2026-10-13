using bhgbd.Data;
using bhgbd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bhgbd.Areas.Client.Controllers
{
    [Area("Client")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Client/Product/List
        public async Task<IActionResult> List(int? categoryId, string? sole, string? brand, string? price, string? search, string? q, string? sort)
        {
            string rawSearch = !string.IsNullOrWhiteSpace(search) ? search : (!string.IsNullOrWhiteSpace(q) ? q : "");
            string keyword = rawSearch.Trim().ToLower();

            string cleanSole = !string.IsNullOrWhiteSpace(sole) ? sole.Trim().ToUpper() : "";
            string cleanBrand = !string.IsNullOrWhiteSpace(brand) ? brand.Trim().ToLower() : "";
            string cleanPrice = !string.IsNullOrWhiteSpace(price) ? price.Trim().ToLower() : "";
            string cleanSort = !string.IsNullOrWhiteSpace(sort) ? sort.Trim().ToLower() : "";

            // Query lấy sản phẩm kèm Danh mục và Biến thể
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                .AsNoTracking()
                .AsQueryable();

            // 1. Lọc Danh mục
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.categoryId == categoryId.Value);
            }

            // 2. Lọc Loại đinh (Chỉ lọc NẾU chọn đinh cụ thể TF, FG, IC, AG)
            if (!string.IsNullOrEmpty(cleanSole) && cleanSole != "ALL")
            {
                query = query.Where(p => p.ProductVariants.Any(v => v.soleType != null && v.soleType.Trim().ToUpper() == cleanSole));
            }

            // 3. Lọc Thương hiệu
            if (!string.IsNullOrEmpty(cleanBrand) && cleanBrand != "all")
            {
                query = query.Where(p => (p.Category != null && p.Category.categoryName.ToLower().Contains(cleanBrand))
                                      || (p.productName != null && p.productName.ToLower().Contains(cleanBrand)));
            }

            // 4. Lọc Từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(keyword))
            {
                string kwNormal = keyword;
                string kwWithDash = keyword.Replace(" ", "-");
                string kwWithSpace = keyword.Replace("-", " ");

                query = query.Where(p => (p.productName != null && (p.productName.ToLower().Contains(kwNormal) || p.productName.ToLower().Contains(kwWithDash) || p.productName.ToLower().Contains(kwWithSpace)))
                                      || (p.description != null && p.description.ToLower().Contains(kwNormal)));
            }

            // 5. Lọc Khoảng giá
            if (!string.IsNullOrEmpty(cleanPrice) && cleanPrice != "all" && cleanPrice != "default")
            {
                switch (cleanPrice)
                {
                    case "under-2":
                        query = query.Where(p => p.price < 2000000);
                        break;
                    case "2-3":
                        query = query.Where(p => p.price >= 2000000 && p.price <= 3000000);
                        break;
                    case "over-3":
                        query = query.Where(p => p.price > 3000000);
                        break;
                }
            }

            // 6. Sắp xếp
            query = cleanSort switch
            {
                "price-asc" => query.OrderBy(p => p.price),
                "price-desc" => query.OrderByDescending(p => p.price),
                "new" => query.OrderByDescending(p => p.createdAt),
                "best" => query.OrderByDescending(p => p.createdAt),
                _ => query.OrderByDescending(p => p.productId)
            };

            var rawProducts = await query.ToListAsync();

            // Ánh xạ khớp 100% thuộc tính với ProductViewModel
            var productViewModels = rawProducts.Select(p => new ProductViewModel
            {
                Id = p.productId,
                Name = p.productName ?? "Sản phẩm",
                Brand = p.Category != null ? p.Category.categoryName : "BHGBD",
                Sole = p.ProductVariants.FirstOrDefault() != null ? p.ProductVariants.FirstOrDefault()!.soleType : "TF",
                PriceValue = p.price,
                OldPriceValue = null,
                Image = p.imageUrl ?? "/client/images/products/default.webp",
                Badge = "",
                Sizes = string.Join(" ", p.ProductVariants.Select(v => v.size.ToString()))
            }).ToList();

            ViewBag.Sole = sole;
            ViewBag.Brand = brand;
            ViewBag.Search = rawSearch;

            return View(productViewModels);
        }

        // GET: Client/Product/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.productId == id);

            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại!";
                return RedirectToAction(nameof(List));
            }

            var soleType = product.ProductVariants.FirstOrDefault()?.soleType ?? "TF";

            var detailModel = new ProductDetailViewModel
            {
                Id = product.productId,
                Name = product.productName,
                Brand = product.Category != null ? product.Category.categoryName : "BHGBD",
                Sole = soleType,
                SoleName = "Sân phù hợp",
                PriceValue = product.price,
                Summary = product.description,
                StoryTitle = "THÔNG TIN SẢN PHẨM",
                Description = product.description,
                Fit = "Thiết kế chuẩn form chân.",
                Sizes = product.ProductVariants.Select(v => v.size.ToString()).Distinct().ToList(),
                Images = new List<string> { product.imageUrl ?? "/client/images/products/default.webp" }
            };

            var relatedProducts = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                .AsNoTracking()
                .Where(p => p.categoryId == product.categoryId && p.productId != id)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts.Select(p => new ProductViewModel
            {
                Id = p.productId,
                Name = p.productName,
                Brand = product.Category != null ? product.Category.categoryName : "BHGBD",
                Sole = p.ProductVariants.FirstOrDefault() != null ? p.ProductVariants.FirstOrDefault()!.soleType : "TF",
                PriceValue = p.price,
                Image = p.imageUrl ?? "/client/images/products/default.webp"
            }).ToList();

            return View(detailModel);
        }
    }
}