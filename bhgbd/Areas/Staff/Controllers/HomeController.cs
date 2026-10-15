using bhgbd.Data;
using bhgbd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bhgbd.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Staff/Home/Index (Dashboard)
        public async Task<IActionResult> Index()
        {
            // 1. Thống kê số lượng đơn theo Enum OrderStatus (Dùng AsNoTracking để tối ưu hiệu năng)
            ViewBag.PendingCount = await _context.Orders
                .AsNoTracking()
                .CountAsync(o => o.status == OrderStatus.Pending);

            ViewBag.CancelRequestedCount = await _context.Orders
                .AsNoTracking()
                .CountAsync(o => o.status == OrderStatus.CancelRequested);

            ViewBag.ConfirmedCount = await _context.Orders
                .AsNoTracking()
                .CountAsync(o => o.status == OrderStatus.Confirmed);

            // 2. Lấy 10 đơn hàng mới nhất kèm thông tin Khách hàng
            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.orderDate)
                .Take(10)
                .AsNoTracking()
                .ToListAsync();

            return View(recentOrders);
        }

        // POST: Staff/Home/ApproveOrder (Xử lý nút Duyệt đơn)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null && order.status == OrderStatus.Pending)
            {
                // Lấy staffId của nhân viên đang đăng nhập từ Session (Mặc định = 1 nếu chưa đăng nhập)
                int currentStaffId = HttpContext.Session.GetInt32("STAFF_ID") ?? 1;

                order.status = OrderStatus.Confirmed;
                order.staffId = currentStaffId; // Gán nhân viên phụ trách duyệt đơn

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã duyệt đơn hàng #BH{order.orderId:D6} thành công!";
            }
            else
            {
                TempData["Error"] = "Đơn hàng không hợp lệ hoặc đã được xử lý trước đó!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Staff/Home/ConfirmCancel (Xử lý nút Hủy đơn)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCancel(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null && (order.status == OrderStatus.Pending || order.status == OrderStatus.CancelRequested))
            {
                int currentStaffId = HttpContext.Session.GetInt32("STAFF_ID") ?? 1;

                order.status = OrderStatus.Cancelled;
                order.staffId = currentStaffId; // Gán nhân viên phụ trách hủy đơn

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã xác nhận hủy đơn hàng #BH{order.orderId:D6}!";
            }
            else
            {
                TempData["Error"] = "Đơn hàng không thể hủy!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}