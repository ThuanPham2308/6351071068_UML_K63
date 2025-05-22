using System;
using System.Collections.Generic;

namespace BTL_UML.Data;

public partial class LuanVan
{
    public string MaLuanVan { get; set; } = null!;

    public string MaDeTai { get; set; } = null!;

    public DateTime? NgayNop { get; set; }

    public string? TepTinNop { get; set; }

    public string? MoTa { get; set; }

    public string? XacNhanBaoVe { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? NgayDuyet { get; set; }

    public string? TrangThaiDuyet { get; set; }

    public virtual DeTai MaDeTaiNavigation { get; set; } = null!;
}
