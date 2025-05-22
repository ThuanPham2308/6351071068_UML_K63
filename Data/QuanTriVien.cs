using System;
using System.Collections.Generic;

namespace BTL_UML.Data;

public partial class QuanTriVien
{
    public string MaQuanTri { get; set; } = null!;

    public string? MaNguoiDung { get; set; }

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }
}
