using System;
using System.Collections.Generic;

namespace BTL_UML.Data;

public partial class GiangVien
{
    public string MaGiangVien { get; set; } = null!;

    public string ChucVu { get; set; } = null!;

    public string ChuyenMon { get; set; } = null!;

    public string HocVi { get; set; } = null!;

    public string? MaNguoiDung { get; set; }

    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }

    public virtual ICollection<PhanCongGiangVien> PhanCongGiangViens { get; set; } = new List<PhanCongGiangVien>();

    public virtual ICollection<ThanhVienHoiDong> ThanhVienHoiDongs { get; set; } = new List<ThanhVienHoiDong>();
}
