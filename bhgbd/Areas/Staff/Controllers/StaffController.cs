using Microsoft.AspNetCore.Mvc;

namespace bhgbd.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class StaffController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
