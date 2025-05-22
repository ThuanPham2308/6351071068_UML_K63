using System;
using System.Collections.Generic;

namespace BTL_UML.Data;

public partial class ThanhVienHoiDong
{
    public string MaHoiDong { get; set; } = null!;

    public string MaGiangVien { get; set; } = null!;

    public string VaiTro { get; set; } = null!;

    public virtual GiangVien MaGiangVienNavigation { get; set; } = null!;

    public virtual HoiDongBaoVe MaHoiDongNavigation { get; set; } = null!;
}
