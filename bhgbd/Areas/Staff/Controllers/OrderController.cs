using bhgbd.Data;
using bhgbd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bhgbd.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH ĐƠN HÀNG + BỘ LỌC TABS + TÌM KIẾM
        public async Task<IActionResult> Index(string statusFilter, string searchString)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .AsNoTracking()
                .AsQueryable();

            // Lọc theo Tab trạng thái
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                if (Enum.TryParse<OrderStatus>(statusFilter, out var parsedStatus))
                {
                    query = query.Where(o => o.status == parsedStatus);
                }
            }

            // Tìm kiếm theo Mã đơn, Tên người nhận, SĐT
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim().ToLower();
                query = query.Where(o => o.orderId.ToString().Contains(searchString) ||
                                         o.receiverName.ToLower().Contains(searchString) ||
                                         o.receiverPhone.Contains(searchString));
            }

            // Đếm số lượng đơn cho Badges trên Tabs
            ViewBag.AllCount = await _context.Orders.AsNoTracking().CountAsync();
            ViewBag.PendingCount = await _context.Orders.AsNoTracking().CountAsync(o => o.status == OrderStatus.Pending);
            ViewBag.ConfirmedCount = await _context.Orders.AsNoTracking().CountAsync(o => o.status == OrderStatus.Confirmed);
            ViewBag.CancelReqCount = await _context.Orders.AsNoTracking().CountAsync(o => o.status == OrderStatus.CancelRequested);
            ViewBag.CancelledCount = await _context.Orders.AsNoTracking().CountAsync(o => o.status == OrderStatus.Cancelled);

            ViewBag.CurrentFilter = statusFilter ?? "All";
            ViewBag.SearchString = searchString;

            var orders = await query.OrderByDescending(o => o.orderDate).ToListAsync();
            return View(orders);
        }

        // 2. CHI TIẾT ĐƠN HÀNG
        public async Task<IActionResult> Detail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.orderId == id);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        // 3. XỬ LÝ DUYỆT ĐƠN (Pending -> Confirmed)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null && order.status == OrderStatus.Pending)
            {
                int currentUserId = HttpContext.Session.GetInt32("STAFF_ID") ?? HttpContext.Session.GetInt32("userId") ?? 1;

                order.status = OrderStatus.Confirmed;
                order.staffId = currentUserId;

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã duyệt thành công đơn hàng #ORD-{order.orderId}!";
            }
            else
            {
                TempData["Error"] = "Xử lý duyệt đơn thất bại!";
            }

            return RedirectToAction(nameof(Index));
        }

        // 4. XỬ LÝ CHẤP NHẬN HỦY ĐƠN (CancelRequested -> Cancelled)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptCancelOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order != null && (order.status == OrderStatus.Pending || order.status == OrderStatus.CancelRequested))
            {
                int currentUserId = HttpContext.Session.GetInt32("STAFF_ID") ?? HttpContext.Session.GetInt32("userId") ?? 1;

                order.status = OrderStatus.Cancelled;
                order.staffId = currentUserId;

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã xác nhận hủy đơn hàng #ORD-{order.orderId}!";
            }
            else
            {
                TempData["Error"] = "Xử lý hủy đơn thất bại!";
            }

            return RedirectToAction(nameof(Index));
        }

        // 5. CẬP NHẬT TRẠNG THÁI TỪ DROPDOWN TRONG TRANG DETAIL
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                int currentUserId = HttpContext.Session.GetInt32("STAFF_ID") ?? HttpContext.Session.GetInt32("userId") ?? 1;
                order.status = status;
                order.staffId = currentUserId;

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Cập nhật trạng thái đơn hàng #ORD-{order.orderId} thành công!";
            }

            return RedirectToAction(nameof(Detail), new { id = orderId });
        }
    }
}