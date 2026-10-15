using bhgbd.Data;
using bhgbd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace bhgbd.Areas.Client.Controllers
{
    [Area("Client")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CART_KEY = "CLIENT_CART"; // Dùng thống nhất key này cho toàn hệ thống

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Client/Order/Checkout
        public async Task<IActionResult> Checkout()
        {
            var cartJson = HttpContext.Session.GetString(CART_KEY);
            var cart = string.IsNullOrEmpty(cartJson) ? new CartViewModel() : JsonSerializer.Deserialize<CartViewModel>(cartJson)!;

            if (!cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                CartItems = cart.Items
            };

            int? currentUserId = HttpContext.Session.GetInt32("userId");
            if (currentUserId.HasValue)
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.userId == currentUserId.Value);
                if (customer != null)
                {
                    model.ReceiverName = customer.customerName ?? string.Empty;
                    model.ReceiverPhone = customer.phone ?? string.Empty;
                    model.ReceiverAddress = customer.address ?? string.Empty;
                }
            }

            return View(model);
        }

        // POST: Client/Order/CreateOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(CheckoutViewModel model)
        {
            // 1. Đọc dữ liệu giỏ hàng từ Session với đúng key CART_KEY ("CLIENT_CART")
            var cartJson = HttpContext.Session.GetString(CART_KEY);
            var cart = !string.IsNullOrEmpty(cartJson)
                ? JsonSerializer.Deserialize<CartViewModel>(cartJson)
                : new CartViewModel();

            if (cart == null || cart.Items == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "Cart");
            }

            // 2. Bỏ qua validation thuộc tính Note/Voucher không bắt buộc
            ModelState.Remove("Note");
            ModelState.Remove("Notes");
            ModelState.Remove("VoucherCode");

            // 3. Kiểm tra thông tin người nhận
            if (string.IsNullOrWhiteSpace(model.ReceiverName) ||
                string.IsNullOrWhiteSpace(model.ReceiverPhone) ||
                string.IsNullOrWhiteSpace(model.ReceiverAddress))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ Họ tên, Số điện thoại và Địa chỉ nhận hàng!";
                return RedirectToAction("Checkout");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy customerId theo userId đang đăng nhập hoặc lấy mặc định 1
                int currentCustomerId = 1;
                int? currentUserId = HttpContext.Session.GetInt32("userId");
                if (currentUserId.HasValue)
                {
                    var cust = await _context.Customers.FirstOrDefaultAsync(c => c.userId == currentUserId.Value);
                    if (cust != null) currentCustomerId = cust.customerId;
                }
                else if (HttpContext.Session.GetInt32("CUSTOMER_ID").HasValue)
                {
                    currentCustomerId = HttpContext.Session.GetInt32("CUSTOMER_ID")!.Value;
                }

                // 4. Tạo Order
                var order = new Order
                {
                    customerId = currentCustomerId,
                    staffId = null,
                    orderDate = DateTime.Now,
                    receiverName = model.ReceiverName.Trim(),
                    receiverPhone = model.ReceiverPhone.Trim(),
                    receiverAddress = model.ReceiverAddress.Trim(),
                    total = cart.GrandTotal,
                    status = OrderStatus.Pending
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 5. Tạo OrderDetails
                foreach (var item in cart.Items)
                {
                    int.TryParse(item.Size, out int itemSize);

                    var variant = await _context.ProductVariants
                        .FirstOrDefaultAsync(pv => pv.productId == item.ProductId && pv.size == itemSize);

                    if (variant == null)
                    {
                        variant = await _context.ProductVariants
                            .FirstOrDefaultAsync(pv => pv.productId == item.ProductId);
                    }

                    if (variant == null)
                    {
                        variant = new ProductVariant
                        {
                            productId = item.ProductId,
                            name = string.IsNullOrWhiteSpace(item.ProductName) ? $"Size {itemSize} - {item.Sole ?? "TF"}" : item.ProductName,
                            size = itemSize > 0 ? itemSize : 40,
                            soleType = string.IsNullOrEmpty(item.Sole) ? "TF" : item.Sole,
                            quantity = 100
                        };
                        _context.ProductVariants.Add(variant);
                        await _context.SaveChangesAsync();
                    }

                    var orderDetail = new OrderDetail
                    {
                        orderId = order.orderId,
                        variantId = variant.id,
                        price = item.UnitPriceValue,
                        quantity = item.Quantity
                    };

                    _context.OrderDetails.Add(orderDetail);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 6. Xóa giỏ hàng đúng key CART_KEY ("CLIENT_CART")
                HttpContext.Session.Remove(CART_KEY);

                TempData["Success"] = $"Đặt hàng thành công! Mã đơn hàng của bạn là #BH{order.orderId:D6}";
                return RedirectToAction("History", "Order");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                TempData["Error"] = "Lỗi xử lý đặt hàng: " + innerMsg;
                return RedirectToAction("Checkout");
            }
        }

        // GET: Client/Order/History
        public async Task<IActionResult> History(string status = "all")
        {
            int currentCustomerId = 1;
            int? currentUserId = HttpContext.Session.GetInt32("userId");
            if (currentUserId.HasValue)
            {
                var cust = await _context.Customers.FirstOrDefaultAsync(c => c.userId == currentUserId.Value);
                if (cust != null) currentCustomerId = cust.customerId;
            }

            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.customerId == currentCustomerId)
                .OrderByDescending(o => o.orderDate)
                .AsNoTracking()
                .ToListAsync();

            var variants = await _context.ProductVariants
                .Include(pv => pv.Product)
                .AsNoTracking()
                .ToDictionaryAsync(pv => pv.id, pv => pv);

            ViewBag.TotalCount = orders.Count;
            ViewBag.PendingCount = orders.Count(o => o.status == OrderStatus.Pending);
            ViewBag.ConfirmedCount = orders.Count(o => o.status == OrderStatus.Confirmed);
            ViewBag.CancelledCount = orders.Count(o => o.status == OrderStatus.Cancelled);

            if (!string.IsNullOrEmpty(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                orders = orders.Where(o => o.status.ToString().Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var orderViewModels = orders.Select(o =>
            {
                var items = o.OrderDetails.Select(od =>
                {
                    variants.TryGetValue(od.variantId, out var variant);
                    var product = variant?.Product;

                    return new OrderItemViewModel
                    {
                        ProductName = product != null ? product.productName : "Giày bóng đá",
                        Size = variant != null ? variant.size.ToString() : "40",
                        Quantity = od.quantity,
                        Price = od.price,
                        Image = product?.imageUrl ?? "/client/images/products/default.webp"
                    };
                }).ToList();

                return new OrderViewModel
                {
                    Id = o.orderId,
                    OrderCode = $"BH{o.orderId:D6}",
                    CreatedDate = o.orderDate,
                    Status = o.status.ToString(),
                    GrandTotal = o.total,
                    Items = items
                };
            }).ToList();

            return View(orderViewModels);
        }

        // POST: Client/Order/CancelOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.orderId == orderId);
            if (order != null && order.status == OrderStatus.Pending)
            {
                order.status = OrderStatus.CancelRequested;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã gửi yêu cầu hủy đơn hàng #{orderId}!";
            }
            return RedirectToAction(nameof(History));
        }
    }
}