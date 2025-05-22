using System.ComponentModel.DataAnnotations;

namespace BTL_UML.ViewModels
{
    public class KiBaoVeDoAnViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Vui lòng nhập tên kỳ")]
        [Display(Name = "Tên kỳ")]
        [StringLength(100, ErrorMessage = "Tên kỳ không được vượt quá 100 ký tự")]
        public string TenKi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập năm học")]
        [Display(Name = "Năm học")]
        [StringLength(100, ErrorMessage = "Năm học không được vượt quá 100 ký tự")]
        public string NamHoc { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn học kỳ")]
        [Display(Name = "Học kỳ")]
        public string HocKy { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime NgayBatDau { get; set; }

        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime NgayKetThuc { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(255, ErrorMessage = "Ghi chú không được vượt quá 255 ký tự")]
        public string? GhiChu { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (NgayBatDau == DateTime.MinValue)
            {
                yield return new ValidationResult("Vui lòng chọn ngày bắt đầu", new[] { nameof(NgayBatDau) });
            }

            if (NgayKetThuc == DateTime.MinValue)
            {
                yield return new ValidationResult("Vui lòng chọn ngày kết thúc", new[] { nameof(NgayKetThuc) });
            }

            if (NgayBatDau != DateTime.MinValue && NgayKetThuc != DateTime.MinValue && NgayKetThuc <= NgayBatDau)
            {
                yield return new ValidationResult("Ngày kết thúc phải sau ngày bắt đầu", new[] { nameof(NgayKetThuc) });
            }
        }
    }
}
