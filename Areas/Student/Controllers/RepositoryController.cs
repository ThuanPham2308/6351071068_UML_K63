using BTL_UML.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using PagedList;
using BTL_UML.ViewModels;

namespace BTL_UML.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Sinh viên")]
    public class RepositoryController : Controller
    {
        private readonly QldaLvContext _context;

        public RepositoryController(QldaLvContext context)
        {
            _context = context;
        }
        public IActionResult Index(string search, string maKiChon, int? page)
        {
            var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            ViewData["MaNguoiDung"] = maNguoiDung;
            ViewData["VaiTro"] = vaiTro;

            var user = _context.NguoiDungs.FirstOrDefault(u => u.MaNguoiDung == maNguoiDung);
            ViewData["TenNguoiDung"] = user?.HoTen;
            ViewData["Avatar"] = "~/shared/images/default-avatar.jpg";

            var danhSachKi = _context.KiBaoVeDoAns.OrderByDescending(k => k.NgayBatDau).ToList();
            var defaultKi = danhSachKi.FirstOrDefault();

            ViewBag.DanhSachKi = new SelectList(danhSachKi, "MaKi", "TenKi", maKiChon ?? "all");
            ViewBag.MaKiChon = maKiChon ?? "all";
            ViewBag.CurrentSearch = search;

            var deTaisQuery = _context.DeTais
                .Include(dt => dt.MaSinhVienNavigation).ThenInclude(sv => sv.MaNguoiDungNavigation)
                .Include(dt => dt.PhanCongGiangViens).ThenInclude(p => p.MaGiangVienNavigation).ThenInclude(gv => gv.MaNguoiDungNavigation)
                .Include(dt => dt.LuanVans)
                .Include(dt => dt.MaKiNavigation)
                .Include(dt => dt.KetQuas)
                .AsQueryable();

            if (maKiChon != "all" && !string.IsNullOrEmpty(maKiChon))
            {
                deTaisQuery = deTaisQuery.Where(dt => dt.MaKi == maKiChon);
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                deTaisQuery = deTaisQuery.Where(dt =>
                    dt.TenDeTai.ToLower().Contains(search) ||
                    dt.MaSinhVienNavigation.MaNguoiDungNavigation.HoTen.ToLower().Contains(search) ||
                    dt.PhanCongGiangViens.Any(p => p.MaGiangVienNavigation.MaNguoiDungNavigation.HoTen.ToLower().Contains(search)) ||
                    dt.MaKiNavigation.TenKi.ToLower().Contains(search)
                );
            }

            var filteredList = deTaisQuery
                .Where(dt => dt.LuanVans.Any(lv => lv.TrangThai == "Bảo vệ thành công"))
                .Select(dt => new DeTaiViewModel
                {
                    TenDeTai = dt.TenDeTai,
                    MoTa = dt.MoTa,
                    MaSinhVien = dt.MaSinhVien,
                    MaKi = dt.MaKi,
                    NgayDangKy = dt.NgayDangKy,
                    TrangThaiDuyet = dt.TrangThaiDuyet,
                    PhanCongGiangViens = dt.PhanCongGiangViens.ToList(),
                    KetQuas = dt.KetQuas.ToList()
                })
                .OrderByDescending(dt => dt.NgayDangKy)
                .ToList();

            int pageSize = 10;
            int pageNumber = page ?? 1;
            var pagedList = filteredList.ToPagedList(pageNumber, pageSize);

            int totalPages = pagedList.PageCount;
            int startPage = Math.Max(1, pageNumber - 1);
            int endPage = Math.Min(totalPages, pageNumber + 1);
            ViewBag.PagesToShow = Enumerable.Range(startPage, endPage - startPage + 1).ToList();

            return View(pagedList);
        }
        public async Task<IActionResult> DetailTopic(string id)
        {
            if (string.IsNullOrEmpty(id))
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
    }
}