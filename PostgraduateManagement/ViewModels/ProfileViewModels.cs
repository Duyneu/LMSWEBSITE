// File: ViewModels/ProfileViewModels.cs
using System.ComponentModel.DataAnnotations;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.ViewModels
{
    /// <summary>
    /// Danh sách hồ sơ người học sau đại học kèm từ khóa tìm kiếm.
    /// </summary>
    public class StudentListViewModel
    {
        public string? Search { get; set; }
        public List<PostgraduateStudent> Students { get; set; } = new();
    }

    /// <summary>
    /// Biểu mẫu tạo mới / cập nhật hồ sơ người học sau đại học.
    /// </summary>
    public class StudentFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã người học dùng chung.")]
        [StringLength(50)]
        [Display(Name = "Mã người học dùng chung")]
        public string SharedStudentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(150)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập chuyên ngành.")]
        [StringLength(200)]
        [Display(Name = "Chuyên ngành đào tạo")]
        public string Major { get; set; } = string.Empty;

        [Range(1990, 2100, ErrorMessage = "Năm nhập học không hợp lệ.")]
        [Display(Name = "Năm nhập học")]
        public int EnrollmentYear { get; set; } = DateTime.Now.Year;
    }

    /// <summary>
    /// Danh sách chuyên gia / cán bộ kèm bộ lọc cơ hữu - ngoài trường.
    /// </summary>
    public class ExpertListViewModel
    {
        public string? Search { get; set; }
        public bool? IsExternal { get; set; }
        public List<AcademicPerson> Experts { get; set; } = new();
    }

    /// <summary>
    /// Biểu mẫu tạo mới / cập nhật chuyên gia, cán bộ (bao gồm chuyên gia ngoài trường).
    /// </summary>
    public class ExpertFormViewModel
    {
        public int Id { get; set; }

        [StringLength(50)]
        [Display(Name = "Mã nhân sự dùng chung")]
        public string? SharedPersonCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(150)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Học hàm / học vị")]
        public AcademicDegree AcademicDegree { get; set; } = AcademicDegree.TS;

        [Required(ErrorMessage = "Vui lòng nhập lĩnh vực nghiên cứu.")]
        [StringLength(300)]
        [Display(Name = "Lĩnh vực nghiên cứu")]
        public string ResearchField { get; set; } = string.Empty;

        [Display(Name = "Chuyên gia ngoài trường")]
        public bool IsExternal { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [StringLength(250)]
        [Display(Name = "Đơn vị công tác")]
        public string? Workplace { get; set; }
    }
}
