using bhgbd.Data;
using bhgbd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace bhgbd.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegisterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public RegisterInput Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!Input.AgreeTerms)
            {
                ModelState.AddModelError("Input.AgreeTerms", "Bạn cần đồng ý với điều khoản sử dụng.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            string cleanUsername = Input.Username.Trim();
            string cleanEmail = Input.Email?.Trim() ?? "";

            if (_context.Users.Any(u => u.username == cleanUsername))
            {
                ErrorMessage = "Tên đăng nhập đã được sử dụng.";
                return Page();
            }

            if (!string.IsNullOrEmpty(cleanEmail) && _context.Customers.Any(c => c.email == cleanEmail))
            {
                ErrorMessage = "Email đã được đăng ký.";
                return Page();
            }

            try
            {
                // 1. Khởi tạo User
                var newUser = new User
                {
                    username = cleanUsername,
                    password = Input.Password.Trim(),
                    role = UserRole.Customer
                };

                _context.Users.Add(newUser);
                _context.SaveChanges(); // Lưu User để lấy userId tự tăng

                Enum.TryParse<Gender>(Input.Gender, out var genderEnum);

                // 2. Khởi tạo Customer (Không gán customerId)
                var newCustomer = new Customer
                {
                    customerName = string.IsNullOrWhiteSpace(Input.CustomerName) ? cleanUsername : Input.CustomerName.Trim(),
                    age = Input.Age ?? 20,
                    gender = genderEnum,
                    email = cleanEmail,
                    address = string.IsNullOrWhiteSpace(Input.Address) ? "Chưa cập nhật" : Input.Address.Trim(),
                    phone = string.IsNullOrWhiteSpace(Input.PhoneNumber) ? "Chưa cập nhật" : Input.PhoneNumber.Trim(),
                    userId = newUser.userId
                };

                _context.Customers.Add(newCustomer);
                _context.SaveChanges(); // Lưu Customer

                return RedirectToPage("/Auth/Login");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi đăng ký: " + ex.Message;
                return Page();
            }
        }

        public class RegisterInput
        {
            // ===== Thông tin khách hàng =====

            [Required(ErrorMessage = "Vui lòng nhập họ tên")]
            [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
            [Display(Name = "Customer Name")]
            public string CustomerName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập tuổi")]
            [Range(10, 100, ErrorMessage = "Tuổi phải từ {1} đến {2}")]
            [Display(Name = "Age")]
            public int? Age { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn giới tính")]
            [Display(Name = "Gender")]
            public string Gender { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
            [Display(Name = "Address")]
            public string Address { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; } = string.Empty;

            // ===== Thông tin tài khoản =====

            [Required(ErrorMessage = "Vui lòng nhập username")]
            [StringLength(30, MinimumLength = 4, ErrorMessage = "Username phải từ {2} đến {1} ký tự")]
            [Display(Name = "Username")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải tối thiểu {2} ký tự")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [AllowedValues(true, ErrorMessage = "Bạn cần đồng ý với điều khoản sử dụng")]
            [Display(Name = "Tôi đồng ý với điều khoản sử dụng")]
            public bool AgreeTerms { get; set; }
        }
    }
}