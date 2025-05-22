using System;
using System.Collections.Generic;

namespace BTL_UML.Data;

public partial class KiBaoVeDoAn
{
    public string MaKi { get; set; } = null!;

    public string TenKi { get; set; } = null!;

    public string NamHoc { get; set; } = null!;

    public string? HocKy { get; set; }

    public DateTime NgayBatDau { get; set; }

    public DateTime NgayKetThuc { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<DeTai> DeTais { get; set; } = new List<DeTai>();
}
