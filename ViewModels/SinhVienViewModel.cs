using System;
using System.ComponentModel.DataAnnotations;

namespace BTL_UML.ViewModels
{
    public class SinhVienViewModel
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime NgaySinh { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [RegularExpression("Nam|Nữ", ErrorMessage = "Giới tính phải là 'Nam' hoặc 'Nữ'")]
        public string GioiTinh { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Quê quán là bắt buộc")]
        [StringLength(255, ErrorMessage = "Quê quán không được vượt quá 255 ký tự")]
        public string QueQuan { get; set; }

        [StringLength(255, ErrorMessage = "Đường dẫn ảnh đại diện không được vượt quá 255 ký tự")]
        public string ?AnhDaiDien { get; set; }

        [Required(ErrorMessage = "Số CMND là bắt buộc")]
        [RegularExpression(@"^\d{9,12}$", ErrorMessage = "Số CMND phải là chuỗi số có 9 đến 12 chữ số")]
        public string SoCmnd { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải là 10 chữ số")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Tên lớp là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên lớp không được vượt quá 100 ký tự")]
        public string TenLop { get; set; }
    }
}
