using System;
using System.Collections.Generic;

namespace BTL_UML.Data;

public partial class KetQua
{
    public string MaKetQua { get; set; } = null!;

    public string MaDeTai { get; set; } = null!;

    public decimal? DiemQuaTrinh { get; set; }

    public decimal? DiemBaoVe { get; set; }

    public string? NhanXet { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public virtual DeTai MaDeTaiNavigation { get; set; } = null!;
}
