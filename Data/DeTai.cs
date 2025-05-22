using System;
using System.Collections.Generic;

namespace BTL_UML.Data;

public partial class DeTai
{
    public string MaDeTai { get; set; } = null!;

    public string TenDeTai { get; set; } = null!;

    public string MoTa { get; set; } = null!;

    public string MaSinhVien { get; set; } = null!;

    public string? MaKi { get; set; }

    public DateTime? NgayDangKy { get; set; }

    public string? TrangThaiDuyet { get; set; }

    public virtual ICollection<BaoCaoTienDo> BaoCaoTienDos { get; set; } = new List<BaoCaoTienDo>();

    public virtual ICollection<DeCuongKeHoach> DeCuongKeHoaches { get; set; } = new List<DeCuongKeHoach>();

    public virtual ICollection<KetQua> KetQuas { get; set; } = new List<KetQua>();

    public virtual ICollection<LuanVan> LuanVans { get; set; } = new List<LuanVan>();

    public virtual KiBaoVeDoAn? MaKiNavigation { get; set; }

    public virtual SinhVien MaSinhVienNavigation { get; set; } = null!;

    public virtual ICollection<PhanCongGiangVien> PhanCongGiangViens { get; set; } = new List<PhanCongGiangVien>();
}
