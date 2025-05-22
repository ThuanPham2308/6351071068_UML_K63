using System;
using System.Collections.Generic;

namespace BTL_UML.Data;

public partial class LopHoc
{
    public string MaLop { get; set; } = null!;

    public string MaGiangVien { get; set; } = null!;

    public string TenLop { get; set; } = null!;

    public string Khoa { get; set; } = null!;

    public string NienKhoa { get; set; } = null!;

    public string? GhiChu { get; set; }

    public virtual GiangVien MaGiangVienNavigation { get; set; } = null!;

    public virtual ICollection<SinhVien> SinhViens { get; set; } = new List<SinhVien>();
}
