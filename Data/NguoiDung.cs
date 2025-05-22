using System;
using System.Collections.Generic;

namespace BTL_UML.Data;

public partial class NguoiDung
{
    public string MaNguoiDung { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public DateTime NgaySinh { get; set; }

    public string GioiTinh { get; set; } = null!;

    public string DiaChi { get; set; } = null!;

    public string QueQuan { get; set; } = null!;

    public string? AnhDaiDien { get; set; }

    public string SoCmnd { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string VaiTro { get; set; } = null!;

    public virtual GiangVien? GiangVien { get; set; }

    public virtual QuanTriVien? QuanTriVien { get; set; }

    public virtual SinhVien? SinhVien { get; set; }
}
