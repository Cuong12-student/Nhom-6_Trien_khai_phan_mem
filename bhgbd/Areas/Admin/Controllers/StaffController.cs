using bhgbd.Data;
using bhgbd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bhgbd.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var staffList = await _context.Staffs
                .Include(s => s.User)
                .ToListAsync();
            return View(staffList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string username, string password, string staffName, string email, string phone, string address, int age, Gender gender)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(staffName))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ Tên đăng nhập, Mật khẩu và Họ tên nhân viên!";
                return RedirectToAction(nameof(Index));
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.username == username);
            if (existingUser != null)
            {
                TempData["Error"] = "Tên đăng nhập đã tồn tại trong hệ thống!";
                return RedirectToAction(nameof(Index));
            }
            int maxUserId = await _context.Users.MaxAsync(u => (int?)u.userId) ?? 0;
            int newUserId = maxUserId + 1;

            var newUser = new User
            {
                username = username,
                password = password,
                role = UserRole.Staff
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            int maxStaffId = await _context.Staffs.MaxAsync(s => (int?)s.staffId) ?? 0;
            int newStaffId = maxStaffId + 1;

            var newStaff = new bhgbd.Models.Staff
            {
                staffId = newUser.userId,
                userId = newUser.userId,
                staffName = staffName,
                email = email ?? "",
                phone = phone ?? "",
                address = address ?? "Chưa cập nhật",
                age = age > 0 ? age : 25,
                gender = gender
            };
            _context.Staffs.Add(newStaff);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cấp tài khoản Nhân viên thành công cho '{staffName}'!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int staffId, string staffName, string email, string phone, string address, int age)
        {
            var staff = await _context.Staffs.FindAsync(staffId);
            if (staff != null)
            {
                staff.staffName = staffName;
                staff.email = email ?? "";
                staff.phone = phone ?? "";
                staff.address = address ?? "";
                staff.age = age;
                _context.Staffs.Update(staff);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thông tin nhân viên thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null && !string.IsNullOrWhiteSpace(newPassword))
            {
                user.password = newPassword;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã đặt lại mật khẩu thành công cho tài khoản '{user.username}'!";
            }
            else
            {
                TempData["Error"] = "Mật khẩu mới không được để trống!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
