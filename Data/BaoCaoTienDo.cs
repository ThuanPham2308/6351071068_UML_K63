using System;
using System.Collections.Generic;

namespace BTL_UML.Data;

public partial class BaoCaoTienDo
{
    public string MaBctd { get; set; } = null!;

    public string MaDeTai { get; set; } = null!;

    public string? NoiDungBaoCao { get; set; }

    public string? TepTinNop { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public string? NhanXet { get; set; }

    public virtual DeTai MaDeTaiNavigation { get; set; } = null!;
}
