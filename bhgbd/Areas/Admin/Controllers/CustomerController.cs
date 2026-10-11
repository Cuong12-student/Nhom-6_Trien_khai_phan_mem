using bhgbd.Data;
using bhgbd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bhgbd.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            // Lấy danh sách Khách hàng chính thức có User role là Customer
            var query = _context.Customers
                .Include(c => c.User)
                .Where(c => c.User != null && c.User.role == UserRole.Customer)
                .AsQueryable();

            // Tìm kiếm theo tên, số điện thoại hoặc email
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                string searchLower = searchString.Trim().ToLower();
                query = query.Where(c => (c.customerName != null && c.customerName.ToLower().Contains(searchLower)) ||
                                         (c.phone != null && c.phone.Contains(searchLower)) ||
                                         (c.email != null && c.email.ToLower().Contains(searchLower)) ||
                                         (c.User != null && c.User.username.ToLower().Contains(searchLower)));
            }

            ViewBag.SearchString = searchString;
            ViewBag.TotalCustomers = await query.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.PendingOrders = await _context.Orders.CountAsync(o => o.status == OrderStatus.Pending);

            var customers = await query.OrderByDescending(c => c.customerId).ToListAsync();
            return View(customers);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerOrders(int customerId)
        {
            var orders = await _context.Orders
                .Where(o => o.customerId == customerId)
                .OrderByDescending(o => o.orderDate)
                .Select(o => new
                {
                    o.orderId,
                    o.orderDate,
                    o.total,
                    o.receiverName,
                    o.receiverPhone,
                    o.receiverAddress,
                    status = o.status.ToString()
                })
                .ToListAsync();

            return Json(orders);
        }
    }
}
