using BTL_UML.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace BTL_UML.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Sinh viên")]
    public class ProgressUpdateController : Controller
    {
        private readonly QldaLvContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProgressUpdateController(QldaLvContext context, IWebHostEnvironment webHostEnvironment)
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
                .FirstOrDefault(d => d.MaSinhVien == sv.MaSinhVien && d.MaKi == currentMaKi);

            if (deTai == null)
            {
                ViewBag.ThongBao = "Bạn chưa có đề tài trong kỳ bảo vệ hiện tại.";
                return View();
            }

            if (deTai.TrangThaiDuyet != "Đã duyệt")
            {
                ViewBag.ThongBao = $"Đề tài của bạn {deTai.TrangThaiDuyet.ToLower()}, không thể nộp báo cáo tiến độ.";
                return View();
            }

            var danhSachBaoCao = _context.BaoCaoTienDos
                .Where(b => b.MaDeTai == deTai.MaDeTai)
                .OrderByDescending(b => b.NgayCapNhat)
                .ToList();

            ViewData["DeTai"] = deTai;
            ViewData["DanhSachBaoCao"] = danhSachBaoCao;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReport(string MaDeTai, string NoiDungBaoCao, IFormFile TepTinNop)
        {
            if (string.IsNullOrEmpty(MaDeTai)) return NotFound();

            var newBaoCao = new BaoCaoTienDo
            {
                MaBctd = GenerateMaBCTD(),
                MaDeTai = MaDeTai,
                NoiDungBaoCao = NoiDungBaoCao,
                NgayCapNhat = DateTime.Now
            };

            if (TepTinNop != null && TepTinNop.Length > 0)
            {
                var fileName = $"{newBaoCao.MaBctd}_{Path.GetFileName(TepTinNop.FileName)}";
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "reports");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await TepTinNop.CopyToAsync(fileStream);
                }

                newBaoCao.TepTinNop = $"/uploads/reports/{fileName}";
            }

            _context.BaoCaoTienDos.Add(newBaoCao);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Nộp báo cáo tiến độ thành công!";
            return RedirectToAction("Index");
        }

        private string GenerateMaBCTD()
        {
            var lastMa = _context.BaoCaoTienDos
                .OrderByDescending(b => b.MaBctd)
                .Select(b => b.MaBctd)
                .FirstOrDefault();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastMa) && lastMa.Length > 2)
            {
                var numberPart = lastMa.Substring(2);
                if (int.TryParse(numberPart, out int num))
                {
                    nextNumber = num + 1;
                }
            }

            return "BC" + nextNumber.ToString("D3");
        }
    }
}