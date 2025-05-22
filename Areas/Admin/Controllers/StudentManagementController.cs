using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using PagedList;
using BTL_UML.Data;
using BTL_UML.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BTL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị")]
    public class StudentManagementController : Controller
    {
        private readonly QldaLvContext _context;

        public StudentManagementController(QldaLvContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search, string classFilter, string semesterFilter, int? page)
        {
            var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            ViewData["MaNguoiDung"] = maNguoiDung;
            ViewData["VaiTro"] = vaiTro;

            var user = _context.NguoiDungs.FirstOrDefault(u => u.MaNguoiDung == maNguoiDung);
            ViewData["TenNguoiDung"] = user?.HoTen;
            ViewData["Avatar"] = "~/shared/images/default-avatar.jpg";

            int pageSize = 10;
            int pageNum = (page ?? 1);

            ViewBag.CurrentSearch = search;
            ViewBag.ClassFilter = classFilter;
            ViewBag.SemesterFilter = semesterFilter;

            var classList = _context.LopHocs
                            .Select(l => new SelectListItem
                            {
                                Value = l.MaLop,
                                Text = l.TenLop
                            }).ToList();
            ViewBag.ClassList = new SelectList(classList, "Value", "Text");

            var semesterList = _context.KiBaoVeDoAns
                                .Select(k => new SelectListItem
                                {
                                    Value = k.MaKi,
                                    Text = k.TenKi
                                }).ToList();
            ViewBag.SemesterList = new SelectList(semesterList, "Value", "Text");

            var sinhViens = _context.SinhViens
                .Include(sv => sv.MaLopNavigation)
                .Include(sv => sv.MaNguoiDungNavigation)
                .Include(sv => sv.DeTais)
                .ThenInclude(dt => dt.MaKiNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                sinhViens = sinhViens.Where(sv => sv.MaNguoiDungNavigation.HoTen.Contains(search) ||
                                                  sv.MaSinhVien.Contains(search));
            }

            if (!string.IsNullOrEmpty(classFilter))
            {
                sinhViens = sinhViens.Where(sv => sv.MaLop == classFilter);
            }

            if (!string.IsNullOrEmpty(semesterFilter))
            {
                sinhViens = sinhViens.Where(sv => sv.DeTais.Any(dt => dt.MaKi == semesterFilter));
            }

            var pagedSinhViens = sinhViens.ToList().ToPagedList(pageNum, pageSize);

            int totalPages = pagedSinhViens.PageCount;
            int currentPage = pagedSinhViens.PageNumber;
            int startPage = Math.Max(1, currentPage - 1);
            int endPage = Math.Min(totalPages, currentPage + 1);

            ViewBag.PagesToShow = Enumerable.Range(startPage, endPage - startPage + 1).ToList();

            return View(pagedSinhViens);
        }

        public IActionResult Create()
        {
            var lopList = _context.LopHocs.Select(l => l.TenLop).ToList();
            ViewBag.LopHocList = new SelectList(lopList);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SinhVienViewModel viewModel)
        {
            var lopList = _context.LopHocs.Select(l => l.TenLop).ToList();
            ViewBag.LopHocList = new SelectList(lopList, viewModel.TenLop);

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var lop = await _context.LopHocs.FirstOrDefaultAsync(l => l.TenLop == viewModel.TenLop);
            if (lop == null)
            {
                ModelState.AddModelError("TenLop", "Lớp học không tồn tại.");
                return View(viewModel);
            }

            var existingMaNguoiDungList = _context.NguoiDungs.Select(nd => nd.MaNguoiDung);
            string maNguoiDung = GenerateNewCode("ND", existingMaNguoiDungList);

            var existingMaSinhVienList = _context.SinhViens.Select(sv => sv.MaSinhVien);
            string maSinhVien = GenerateNewCode("SV", existingMaSinhVienList);

            var nguoiDung = new NguoiDung
            {
                MaNguoiDung = maNguoiDung,
                HoTen = viewModel.HoTen,
                NgaySinh = viewModel.NgaySinh,
                GioiTinh = viewModel.GioiTinh,
                DiaChi = viewModel.DiaChi ?? "",
                QueQuan = viewModel.QueQuan ?? "",
                AnhDaiDien = viewModel.AnhDaiDien ?? "",
                SoCmnd = viewModel.SoCmnd,
                Email = viewModel.Email,
                SoDienThoai = viewModel.SoDienThoai,
                TenDangNhap = viewModel.TenDangNhap,
                MatKhau = viewModel.NgaySinh.ToString("dd/MM/yyyy"),
                VaiTro = "Sinh viên"
            };

            var sinhVien = new SinhVien
            {
                MaSinhVien = maSinhVien,
                MaLop = lop.MaLop,
                NamHoc = 1,
                MaNguoiDung = maNguoiDung
            };

            try
            {
                _context.NguoiDungs.Add(nguoiDung);
                _context.SinhViens.Add(sinhVien);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
                return View(viewModel);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Error", "Home");

            var sinhVien = await _context.SinhViens
                .Include(sv => sv.MaNguoiDungNavigation)
                .Include(sv => sv.MaLopNavigation)
                .FirstOrDefaultAsync(sv => sv.MaSinhVien == id);

            if (sinhVien == null || sinhVien.MaNguoiDungNavigation == null || sinhVien.MaLopNavigation == null)
            {
                return RedirectToAction("Error", "Home");
            }

            TempData["MaSinhVien"] = sinhVien.MaSinhVien;

            var viewModel = new SinhVienViewModel
            {
                HoTen = sinhVien.MaNguoiDungNavigation.HoTen,
                NgaySinh = sinhVien.MaNguoiDungNavigation.NgaySinh,
                GioiTinh = sinhVien.MaNguoiDungNavigation.GioiTinh,
                DiaChi = sinhVien.MaNguoiDungNavigation.DiaChi,
                QueQuan = sinhVien.MaNguoiDungNavigation.QueQuan,
                AnhDaiDien = sinhVien.MaNguoiDungNavigation.AnhDaiDien ?? "",
                SoCmnd = sinhVien.MaNguoiDungNavigation.SoCmnd,
                Email = sinhVien.MaNguoiDungNavigation.Email,
                SoDienThoai = sinhVien.MaNguoiDungNavigation.SoDienThoai,
                TenDangNhap = sinhVien.MaNguoiDungNavigation.TenDangNhap,
                TenLop = sinhVien.MaLopNavigation.TenLop
            };

            var lopList = _context.LopHocs.Select(l => l.TenLop).ToList();
            ViewBag.LopHocList = new SelectList(lopList, viewModel.TenLop);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SinhVienViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var maSinhVien = TempData["MaSinhVien"]?.ToString();
            if (string.IsNullOrEmpty(maSinhVien))
            {
                ModelState.AddModelError("", "Không xác định được mã sinh viên.");
                return View(viewModel);
            }

            var sinhVien = await _context.SinhViens
                .Include(sv => sv.MaNguoiDungNavigation)
                .Include(sv => sv.MaLopNavigation)
                .FirstOrDefaultAsync(sv => sv.MaSinhVien == maSinhVien);

            if (sinhVien == null || sinhVien.MaNguoiDungNavigation == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var nguoiDung = sinhVien.MaNguoiDungNavigation;
            nguoiDung.HoTen = viewModel.HoTen;
            nguoiDung.NgaySinh = viewModel.NgaySinh;
            nguoiDung.GioiTinh = viewModel.GioiTinh;
            nguoiDung.DiaChi = viewModel.DiaChi;
            nguoiDung.QueQuan = viewModel.QueQuan;
            nguoiDung.AnhDaiDien = viewModel.AnhDaiDien ?? "";
            nguoiDung.SoCmnd = viewModel.SoCmnd;
            nguoiDung.Email = viewModel.Email;
            nguoiDung.SoDienThoai = viewModel.SoDienThoai;
            nguoiDung.TenDangNhap = viewModel.TenDangNhap;

            if (!string.IsNullOrEmpty(viewModel.TenLop))
            {
                var lop = await _context.LopHocs.FirstOrDefaultAsync(l => l.TenLop == viewModel.TenLop);
                if (lop == null)
                {
                    ModelState.AddModelError("TenLop", "Lớp học không tồn tại.");
                    return View(viewModel);
                }
                sinhVien.MaLop = lop.MaLop;
            }

            try
            {
                _context.Update(nguoiDung);
                _context.Update(sinhVien);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật thông tin sinh viên thành công.";
                return RedirectToAction(nameof(Index), new { id = sinhVien.MaSinhVien });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật dữ liệu: " + ex.Message);
                return View(viewModel);
            }
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var sinhVien = _context.SinhViens.Find(id);
            if (sinhVien != null)
            {
                var dt = _context.DeTais.FirstOrDefault(d => d.MaSinhVien == id);

                if (dt != null)
                {
                    var baoCaoTienDos = _context.BaoCaoTienDos.Where(b => b.MaDeTai == dt.MaDeTai).ToList();
                    var deCuongs = _context.DeCuongKeHoaches.Where(dc => dc.MaDeTai == dt.MaDeTai).ToList();
                    var ketQuas = _context.KetQuas.Where(k => k.MaDeTai == dt.MaDeTai).ToList();
                    var luanVans = _context.LuanVans.Where(lv => lv.MaDeTai == dt.MaDeTai).ToList();
                    var phanCongs = _context.PhanCongGiangViens.Where(pc => pc.MaDeTai == dt.MaDeTai).ToList();

                    _context.BaoCaoTienDos.RemoveRange(baoCaoTienDos);
                    _context.DeCuongKeHoaches.RemoveRange(deCuongs);
                    _context.KetQuas.RemoveRange(ketQuas);
                    _context.LuanVans.RemoveRange(luanVans);
                    _context.PhanCongGiangViens.RemoveRange(phanCongs);

                    _context.DeTais.Remove(dt);
                }

                var nd = _context.NguoiDungs.FirstOrDefault(n => n.MaNguoiDung == sinhVien.MaNguoiDung);
                _context.SinhViens.Remove(sinhVien);
                if (nd != null)
                {
                    _context.NguoiDungs.Remove(nd);
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        private string GenerateNewCode(string prefix, IQueryable<string> existingCodes)
        {
            int maxIndex = 0;
            foreach (var code in existingCodes)
            {
                if (code.StartsWith(prefix) && int.TryParse(code.Substring(prefix.Length), out int index))
                {
                    if (index > maxIndex)
                        maxIndex = index;
                }
            }
            return prefix + (maxIndex + 1).ToString("D3");
        }
    }
}
