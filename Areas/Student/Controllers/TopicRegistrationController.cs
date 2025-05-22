using BTL_UML.Data;
using BTL_UML.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_UML.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Sinh viên")]
    public class TopicRegistrationController : Controller
    {
        private readonly QldaLvContext _context;

        public TopicRegistrationController(QldaLvContext context)
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
            ViewData["Avatar"] = "~/shared/images/default-avatar.jpg";

            var currentDate = DateTime.Today;
            var currentKi = _context.KiBaoVeDoAns
                .Where(k => k.NgayBatDau <= currentDate && currentDate <= k.NgayKetThuc)
                .OrderByDescending(k => k.NgayBatDau)
                .FirstOrDefault();

            ViewBag.CurrentKi = currentKi;

            var sinhVien = _context.SinhViens.FirstOrDefault(sv => sv.MaNguoiDung == maNguoiDung);
            var maSinhVien = sinhVien?.MaSinhVien ?? "";
            ViewData["MaSinhVien"] = maSinhVien;

            if (currentKi != null)
            {
                var deTai = _context.DeTais
                    .FirstOrDefault(d => d.MaSinhVien == maSinhVien && d.MaKi == currentKi.MaKi);

                ViewBag.DeTaiDaDangKy = deTai;

                if (deTai != null)
                {
                    var maDeTai = deTai.MaDeTai;  

                    var giangVienHuongDan = _context.PhanCongGiangViens
                        .Include(pc => pc.MaGiangVienNavigation)
                        .ThenInclude(gv => gv.MaNguoiDungNavigation)
                        .FirstOrDefault(pc => pc.MaDeTai == maDeTai && pc.VaiTro == "Hướng dẫn");

                    ViewBag.GiangVienHuongDan = giangVienHuongDan;
                }
                else
                {
                    ViewBag.GiangVienHuongDan = null;
                }
            }
            ViewBag.DanhSachGiangVien = _context.GiangViens
                .Include(gv => gv.MaNguoiDungNavigation)
                .ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeTaiViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var existingMaDeTais = await _context.DeTais
                    .Where(dt => dt.MaDeTai.StartsWith("DT"))
                    .Select(dt => dt.MaDeTai)
                    .ToListAsync();

                int maxIndex = existingMaDeTais
                    .Select(id =>
                    {
                        var numericPart = id.Substring(2);
                        return int.TryParse(numericPart, out int number) ? number : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                string newMaDeTai = "DT" + (maxIndex + 1).ToString("D3");

                var currentDate = DateTime.Today;
                var currentKi = await _context.KiBaoVeDoAns
                    .Where(k => k.NgayBatDau <= currentDate && currentDate <= k.NgayKetThuc)
                    .OrderByDescending(k => k.NgayBatDau)
                    .FirstOrDefaultAsync();

                string? currentMaKi = currentKi?.MaKi;

                var maSinhVien = viewModel.MaSinhVien;
                if (string.IsNullOrEmpty(maSinhVien))
                {
                    ModelState.AddModelError("", "Không tìm thấy thông tin sinh viên trong phiên đăng nhập.");
                    return View("Index", viewModel);
                }

                var deTai = new DeTai
                {
                    MaDeTai = newMaDeTai,
                    TenDeTai = viewModel.TenDeTai,
                    MoTa = viewModel.MoTa,
                    MaSinhVien = maSinhVien,
                    MaKi = currentMaKi,
                    NgayDangKy = DateTime.Now,
                    TrangThaiDuyet = "Chờ duyệt"
                };

                try
                {
                    _context.Add(deTai);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đăng ký đề tài thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
                }
            }

            var currentKiAgain = await _context.KiBaoVeDoAns
                .Where(k => k.NgayBatDau <= DateTime.Today && DateTime.Today <= k.NgayKetThuc)
                .OrderByDescending(k => k.NgayBatDau)
                .FirstOrDefaultAsync();
            ViewBag.CurrentKi = currentKiAgain;

            return View("Index", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAdvisor(string maDeTai, string maGiangVien)
        {
            if (string.IsNullOrEmpty(maDeTai) || string.IsNullOrEmpty(maGiangVien))
            {
                TempData["ErrorMessage"] = "Thông tin không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var deTai = await _context.DeTais.FindAsync(maDeTai);
            if (deTai == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đề tài.";
                return RedirectToAction(nameof(Index));
            }

            if (deTai.TrangThaiDuyet != "Chờ duyệt")
            {
                TempData["ErrorMessage"] = "Chỉ có thể đăng ký giảng viên khi đề tài đang ở trạng thái chờ duyệt.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var existingAdvisor = await _context.PhanCongGiangViens
                    .FirstOrDefaultAsync(pc => pc.MaDeTai == maDeTai && pc.VaiTro == "Hướng dẫn");

                if (existingAdvisor != null)
                {
                    _context.PhanCongGiangViens.Remove(existingAdvisor);
                    await _context.SaveChangesAsync();
                }

                var phanCong = new PhanCongGiangVien
                {
                    MaDeTai = maDeTai,
                    MaGiangVien = maGiangVien,
                    VaiTro = "Hướng dẫn"
                };
                _context.PhanCongGiangViens.Add(phanCong);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký giảng viên hướng dẫn thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật giảng viên hướng dẫn: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}