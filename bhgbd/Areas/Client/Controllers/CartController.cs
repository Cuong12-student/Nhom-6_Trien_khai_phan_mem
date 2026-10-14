using bhgbd.Data;
using bhgbd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace bhgbd.Areas.Client.Controllers
{
    [Area("Client")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CART_KEY = "CLIENT_CART";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Client/Cart
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View("Cart",cart);
        }

        // POST: Client/Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, string size, int quantity)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.productId == productId);

            if (product == null) return NotFound();

            int sizeInt = int.TryParse(size, out var s) ? s : 40;
            var variant = product.ProductVariants.FirstOrDefault(v => v.size == sizeInt)
                          ?? product.ProductVariants.FirstOrDefault();

            var cart = GetCartFromSession();
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.Size == size);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItemViewModel
                {
                    CartItemId = variant?.id ?? 0,
                    ProductId = product.productId,
                    ProductName = product.productName,
                    Brand = product.Category != null ? product.Category.categoryName : "BHGBD",
                    Sole = variant != null ? variant.soleType : "TF",
                    Size = size ?? "40",
                    Image = product.imageUrl ?? "/client/images/products/default.webp",
                    UnitPriceValue = product.price,
                    Quantity = quantity
                });
            }

            SaveCartToSession(cart);
            return RedirectToAction(nameof(Index));
        }

        // POST: Client/Cart/UpdateQuantity
        [HttpPost]
        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            var cart = GetCartFromSession();
            var item = cart.Items.FirstOrDefault(i => i.CartItemId == cartItemId);
            if (item != null)
            {
                if (quantity <= 0) cart.Items.Remove(item);
                else item.Quantity = quantity;
            }
            SaveCartToSession(cart);
            return RedirectToAction(nameof(Index));
        }

        // POST: Client/Cart/RemoveItem
        [HttpPost]
        public IActionResult RemoveItem(int cartItemId)
        {
            var cart = GetCartFromSession();
            cart.Items.RemoveAll(i => i.CartItemId == cartItemId);
            SaveCartToSession(cart);
            return RedirectToAction(nameof(Index));
        }

        #region Helper Session
        private CartViewModel GetCartFromSession()
        {
            var json = HttpContext.Session.GetString(CART_KEY);
            return string.IsNullOrEmpty(json) ? new CartViewModel() : JsonSerializer.Deserialize<CartViewModel>(json)!;
        }

        private void SaveCartToSession(CartViewModel cart)
        {
            HttpContext.Session.SetString(CART_KEY, JsonSerializer.Serialize(cart));
        }
        #endregion
    }
}