using BTL_UML.Data;

namespace BTL_UML.ViewModels
{
    public class ChiTietDeTaiViewModel
    {
        public string MaDeTai { get; set; }
        public string TenDeTai { get; set; }
        public string MoTa { get; set; }
        public string TrangThaiDuyet { get; set; }

        public string MaSinhVien { get; set; }
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string DiaChi { get; set; }
        public string Email { get; set; }

        public List<DeCuongKeHoach> DeCuongKeHoaches { get; set; } = new();
        public List<BaoCaoTienDo> BaoCaoTienDos { get; set; } = new();
        public List<LuanVan> LuanVans { get; set; } = new();
        public List<KetQua> KetQuas { get; set; } = new();
        public List<PhanCongGiangVien> PhanCongGiangViens { get; set; } = new();

    }
}