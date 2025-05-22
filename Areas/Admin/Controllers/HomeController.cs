using BTL_UML.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BTL_UML.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị")]
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
            //ViewData["Avatar"] = user?.AnhDaiDien ?? "~/shared/images/default-avatar.png";
            ViewData["Avatar"] = "~/shared/images/default-avatar.jpg";

            var currentDateTime = DateTime.Today;

            var currentKi = _context.KiBaoVeDoAns
                .Where(k => k.NgayBatDau.Date <= currentDateTime && currentDateTime <= k.NgayKetThuc.Date)
                .OrderByDescending(k => k.NgayBatDau)
                .FirstOrDefault();

            string currentMaKi = currentKi?.MaKi ?? null;

            ViewBag.TotalArchived = _context.LuanVans.Count(lv => lv.TrangThai == "Bảo vệ thành công");
            ViewBag.TotalLecturers = _context.GiangViens.Count();

            if (currentMaKi == null)
            {
                ViewBag.StudentsInCurrentDefense = 0;
                ViewBag.TotalStudents = 0;
                ViewBag.OngoingTopics = 0;
                ViewBag.CompletedTopics = 0;
                ViewBag.PendingTopics = 0;
                return View();
            }

            var studentsInCurrentDefense = _context.DeTais
                .Where(dt => dt.MaKi == currentMaKi && dt.TrangThaiDuyet == "Đã duyệt")
                .Select(dt => dt.MaSinhVien)
                .Distinct()
                .Count();

            var ongoingTopics = _context.DeTais
                .Count(dt => dt.MaKi == currentMaKi && dt.TrangThaiDuyet == "Chờ duyệt");

            var completedTopics = _context.DeTais
                .Count(dt => dt.MaKi == currentMaKi && dt.TrangThaiDuyet == "Đã duyệt");

            var pendingTopics = _context.DeTais
                .Count(dt => dt.MaKi == currentMaKi && dt.TrangThaiDuyet == "Từ chối");

            ViewBag.StudentsInCurrentDefense = studentsInCurrentDefense;
            ViewBag.OngoingTopics = ongoingTopics;
            ViewBag.CompletedTopics = completedTopics;
            ViewBag.PendingTopics = pendingTopics;

            var lecturerTopicData = _context.PhanCongGiangViens
                .Where(pc => (pc.VaiTro == "Hướng dẫn" || pc.VaiTro == "Biện luận") &&
                             _context.DeTais.Any(dt => dt.MaDeTai == pc.MaDeTai && dt.MaKi == currentMaKi))
                .GroupBy(pc => pc.MaGiangVien)
                .Select(g => new
                {
                    LecturerName = _context.GiangViens
                        .Where(gv => gv.MaGiangVien == g.Key)
                        .Select(gv => gv.MaNguoiDungNavigation.HoTen)
                        .FirstOrDefault(),
                    TopicCount = g.Select(pc => pc.MaDeTai).Distinct().Count()
                })
                .OrderByDescending(x => x.TopicCount)
                .ToList();

            ViewBag.LecturerNamesJson = JsonConvert.SerializeObject(lecturerTopicData.Select(x => x.LecturerName));
            ViewBag.TopicCountsJson = JsonConvert.SerializeObject(lecturerTopicData.Select(x => x.TopicCount));

            return View();
        }

    }
}
