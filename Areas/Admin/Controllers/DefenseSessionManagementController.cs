using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.Options;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using PagedList;
using BTL_UML.Data;
using BTL_UML.ViewModels;
using System.Threading;

namespace BTL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị")]
    public class DefenseSessionManagementController : Controller
    {
        private readonly QldaLvContext _context;

        public DefenseSessionManagementController(QldaLvContext context)
        {
            _context = context;
        }
        public IActionResult Index(string search, string maKiChon, string trangThai, int? page)
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
            ViewBag.CurrentTrangThai = trangThai;

            if (defaultKi == null)
                return View(new PagedList<KiBaoVeDoAn>(new List<KiBaoVeDoAn>(), 1, 1));

            var defenseSessions = _context.KiBaoVeDoAns
                .Include(k => k.DeTais).ThenInclude(dt => dt.PhanCongGiangViens).ThenInclude(p => p.MaGiangVienNavigation).ThenInclude(g => g.MaNguoiDungNavigation)
                .Include(k => k.DeTais).ThenInclude(dt => dt.MaSinhVienNavigation).ThenInclude(sv => sv.MaNguoiDungNavigation)
                .AsQueryable();

            if (maKiChon != "all" && !string.IsNullOrEmpty(maKiChon))
            {
                defenseSessions = defenseSessions.Where(k => k.MaKi == maKiChon);
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                defenseSessions = defenseSessions.Where(k =>
                    k.TenKi.ToLower().Contains(search) ||
                    k.DeTais.Any(dt => dt.TenDeTai.ToLower().Contains(search)) ||
                    k.DeTais.Any(dt => dt.MaSinhVienNavigation.MaNguoiDungNavigation.HoTen.ToLower().Contains(search)) ||
                    k.DeTais.Any(dt => dt.PhanCongGiangViens.Any(p => p.MaGiangVienNavigation.MaNguoiDungNavigation.HoTen.ToLower().Contains(search)))
                );
            }

            if (!string.IsNullOrEmpty(trangThai) && trangThai != "Tất cả")
            {
                defenseSessions = defenseSessions.Where(k =>
                    k.DeTais.Any(dt => dt.TrangThaiDuyet == trangThai)
                );
            }

            int pageSize = 10;
            int pageNum = page ?? 1;
            var pagedSessions = defenseSessions.OrderByDescending(k => k.NgayBatDau).ToList().ToPagedList(pageNum, pageSize);

            int totalPages = pagedSessions.PageCount;
            int startPage = Math.Max(1, pageNum - 1);
            int endPage = Math.Min(totalPages, pageNum + 1);

            ViewBag.PagesToShow = Enumerable.Range(startPage, endPage - startPage + 1).ToList();

            return View(pagedSessions);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KiBaoVeDoAnViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.NgayKetThuc <= viewModel.NgayBatDau)
                {
                    ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau ngày bắt đầu");
                    return View(viewModel);
                }

                var existingMaKiList = _context.KiBaoVeDoAns
                    .Select(k => k.MaKi)
                    .ToList();

                int maxIndex = 0;
                foreach (var maKi in existingMaKiList)
                {
                    if (maKi.StartsWith("KI") && int.TryParse(maKi.Substring(2), out int index))
                    {
                        if (index > maxIndex)
                            maxIndex = index;
                    }
                }

                string newMaKi = "KI" + (maxIndex + 1).ToString("D3");
                var kiBaoVeDoAn = new KiBaoVeDoAn
                {
                    MaKi = newMaKi,
                    TenKi = viewModel.TenKi,
                    NamHoc = viewModel.NamHoc,
                    HocKy = viewModel.HocKy,
                    NgayBatDau = viewModel.NgayBatDau,
                    NgayKetThuc = viewModel.NgayKetThuc,
                    GhiChu = viewModel.GhiChu
                };

                try
                {
                    _context.Add(kiBaoVeDoAn);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm kỳ bảo vệ thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
                }
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var ki = await _context.KiBaoVeDoAns.FindAsync(id);
            if (ki == null)
            {
                return Redirect("/Home/Error");
            }
            TempData["MaKi"] = ki.MaKi;
            var viewModel = new KiBaoVeDoAnViewModel
            {
                TenKi = ki.TenKi,
                NamHoc = ki.NamHoc,
                HocKy = ki.HocKy,
                NgayBatDau = ki.NgayBatDau,
                NgayKetThuc = ki.NgayKetThuc,
                GhiChu = ki.GhiChu
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(KiBaoVeDoAnViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            if (viewModel.NgayKetThuc <= viewModel.NgayBatDau)
            {
                ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau ngày bắt đầu");
                return View(viewModel);
            }

            var maKi = TempData["MaKi"]?.ToString();
            if (string.IsNullOrEmpty(maKi))
            {
                return Redirect("/Home/Error");
            }

            var ki = await _context.KiBaoVeDoAns.FindAsync(maKi);
            if (ki == null)
            {
                return Redirect("/Home/Error");
            }

            ki.TenKi = viewModel.TenKi;
            ki.NamHoc = viewModel.NamHoc;
            ki.HocKy = viewModel.HocKy;
            ki.NgayBatDau = viewModel.NgayBatDau;
            ki.NgayKetThuc = viewModel.NgayKetThuc;
            ki.GhiChu = viewModel.GhiChu;

            try
            {
                _context.Update(ki);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật kỳ bảo vệ thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật dữ liệu: " + ex.Message);
                return View(viewModel);
            }
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

        [HttpGet]
        public IActionResult CapNhatTrangThaiDuyet(string MaDeTai, string TrangThaiDuyet)
        {
            var deTai = _context.DeTais.Find(MaDeTai);
            if (deTai == null)
            {
                return NotFound();
            }

            deTai.TrangThaiDuyet = TrangThaiDuyet;
            _context.SaveChanges();

            return RedirectToAction("DetailTopic", "DefenseSessionManagement", new { area = "Admin", id = deTai.MaDeTai });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatGiangVien(string MaDeTai, string GiangVienHuongDanId, string GiangVienBienLuanId)
        {
            Console.WriteLine($"Action được gọi với: MaDeTai={MaDeTai}, GVHD={GiangVienHuongDanId}, GVBL={GiangVienBienLuanId}");

            try
            {
                if (string.IsNullOrEmpty(MaDeTai))
                {
                    Console.WriteLine("MaDeTai rỗng!");
                    TempData["ErrorMessage"] = "Mã đề tài không hợp lệ.";
                    return RedirectToAction("Index");
                }

                var deTai = await _context.DeTais
                    .Include(dt => dt.PhanCongGiangViens)
                    .FirstOrDefaultAsync(dt => dt.MaDeTai == MaDeTai);

                if (deTai == null)
                {
                    Console.WriteLine("Không tìm thấy đề tài!");
                    TempData["ErrorMessage"] = "Không tìm thấy đề tài.";
                    return RedirectToAction("DetailTopic", new { id = MaDeTai });
                }

                var huongDan = deTai.PhanCongGiangViens.FirstOrDefault(pc => pc.VaiTro == "Hướng dẫn");
                if (!string.IsNullOrEmpty(GiangVienHuongDanId))
                {
                    if (huongDan != null)
                    {
                        huongDan.MaGiangVien = GiangVienHuongDanId;
                        _context.PhanCongGiangViens.Update(huongDan);
                    }
                    else
                    {
                        _context.PhanCongGiangViens.Add(new PhanCongGiangVien
                        {
                            MaDeTai = MaDeTai,
                            MaGiangVien = GiangVienHuongDanId,
                            VaiTro = "Hướng dẫn"
                        });
                    }
                }
                else if (huongDan != null)
                {
                    _context.PhanCongGiangViens.Remove(huongDan);
                }

                var bienLuan = deTai.PhanCongGiangViens.FirstOrDefault(pc => pc.VaiTro == "Biện luận");
                if (!string.IsNullOrEmpty(GiangVienBienLuanId))
                {
                    if (bienLuan != null)
                    {
                        bienLuan.MaGiangVien = GiangVienBienLuanId;
                        _context.PhanCongGiangViens.Update(bienLuan);
                    }
                    else
                    {
                        _context.PhanCongGiangViens.Add(new PhanCongGiangVien
                        {
                            MaDeTai = MaDeTai,
                            MaGiangVien = GiangVienBienLuanId,
                            VaiTro = "Biện luận"
                        });
                    }
                }
                else if (bienLuan != null)
                {
                    _context.PhanCongGiangViens.Remove(bienLuan);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("Cập nhật thành công!");

                TempData["SuccessMessage"] = "Cập nhật giảng viên thành công.";
                return RedirectToAction("DetailTopic", new { id = MaDeTai });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("DetailTopic", new { id = MaDeTai });
            }
        }
    }
}
