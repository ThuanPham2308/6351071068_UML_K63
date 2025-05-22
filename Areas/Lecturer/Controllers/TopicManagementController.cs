using BTL_UML.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using PagedList;
using BTL_UML.ViewModels;
using BTL_UML.Models;

namespace BTL_UML.Areas.Lecturer.Controllers
{
    [Area("Lecturer")]
    [Authorize(Roles = "Giảng viên")]
    public class TopicManagementController : Controller
    {
        private readonly QldaLvContext _context;

        public TopicManagementController(QldaLvContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search, string roleFilter, string statusFilter, int? page)
        {
            var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");

            ViewData["MaNguoiDung"] = maNguoiDung;
            ViewData["VaiTro"] = HttpContext.Session.GetString("VaiTro");

            var user = _context.NguoiDungs.FirstOrDefault(u => u.MaNguoiDung == maNguoiDung);
            ViewData["TenNguoiDung"] = user?.HoTen;
            ViewData["Avatar"] = "~/shared/images/default-avatar.jpg";

            var currentKi = _context.KiBaoVeDoAns
                            .OrderByDescending(k => k.NgayBatDau)
                            .FirstOrDefault();

            if (currentKi == null)
            {
                ViewBag.Message = "Chưa có kỳ bảo vệ nào.";
                return Redirect("/Home/Error"); 
            }

            ViewBag.CurrentKi = currentKi.TenKi;

            var giangVien = _context.GiangViens.FirstOrDefault(gv => gv.MaNguoiDung == maNguoiDung);
            if (giangVien == null) return Redirect("/Home/Error");

            var deTaisQuery = _context.PhanCongGiangViens
                .Where(pc => pc.MaGiangVien == giangVien.MaGiangVien)
                .Include(pc => pc.MaDeTaiNavigation)
                    .ThenInclude(dt => dt.MaSinhVienNavigation)
                        .ThenInclude(sv => sv.MaNguoiDungNavigation)
                .Include(pc => pc.MaDeTaiNavigation)
                    .ThenInclude(dt => dt.MaKiNavigation)
                .Include(pc => pc.MaDeTaiNavigation)
                    .ThenInclude(dt => dt.KetQuas)
                .Include(pc => pc.MaDeTaiNavigation)
                    .ThenInclude(dt => dt.PhanCongGiangViens)
                        .ThenInclude(pc => pc.MaGiangVienNavigation)
                            .ThenInclude(gv => gv.MaNguoiDungNavigation)
                .AsQueryable();

            deTaisQuery = deTaisQuery.Where(pc => pc.MaDeTaiNavigation.MaKi == currentKi.MaKi);

            if (!string.IsNullOrEmpty(roleFilter) && roleFilter != "all")
            {
                deTaisQuery = deTaisQuery.Where(pc => pc.VaiTro == roleFilter);
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                deTaisQuery = deTaisQuery.Where(pc => pc.MaDeTaiNavigation.TrangThaiDuyet == statusFilter);
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                deTaisQuery = deTaisQuery.Where(pc =>
                    pc.MaDeTaiNavigation.TenDeTai.ToLower().Contains(search) ||
                    pc.MaDeTaiNavigation.MaSinhVienNavigation.MaNguoiDungNavigation.HoTen.ToLower().Contains(search));
            }

            var filteredList = deTaisQuery
                .Select(pc => pc.MaDeTaiNavigation)
                .Distinct()
                .OrderByDescending(dt => dt.NgayDangKy)
                .ToList();

            int pageSize = 10;
            int pageNumber = page ?? 1;
            var pagedList = filteredList.ToPagedList(pageNumber, pageSize);

            ViewBag.PagesToShow = Enumerable.Range(
                Math.Max(1, pageNumber - 1),
                Math.Min(3, pagedList.PageCount)
            ).ToList();

            return View(pagedList);
        }

        public async Task<IActionResult> DetailTopic(string id)
        {
            if (string.IsNullOrEmpty(id))
                return Redirect("/Home/Error");

            var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaNguoiDung == maNguoiDung);

            if (giangVien == null)
                return Redirect("/Home/Error");

            var dt = await _context.DeTais
                .Include(d => d.MaSinhVienNavigation)
                    .ThenInclude(sv => sv.MaNguoiDungNavigation)
                .Include(d => d.DeCuongKeHoaches)
                .Include(d => d.BaoCaoTienDos)
                .Include(d => d.LuanVans)
                .Include(d => d.KetQuas)
                .FirstOrDefaultAsync(d => d.MaDeTai == id);

            if (dt == null)
                return Redirect("/Home/Error");

            var danhSachGiangVien = await _context.GiangViens
                .Include(gv => gv.MaNguoiDungNavigation)
                .ToListAsync();

            ViewBag.DanhSachGiangVien = danhSachGiangVien;
            var gvPhanCong = await _context.PhanCongGiangViens
                .Where(pc => pc.MaDeTai == id)
                .Include(pc => pc.MaGiangVienNavigation)
                    .ThenInclude(gv => gv.MaNguoiDungNavigation)
                .ToListAsync();

            var giangVienPhanCong = gvPhanCong.FirstOrDefault(pc => pc.MaGiangVien == giangVien.MaGiangVien);
            ViewBag.VaiTroGiangVien = giangVienPhanCong?.VaiTro ?? "";
            ViewBag.IsHuongDan = giangVienPhanCong?.VaiTro == "Hướng dẫn";
            ViewBag.IsBienLuan = giangVienPhanCong?.VaiTro == "Biện luận";
            ViewBag.MaGiangVien = giangVien.MaGiangVien;

