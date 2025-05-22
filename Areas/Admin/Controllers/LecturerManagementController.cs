using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BTL_UML.Data;
using BTL_UML.ViewModels;

namespace BTL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị")]
    public class LecturerManagementController : Controller
    {
        private readonly QldaLvContext _context;

        public LecturerManagementController(QldaLvContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search, string sortOrder, string departmentFilter, int? page)
        {
            var maNguoiDung = HttpContext.Session.GetString("MaNguoiDung");
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            ViewData["MaNguoiDung"] = maNguoiDung;
            ViewData["VaiTro"] = vaiTro;

            var user = _context.NguoiDungs.FirstOrDefault(u => u.MaNguoiDung == maNguoiDung);
            ViewData["TenNguoiDung"] = user?.HoTen;
            ViewData["Avatar"] = "~/shared/images/default-avatar.jpg";

            var departments = _context.GiangViens
                                      .Select(gv => gv.ChuyenMon)
                                      .Distinct()
                                      .OrderBy(d => d)
                                      .ToList();

            ViewBag.DepartmentList = new SelectList(departments, selectedValue: departmentFilter);

            int pageSize = 10;
            int pageNum = page ?? 1;

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentDepartment = departmentFilter;

            ViewBag.SupervisingSortParam = sortOrder == "supervising_asc" ? "supervising_desc" : "supervising_asc";
            ViewBag.DefendingSortParam = sortOrder == "defending_asc" ? "defending_desc" : "defending_asc";

            var giangViens = _context.GiangViens
                                     .Include(gv => gv.MaNguoiDungNavigation)
                                     .Include(gv => gv.PhanCongGiangViens)
                                     .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                giangViens = giangViens.Where(gv => gv.MaNguoiDungNavigation.HoTen.Contains(search) ||
                                                     gv.MaGiangVien.Contains(search));
            }

            if (!string.IsNullOrEmpty(departmentFilter))
            {
                giangViens = giangViens.Where(gv => gv.ChuyenMon == departmentFilter);
            }

            var lecturersWithProjectCounts = giangViens.Select(gv => new
            {
                GiangVien = gv,
                SupervisingCount = gv.PhanCongGiangViens.Count(pc => pc.VaiTro == "Hướng dẫn"),
                DefendingCount = gv.PhanCongGiangViens.Count(pc => pc.VaiTro == "Biện luận")
            });

            switch (sortOrder)
            {
                case "supervising_asc":
                    lecturersWithProjectCounts = lecturersWithProjectCounts.OrderBy(g => g.SupervisingCount);
                    break;
                case "supervising_desc":
                    lecturersWithProjectCounts = lecturersWithProjectCounts.OrderByDescending(g => g.SupervisingCount);
                    break;
                case "defending_asc":
                    lecturersWithProjectCounts = lecturersWithProjectCounts.OrderBy(g => g.DefendingCount);
                    break;
                case "defending_desc":
                    lecturersWithProjectCounts = lecturersWithProjectCounts.OrderByDescending(g => g.DefendingCount);
                    break;
                default:
                    lecturersWithProjectCounts = lecturersWithProjectCounts.OrderBy(g => g.GiangVien.MaGiangVien);
                    break;
            }

            var giangVienList = lecturersWithProjectCounts.Select(g => g.GiangVien).ToList();

            var pagedGiangViens = giangVienList.ToPagedList(pageNum, pageSize);

            int totalPages = pagedGiangViens.PageCount;
            int currentPage = pagedGiangViens.PageNumber;
            int startPage = Math.Max(1, currentPage - 1);
            int endPage = Math.Min(totalPages, currentPage + 1);

            ViewBag.PagesToShow = Enumerable.Range(startPage, endPage - startPage + 1).ToList();

            return View(pagedGiangViens);
        }

        public IActionResult Create()
        {
            ViewBag.HocViList = new SelectList(new List<string> { "Kỹ sư", "Thạc sĩ", "Tiến sĩ", "Giáo sư" });
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GiangVienViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.HocViList = new SelectList(new List<string> { "Kỹ sư", "Thạc sĩ", "Tiến sĩ", "Giáo sư" }, viewModel.HocVi);
                return View(viewModel);
            }

            var existingMaNguoiDungList = _context.NguoiDungs.Select(nd => nd.MaNguoiDung);
            string maNguoiDung = GenerateNewCode("ND", existingMaNguoiDungList);

            var existingMaGiangVienList = _context.GiangViens.Select(gv => gv.MaGiangVien);
            string maGiangVien = GenerateNewCode("GV", existingMaGiangVienList);

            var nguoiDung = new NguoiDung
            {
                MaNguoiDung = maNguoiDung,
                HoTen = viewModel.HoTen,
                NgaySinh = viewModel.NgaySinh,
                GioiTinh = viewModel.GioiTinh,
                DiaChi = viewModel.DiaChi,
                QueQuan = viewModel.QueQuan,
                AnhDaiDien = viewModel.AnhDaiDien ?? "",
                SoCmnd = viewModel.SoCmnd,
                Email = viewModel.Email,
                SoDienThoai = viewModel.SoDienThoai,
                TenDangNhap = viewModel.TenDangNhap,
                MatKhau = viewModel.NgaySinh.ToString("dd/MM/yyyy"),
                VaiTro = "Giảng viên"
            };

            var giangVien = new GiangVien
            {
                MaGiangVien = maGiangVien,
                MaNguoiDung = maNguoiDung,
                ChucVu = viewModel.ChucVu,
                ChuyenMon = viewModel.ChuyenMon,
                HocVi = viewModel.HocVi
            };

            try
            {
                _context.NguoiDungs.Add(nguoiDung);
                _context.GiangViens.Add(giangVien);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
                ViewBag.HocViList = new SelectList(new List<string> { "Kỹ sư", "Thạc sĩ", "Tiến sĩ", "Giáo sư" }, viewModel.HocVi);
                return View(viewModel);
            }
        }
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Error", "Home");

            var giangVien = await _context.GiangViens
                .Include(gv => gv.MaNguoiDungNavigation)
                .FirstOrDefaultAsync(gv => gv.MaGiangVien == id);

            if (giangVien == null || giangVien.MaNguoiDungNavigation == null)
                return RedirectToAction("Error", "Home");

            TempData["MaGiangVien"] = giangVien.MaGiangVien;

            var viewModel = new GiangVienViewModel
            {
                HoTen = giangVien.MaNguoiDungNavigation.HoTen,
                NgaySinh = giangVien.MaNguoiDungNavigation.NgaySinh,
                GioiTinh = giangVien.MaNguoiDungNavigation.GioiTinh,
                DiaChi = giangVien.MaNguoiDungNavigation.DiaChi,
                QueQuan = giangVien.MaNguoiDungNavigation.QueQuan,
                AnhDaiDien = giangVien.MaNguoiDungNavigation.AnhDaiDien ?? "",
                SoCmnd = giangVien.MaNguoiDungNavigation.SoCmnd,
                Email = giangVien.MaNguoiDungNavigation.Email,
                SoDienThoai = giangVien.MaNguoiDungNavigation.SoDienThoai,
                TenDangNhap = giangVien.MaNguoiDungNavigation.TenDangNhap,
                ChucVu = giangVien.ChucVu,
                ChuyenMon = giangVien.ChuyenMon,
                HocVi = giangVien.HocVi
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GiangVienViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var maGiangVien = TempData["MaGiangVien"]?.ToString();
            if (string.IsNullOrEmpty(maGiangVien))
            {
                ModelState.AddModelError("", "Không xác định được mã giảng viên.");
                return View(viewModel);
            }

            var giangVien = await _context.GiangViens
                .Include(gv => gv.MaNguoiDungNavigation)
                .FirstOrDefaultAsync(gv => gv.MaGiangVien == maGiangVien);

            if (giangVien == null || giangVien.MaNguoiDungNavigation == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var nguoiDung = giangVien.MaNguoiDungNavigation;

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

            giangVien.ChucVu = viewModel.ChucVu;
            giangVien.ChuyenMon = viewModel.ChuyenMon;
            giangVien.HocVi = viewModel.HocVi;

            try
            {
                _context.Update(nguoiDung);
                _context.Update(giangVien);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật thông tin giảng viên thành công.";
                return RedirectToAction(nameof(Index), new { id = giangVien.MaGiangVien });
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
            var giangVien = _context.GiangViens.Find(id);
            if (giangVien == null)
            {
                return NotFound();
            }

            bool dangPhuTrachLop = _context.LopHocs.Any(l => l.MaGiangVien == id);

            bool dangPhanCong = _context.PhanCongGiangViens.Any(pc => pc.MaGiangVien == id);

            if (dangPhuTrachLop || dangPhanCong)
            {
                TempData["ErrorMessage"] = "Không thể xóa giảng viên vì đang phụ trách lớp hoặc phân công đề tài.";
                return RedirectToAction("Index");
            }

            var tvhd = _context.ThanhVienHoiDongs.Where(hd => hd.MaGiangVien == id).ToList();
            _context.ThanhVienHoiDongs.RemoveRange(tvhd);

            var nguoiDung = _context.NguoiDungs.FirstOrDefault(nd => nd.MaNguoiDung == giangVien.MaNguoiDung);

            _context.GiangViens.Remove(giangVien);
            if (nguoiDung != null)
            {
                _context.NguoiDungs.Remove(nguoiDung);
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Xóa giảng viên thành công.";
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
