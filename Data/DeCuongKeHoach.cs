using System;
using System.Collections.Generic;

namespace BTL_UML.Data;

public partial class DeCuongKeHoach
{
    public string MaDeCuong { get; set; } = null!;

    public string MaDeTai { get; set; } = null!;

    public string NoiDungDeCuong { get; set; } = null!;

    public string KeHoachThucHien { get; set; } = null!;

    public DateTime NgayNop { get; set; }

    public string? GhiChu { get; set; }

    public virtual DeTai MaDeTaiNavigation { get; set; } = null!;
}
