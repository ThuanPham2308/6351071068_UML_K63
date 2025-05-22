using System;
using System.Collections.Generic;

namespace BTL_UML.Data;

public partial class SinhVien
{
    public string MaSinhVien { get; set; } = null!;

    public string MaLop { get; set; } = null!;

    public int? NamHoc { get; set; }

    public string? MaNguoiDung { get; set; }

    public virtual ICollection<DeTai> DeTais { get; set; } = new List<DeTai>();

    public virtual LopHoc MaLopNavigation { get; set; } = null!;

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }
}
