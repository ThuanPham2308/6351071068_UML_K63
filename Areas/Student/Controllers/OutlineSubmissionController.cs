using BTL_UML.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BTL_UML.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Sinh viên")]
    public class OutlineSubmissionController : Controller
    {
        private readonly QldaLvContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public OutlineSubmissionController(QldaLvContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            if (string.IsNullOrEmpty(maNguoiDung) || vaiTro != "Sinh viên")
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            ViewData["MaNguoiDung"] = maNguoiDung;
            ViewData["VaiTro"] = vaiTro;

            var user = _context.NguoiDungs.FirstOrDefault(u => u.MaNguoiDung == maNguoiDung);
            ViewData["TenNguoiDung"] = user?.HoTen;
            ViewData["Avatar"] = "~/shared/images/default-avatar.jpg";

            var currentDateTime = DateTime.Today;

            var currentKi = _context.KiBaoVeDoAns
                .Where(k => k.NgayBatDau.Date <= currentDateTime && currentDateTime <= k.NgayKetThuc.Date)
                .OrderByDescending(k => k.NgayBatDau)
                .FirstOrDefault();

            if (currentKi == null)
            {
                ViewBag.ThongBao = "Kỳ bảo vệ chưa mở.";
                return View();
            }

            string currentMaKi = currentKi.MaKi;
            var sv = _context.SinhViens.FirstOrDefault(s => s.MaNguoiDung == maNguoiDung);

            if (sv == null)
            {
                ViewBag.ThongBao = "Không tìm thấy thông tin sinh viên.";
                return View();
            }

            var deTai = _context.DeTais
                .Include(d => d.DeCuongKeHoaches)
                .FirstOrDefault(d => d.MaSinhVien == sv.MaSinhVien && d.MaKi == currentMaKi);

            if (deTai == null)
            {
                ViewBag.ThongBao = "Bạn chưa có đề tài trong kỳ bảo vệ hiện tại.";
                return View();
            }

            if (deTai.TrangThaiDuyet != "Đã duyệt")
            {
                ViewBag.ThongBao = $"Đề tài của bạn {deTai.TrangThaiDuyet.ToLower()}, không thể nộp đề cương.";
                return View();
            }

            ViewData["DeTai"] = deTai;
            var deCuongMoiNhat = deTai.DeCuongKeHoaches.OrderByDescending(d => d.NgayNop).FirstOrDefault();
            ViewData["DeCuong"] = deCuongMoiNhat;

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitOutline(string MaDeTai, IFormFile NoiDungDeCuongFile, IFormFile KeHoachThucHienFile, string GhiChu)
        {
            if (string.IsNullOrEmpty(MaDeTai))
            {
                return NotFound();
            }

            var newDeCuong = new DeCuongKeHoach
            {
                MaDeCuong = GenerateMaDCKH(),
                MaDeTai = MaDeTai,
                NgayNop = DateTime.Now,
                GhiChu = GhiChu
            };

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "outlines");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            if (NoiDungDeCuongFile != null && NoiDungDeCuongFile.Length > 0)
            {
                var fileNameDeCuong = $"{newDeCuong.MaDeCuong}_NoiDung_{Path.GetFileName(NoiDungDeCuongFile.FileName)}";
                var filePathDeCuong = Path.Combine(uploadsFolder, fileNameDeCuong);

                using (var fileStream = new FileStream(filePathDeCuong, FileMode.Create))
                {
                    await NoiDungDeCuongFile.CopyToAsync(fileStream);
                }

                newDeCuong.NoiDungDeCuong = $"/uploads/outlines/{fileNameDeCuong}";
            }
            else
            {
                ModelState.AddModelError("NoiDungDeCuongFile", "Vui lòng chọn file nội dung đề cương.");
                return View("Index"); 
            }

            if (KeHoachThucHienFile != null && KeHoachThucHienFile.Length > 0)
            {
                var fileNameKeHoach = $"{newDeCuong.MaDeCuong}_KeHoach_{Path.GetFileName(KeHoachThucHienFile.FileName)}";
                var filePathKeHoach = Path.Combine(uploadsFolder, fileNameKeHoach);

                using (var fileStream = new FileStream(filePathKeHoach, FileMode.Create))
                {
                    await KeHoachThucHienFile.CopyToAsync(fileStream);
                }

                newDeCuong.KeHoachThucHien = $"/uploads/outlines/{fileNameKeHoach}"; 
            }
            else
            {
                ModelState.AddModelError("KeHoachThucHienFile", "Vui lòng chọn file kế hoạch thực hiện.");
                return View("Index"); 
            }

            _context.DeCuongKeHoaches.Add(newDeCuong);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Nộp đề cương và kế hoạch thành công!";
            return RedirectToAction("Index");
        }

        private string GenerateMaDCKH()
        {
            var lastMa = _context.DeCuongKeHoaches
                .OrderByDescending(d => d.MaDeCuong)
                .Select(d => d.MaDeCuong)
                .FirstOrDefault();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastMa) && lastMa.Length > 3)
            {
                var numberPart = lastMa.Substring(3);
                if (int.TryParse(numberPart, out int num))
                {
                    nextNumber = num + 1;
                }
            }

            return "DCK" + nextNumber.ToString("D3");
        }
    }
}