            var vm = new ChiTietDeTaiViewModel
            {
                MaDeTai = dt.MaDeTai,
                TenDeTai = dt.TenDeTai,
                MoTa = dt.MoTa,
                TrangThaiDuyet = dt.TrangThaiDuyet,

                MaSinhVien = dt.MaSinhVien,
                HoTen = dt.MaSinhVienNavigation?.MaNguoiDungNavigation?.HoTen ?? "",
                NgaySinh = dt.MaSinhVienNavigation?.MaNguoiDungNavigation?.NgaySinh ?? DateTime.MinValue,
                GioiTinh = dt.MaSinhVienNavigation?.MaNguoiDungNavigation?.GioiTinh ?? "",
                DiaChi = dt.MaSinhVienNavigation?.MaNguoiDungNavigation?.DiaChi ?? "",
                Email = dt.MaSinhVienNavigation?.MaNguoiDungNavigation?.Email ?? "",

                DeCuongKeHoaches = dt.DeCuongKeHoaches.ToList(),
                BaoCaoTienDos = dt.BaoCaoTienDos.ToList(),
                LuanVans = dt.LuanVans.ToList(),
                KetQuas = dt.KetQuas.ToList(),

                PhanCongGiangViens = gvPhanCong
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> DuyetDeTai(string maDeTai, string trangThai)
        {
            if (string.IsNullOrEmpty(maDeTai))
                return Json(new { success = false, message = "Mã đề tài không hợp lệ" });

            var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaNguoiDung == maNguoiDung);

            if (giangVien == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin giảng viên" });

            var check = await _context.PhanCongGiangViens
                .AnyAsync(pc => pc.MaDeTai == maDeTai && pc.MaGiangVien == giangVien.MaGiangVien && pc.VaiTro == "Hướng dẫn");

            if (!check)
                return Json(new { success = false, message = "Bạn không có quyền duyệt đề tài này" });

            var lv = await _context.LuanVans
                .FirstOrDefaultAsync(l => l.MaDeTai == maDeTai);

            if (lv == null)
                return Json(new { success = false, message = "Không tìm thấy đề tài" });

            lv.TrangThaiDuyet = trangThai;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
        }

        [HttpPost]
        public async Task<IActionResult> XacNhanBaoVe(string maLuanVan, string trangThai)
        {
            if (string.IsNullOrEmpty(maLuanVan))
                return Json(new { success = false, message = "Mã luận văn không hợp lệ" });

            var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaNguoiDung == maNguoiDung);

            if (giangVien == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin giảng viên" });

            var luanVan = await _context.LuanVans.FindAsync(maLuanVan);
            if (luanVan == null)
                return Json(new { success = false, message = "Không tìm thấy luận văn" });

            var check = await _context.PhanCongGiangViens
                .AnyAsync(pc => pc.MaDeTai == luanVan.MaDeTai && pc.MaGiangVien == giangVien.MaGiangVien && pc.VaiTro == "Biện luận");

            if (!check)
                return Json(new { success = false, message = "Bạn không có quyền xác nhận bảo vệ luận văn này" });

            luanVan.XacNhanBaoVe = trangThai;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
        }

        [HttpPost]
        public async Task<IActionResult> LuuNhanXet(string maBCTD, string nhanXet)
        {
            if (string.IsNullOrEmpty(maBCTD))
                return Json(new { success = false, message = "Mã báo cáo không hợp lệ" });

            var baoCao = await _context.BaoCaoTienDos.FindAsync(maBCTD);
            if (baoCao == null)
                return Json(new { success = false, message = "Không tìm thấy báo cáo" });

            baoCao.NhanXet = nhanXet;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Lưu nhận xét thành công" });
        }

        [HttpPost]
        public async Task<IActionResult> LuuKetQua(KetQua ketQua)
        {
            if (ketQua == null || string.IsNullOrEmpty(ketQua.MaDeTai))
                return Json(new { success = false, message = "Thông tin kết quả không hợp lệ" });

            var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaNguoiDung == maNguoiDung);

            if (giangVien == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin giảng viên" });

            var deTai = await _context.DeTais.FindAsync(ketQua.MaDeTai);
            if (deTai == null)
                return Json(new { success = false, message = "Không tìm thấy đề tài" });

            var luanVan = await _context.LuanVans
                .FirstOrDefaultAsync(lv => lv.MaDeTai == ketQua.MaDeTai);

            if (luanVan == null || luanVan.XacNhanBaoVe != "Được bảo vệ" || luanVan.TrangThaiDuyet != "Đã duyệt")
                return Json(new { success = false, message = "Luận văn chưa được duyệt hoặc chưa được xác nhận bảo vệ" });

            var existingKetQua = await _context.KetQuas
                .FirstOrDefaultAsync(kq => kq.MaDeTai == ketQua.MaDeTai);

            if (existingKetQua != null)
            {
                existingKetQua.DiemQuaTrinh = ketQua.DiemQuaTrinh;
                existingKetQua.DiemBaoVe = ketQua.DiemBaoVe;
                existingKetQua.NhanXet = ketQua.NhanXet;
                existingKetQua.NgayDanhGia = DateTime.Now;
            }
            else
            {
                var existingMaKetQuas = await _context.KetQuas
                    .Where(kq => kq.MaKetQua.StartsWith("KQ"))
                    .Select(kq => kq.MaKetQua)
                    .ToListAsync();

                int maxIndex = existingMaKetQuas
                    .Select(id =>
                    {
                        var numericPart = id.Substring(2); 
                        return int.TryParse(numericPart, out int number) ? number : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                ketQua.MaKetQua = "KQ" + (maxIndex + 1).ToString("D3");
                ketQua.NgayDanhGia = DateTime.Now;

                _context.KetQuas.Add(ketQua);
                await _context.SaveChangesAsync();

            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Lưu kết quả thành công" });
        }
    }
}