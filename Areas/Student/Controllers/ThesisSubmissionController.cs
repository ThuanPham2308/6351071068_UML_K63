using BTL_UML.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;

namespace BTL_UML.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Sinh viên")]
    public class ThesisSubmissionController : Controller
    {
        private readonly QldaLvContext _context;
        private readonly IWebHostEnvironment _env;

        public ThesisSubmissionController(QldaLvContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            ViewData["MaNguoiDung"] = maNguoiDung;
            ViewData["VaiTro"] = vaiTro;

            var user = _context.NguoiDungs.FirstOrDefault(u => u.MaNguoiDung == maNguoiDung);
            ViewData["TenNguoiDung"] = user?.HoTen;
            ViewData["Avatar"] = "~/shared/images/default-avatar.jpg";

            if (string.IsNullOrEmpty(maNguoiDung) || vaiTro != "Sinh viên")
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var sv = _context.SinhViens.FirstOrDefault(s => s.MaNguoiDung == maNguoiDung);
            if (sv == null)
            {
                ViewBag.ThongBao = "Không tìm thấy thông tin sinh viên.";
                return View();
            }

            var today = DateTime.Today;
            var currentKi = _context.KiBaoVeDoAns
                .Where(k => k.NgayBatDau <= today && today <= k.NgayKetThuc)
                .OrderByDescending(k => k.NgayBatDau)
                .FirstOrDefault();

            if (currentKi == null)
            {
                ViewBag.ThongBao = "Kỳ bảo vệ chưa mở.";
                ViewBag.TrangThai = "ChuaMoKi";
                return View();
            }

            var deTai = _context.DeTais.FirstOrDefault(d => d.MaSinhVien == sv.MaSinhVien && d.MaKi == currentKi.MaKi);
            if (deTai == null)
            {
                ViewBag.ThongBao = "Bạn chưa có đề tài đăng ký trong kỳ này.";
                ViewBag.TrangThai = "ChuaDangKyDeTai";
                return View();
            }

            if (deTai.TrangThaiDuyet != "Đã duyệt")
            {
                ViewBag.ThongBao = $"Đề tài của bạn hiện đang ở trạng thái: {deTai.TrangThaiDuyet}. Vui lòng chờ phê duyệt hoặc liên hệ giảng viên.";
                ViewBag.TrangThai = "DeTaiChuaDuyet";
                ViewData["DeTai"] = deTai;
                return View();
            }

            var luanVan = _context.LuanVans.FirstOrDefault(lv => lv.MaDeTai == deTai.MaDeTai);
            ViewData["DeTai"] = deTai;
            ViewData["LuanVan"] = luanVan;
            ViewBag.TrangThai = "DeTaiDaDuyet";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string MaDeTai, string MoTa, IFormFile TepTin)
        {
            if (string.IsNullOrEmpty(MaDeTai))
            {
                return NotFound("Không có mã đề tài.");
            }

            var deTai = await _context.DeTais.FirstOrDefaultAsync(d => d.MaDeTai == MaDeTai);
            if (deTai == null)
            {
                TempData["Error"] = "Không tìm thấy đề tài.";
                return RedirectToAction("Index");
            }

            if (deTai.TrangThaiDuyet != "Đã duyệt")
            {
                TempData["Error"] = "Đề tài chưa được duyệt, không thể nộp luận văn.";
                return RedirectToAction("Index");
            }

            var existing = await _context.LuanVans.FirstOrDefaultAsync(lv => lv.MaDeTai == MaDeTai);
            var maLuanVan = existing?.MaLuanVan ?? GenerateMaLuanVan();

            var fileUrl = existing?.TepTinNop;
            if (TepTin != null && TepTin.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "theses");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{maLuanVan}_{Path.GetFileName(TepTin.FileName)}";
                var path = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await TepTin.CopyToAsync(stream);
                }

                fileUrl = $"/uploads/theses/{fileName}";
            }
            else if (fileUrl == null)
            {
                TempData["Error"] = "Vui lòng đính kèm tệp luận văn.";
                return RedirectToAction("Index");
            }

            if (existing != null)
            {
                if (existing.TrangThaiDuyet == "Đã duyệt")
                {
                    TempData["Error"] = "Luận văn đã được duyệt, không thể chỉnh sửa.";
                    return RedirectToAction("Index");
                }

                existing.MoTa = MoTa;
                existing.TepTinNop = fileUrl;
                existing.NgayNop = DateTime.Now;
                _context.Update(existing);
            }
            else
            {
                var newLuanVan = new LuanVan
                {
                    MaLuanVan = maLuanVan,
                    MaDeTai = MaDeTai,
                    MoTa = MoTa,
                    TepTinNop = fileUrl,
                    NgayNop = DateTime.Now,
                    TrangThaiDuyet = "Chờ duyệt",
                    XacNhanBaoVe = "Chờ duyệt",
                    TrangThai = "Đang bảo vệ"
                };
                _context.Add(newLuanVan);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Nộp luận văn thành công!";
            return RedirectToAction("Index");
        }

        private string GenerateMaLuanVan()
        {
            var last = _context.LuanVans
                .OrderByDescending(lv => lv.MaLuanVan)
                .Select(lv => lv.MaLuanVan)
                .FirstOrDefault();

            int next = 1;
            if (!string.IsNullOrEmpty(last) && last.StartsWith("LV"))
            {
                int.TryParse(last.Substring(2), out next);
                next++;
            }

            return "LV" + next.ToString("D3");
        }
    }
}