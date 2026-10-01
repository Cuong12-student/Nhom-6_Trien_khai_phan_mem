using bhgbd.Data;
using bhgbd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bhgbd.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var totalRevenue = await _context.Orders
                .Where(o => o.status == OrderStatus.Confirmed)
                .SumAsync(o => (decimal?)o.total) ?? 0;

            var totalOrders = await _context.Orders.CountAsync();
            var ordersToday = await _context.Orders.Where(o => o.orderDate >= today).CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalCustomers = await _context.Customers.CountAsync();
            var totalStaff = await _context.Staffs.CountAsync();

            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.orderDate)
                .Take(10)
                .ToListAsync();

            var statusCounts = await _context.Orders
                .GroupBy(o => o.status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.OrdersToday = ordersToday;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.TotalStaff = totalStaff;
            ViewBag.RecentOrders = recentOrders;
            ViewBag.StatusCounts = statusCounts;

            return View();
        }
    }
}
