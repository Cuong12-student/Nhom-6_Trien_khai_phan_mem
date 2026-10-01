using bhgbd.Data; // Bắt buộc có dòng này để nhận diện ApplicationDbContext
using bhgbd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace bhgbd.Pages.Auth
{
    public class LoginModel : PageModel
    {
        // 1. Khai báo biến _context
        private readonly ApplicationDbContext _context;

        // 2. Tiêm ApplicationDbContext vào Constructor
        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public LoginInput Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 3. Tìm User trong Database theo Username hoặc Email của Customer
            var user = _context.Users.FirstOrDefault(u =>
                (u.username == Input.UsernameOrEmail ||
                 _context.Customers.Any(c => c.userId == u.userId && c.email == Input.UsernameOrEmail))
                && u.password == Input.Password);

            if (user == null)
            {
                ErrorMessage = "Tài khoản hoặc mật khẩu không đúng.";
                return Page();
            }

            // 4. Lưu thông tin đăng nhập vào Session
            HttpContext.Session.SetInt32("userId", user.userId);
            HttpContext.Session.SetString("username", user.username);
            HttpContext.Session.SetString("role", user.role.ToString());

            // 5. Điều hướng theo phân quyền (Role)
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            switch (user.role)
            {
                case UserRole.Admin:
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                case UserRole.Staff:
                    return RedirectToAction("Index", "Home", new { area = "Staff" });
                case UserRole.Customer:
                default:
                    return RedirectToAction("Index", "Home", new { area = "Client" });
            }
        }

        public class LoginInput
        {
            [Required(ErrorMessage = "Vui lòng nhập username hoặc email")]
            [Display(Name = "Username hoặc Email")]
            public string UsernameOrEmail { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Duy trì đăng nhập")]
            public bool RememberMe { get; set; }
        }
    }
}