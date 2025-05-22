using BTL_UML.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace BTL_UML.ViewModels
{
    public class DeTaiViewModel
    {
        [Required]
        [Display(Name = "Tên đề tài")]
        [StringLength(200)]
        public string TenDeTai { get; set; }

        [Required]
        [Display(Name = "Mô tả")]
        [StringLength(255)]
        public string MoTa { get; set; }

        [Required]
        [Display(Name = "Mã sinh viên")]
        public string MaSinhVien { get; set; }

        [Display(Name = "Mã kỳ bảo vệ")]
        public string? MaKi { get; set; }

        [Display(Name = "Ngày đăng ký")]
        [DataType(DataType.Date)]
        public DateTime? NgayDangKy { get; set; }

        [Display(Name = "Trạng thái duyệt")]
        public string TrangThaiDuyet { get; set; } = "Chờ duyệt";
        public List<PhanCongGiangVien> PhanCongGiangViens { get; set; } = new();
        public List<KetQua> KetQuas { get; set; } = new();

    }
}
