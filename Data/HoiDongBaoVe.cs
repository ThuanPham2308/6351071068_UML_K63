using System;
using System.Collections.Generic;

namespace BTL_UML.Data;

public partial class HoiDongBaoVe
{
    public string MaHoiDong { get; set; } = null!;

    public string TenHoiDong { get; set; } = null!;

    public DateTime NgayThanhLap { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<ThanhVienHoiDong> ThanhVienHoiDongs { get; set; } = new List<ThanhVienHoiDong>();
}
