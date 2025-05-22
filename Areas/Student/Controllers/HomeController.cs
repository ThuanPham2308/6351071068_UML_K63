using BTL_UML.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace BTL_UML.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Sinh viên")]
    public class HomeController : Controller
    {
        private readonly QldaLvContext _context;

        public HomeController(QldaLvContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            ViewData["MaNguoiDung"] = maNguoiDung;
            ViewData["VaiTro"] = vaiTro;

            var user = _context.NguoiDungs.FirstOrDefault(u => u.MaNguoiDung == maNguoiDung);
            ViewData["TenNguoiDung"] = user?.HoTen;

            //ViewData["Avatar"] = user?.AnhDaiDien ?? "~/shared/images/default-avatar.jpg";
            ViewData["Avatar"] = "~/shared/images/default-avatar.jpg";
            return View();
        }
    }
}
