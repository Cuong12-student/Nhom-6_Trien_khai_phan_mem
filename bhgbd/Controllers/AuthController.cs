using bhgbd.Data;
using bhgbd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace bhgbd.Controllers
{
    public class AuthController : Controller
    {
        // Xử lý Đăng xuất
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa sạch session
            return RedirectToPage("/Auth/Login"); // Chuyển về trang đăng nhập
        }
    }
}