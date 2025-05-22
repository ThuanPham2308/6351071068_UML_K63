using System;
using System.Collections.Generic;

namespace BTL_UML.Data;

public partial class PhanCongGiangVien
{
    public string MaDeTai { get; set; } = null!;

    public string MaGiangVien { get; set; } = null!;

    public string VaiTro { get; set; } = null!;

    public virtual DeTai MaDeTaiNavigation { get; set; } = null!;

    public virtual GiangVien MaGiangVienNavigation { get; set; } = null!;
}
