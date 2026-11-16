// File: ViewModels/ThesisViewModels.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Services.DTOs;

namespace PostgraduateManagement.ViewModels
{
    /// <summary>
    /// ViewModel đại diện cho một bản ghi đề tài trong danh sách hiển thị.
    /// Hoàn toàn tách biệt khỏi Domain Model.
    /// </summary>
    public class ThesisItemViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public ThesisType ThesisType { get; set; }
        public string ThesisTypeName => ThesisType switch
        {
            ThesisType.DeAn => "Đề án thạc sĩ",
            ThesisType.LuanVan => "Luận văn thạc sĩ",
            ThesisType.LuanAn => "Luận án tiến sĩ",
            _ => ThesisType.ToString()
        };
        public string Field { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ThesisStatus ThesisStatus { get; set; }
        public string ThesisStatusName => ThesisStatus switch
        {
            ThesisStatus.Draft => "Dự thảo",
            ThesisStatus.Registered => "Đã đăng ký",
            ThesisStatus.Approved => "Đã phê duyệt",
            ThesisStatus.InProgress => "Đang thực hiện",
            ThesisStatus.SubmittedForReview => "Chờ phản biện & bảo vệ",
            ThesisStatus.Defended => "Đã bảo vệ",
            ThesisStatus.Completed => "Hoàn thành",
            _ => ThesisStatus.ToString()
        };
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public bool IsGraduationCapstone { get; set; }
        public List<string> Supervisors { get; set; } = new List<string>();
        public string SupervisorNames => Supervisors.Count > 0 ? string.Join(", ", Supervisors) : "Chưa phân công";
        public int SubmittedDocumentsCount { get; set; }
        public bool HasScheduledDefense { get; set; }
    }

    /// <summary>
    /// ViewModel cho trang danh sách đề tài sau đại học kèm tìm kiếm, lọc và phân trang.
    /// </summary>
    public class ThesisListViewModel
    {
        /// <summary>
        /// Danh sách đề tài đã phân trang
        /// </summary>
        public List<ThesisItemViewModel> Theses { get; set; } = new List<ThesisItemViewModel>();

        /// <summary>
        /// Từ khóa tìm kiếm theo mã, tiêu đề, học viên, lĩnh vực
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Bộ lọc theo trạng thái đề tài
        /// </summary>
        public ThesisStatus? Status { get; set; }

        /// <summary>
        /// Bộ lọc theo loại hình đề tài
        /// </summary>
        public ThesisType? Type { get; set; }

        /// <summary>
        /// Trang hiện tại (1-based)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Số bản ghi trên mỗi trang
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Tổng số đề tài thỏa điều kiện lọc
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Tổng số trang
        /// </summary>
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

        /// <summary>
        /// Kiểm tra có trang trước hay không
        /// </summary>
        public bool HasPreviousPage => Page > 1;

        /// <summary>
        /// Kiểm tra có trang tiếp theo hay không
        /// </summary>
        public bool HasNextPage => Page < TotalPages;

        /// <summary>
        /// Danh sách tùy chọn trạng thái phục vụ dropdown lọc
        /// </summary>
        public List<SelectListItem> StatusOptions { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Danh sách tùy chọn loại hình đề tài phục vụ dropdown lọc
        /// </summary>
        public List<SelectListItem> TypeOptions { get; set; } = new List<SelectListItem>();
    }

    /// <summary>
    /// ViewModel chi tiết đề tài sau đại học bao gồm toàn bộ thông tin liên kết:
    /// học viên, phân công chuyên gia, thẩm định điều kiện, phiên bảo vệ và tài liệu số hóa.
    /// </summary>
    public class ThesisDetailViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public ThesisType ThesisType { get; set; }
        public string ThesisTypeName => ThesisType switch
        {
            ThesisType.DeAn => "Đề án thạc sĩ",
            ThesisType.LuanVan => "Luận văn thạc sĩ",
            ThesisType.LuanAn => "Luận án tiến sĩ",
            _ => ThesisType.ToString()
        };
        public string Field { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ThesisStatus ThesisStatus { get; set; }
        public string ThesisStatusName => ThesisStatus switch
        {
            ThesisStatus.Draft => "Dự thảo",
            ThesisStatus.Registered => "Đã đăng ký",
            ThesisStatus.Approved => "Đã phê duyệt",
            ThesisStatus.InProgress => "Đang thực hiện",
            ThesisStatus.SubmittedForReview => "Chờ phản biện & bảo vệ",
            ThesisStatus.Defended => "Đã bảo vệ",
            ThesisStatus.Completed => "Hoàn thành",
            _ => ThesisStatus.ToString()
        };
        public bool IsGraduationCapstone { get; set; }
        public string? Abstract { get; set; }

        // Thông tin học viên
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string StudentMajor { get; set; } = string.Empty;
        public int StudentEnrollmentYear { get; set; }

        // Danh sách liên kết
        public List<AssignmentItemViewModel> Assignments { get; set; } = new List<AssignmentItemViewModel>();
        public List<EligibilityCheckItemViewModel> EligibilityChecks { get; set; } = new List<EligibilityCheckItemViewModel>();
        public List<DefenseSessionItemViewModel> DefenseSessions { get; set; } = new List<DefenseSessionItemViewModel>();
        public List<DocumentItemViewModel> SubmittedDocuments { get; set; } = new List<DocumentItemViewModel>();
        public DigitizationProgressDto? DigitizationProgress { get; set; }

        // Cờ điều khiển các nút thao tác quy trình nghiệp vụ
        public bool CanApprove => ThesisStatus == ThesisStatus.Draft || ThesisStatus == ThesisStatus.Registered;
        public bool CanSubmitForReview => ThesisStatus == ThesisStatus.Approved || ThesisStatus == ThesisStatus.InProgress;
        public bool CanScheduleDefense => IsGraduationCapstone &&
            (ThesisStatus == ThesisStatus.Approved || ThesisStatus == ThesisStatus.InProgress || ThesisStatus == ThesisStatus.SubmittedForReview);
    }

    /// <summary>
    /// ViewModel biểu diễn một lần kiểm tra điều kiện quy chế đào tạo.
    /// </summary>
    public class EligibilityCheckItemViewModel
    {
        public int Id { get; set; }
        public int ThesisId { get; set; }
        public int PersonId { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public AcademicDegree AcademicDegree { get; set; }
        public bool IsExternal { get; set; }
        public AssignmentRole Role { get; set; }
        public string RoleName => Role switch
        {
            AssignmentRole.Supervisor => "Hướng dẫn",
            AssignmentRole.Reviewer => "Phản biện",
            AssignmentRole.Committee => "Hội đồng",
            _ => Role.ToString()
        };
        public CheckResult Result { get; set; }
        public string ResultName => Result switch
        {
            CheckResult.Eligible => "Đủ điều kiện",
            CheckResult.Ineligible => "Không đủ điều kiện",
            CheckResult.Conditional => "Có điều kiện",
            _ => Result.ToString()
        };
        public string Basis { get; set; } = string.Empty;
        public string RegulationVersion { get; set; } = string.Empty;
        public DateTime CheckedAt { get; set; }
    }

    /// <summary>
    /// ViewModel tạo mới đề tài sau đại học.
    /// </summary>
    public class CreateThesisViewModel
    {
        [Required(ErrorMessage = "Mã đề tài không được để trống")]
        [StringLength(50, ErrorMessage = "Mã đề tài tối đa 50 ký tự")]
        [Display(Name = "Mã đề tài")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên đề tài không được để trống")]
        [StringLength(500, ErrorMessage = "Tên đề tài tối đa 500 ký tự")]
        [Display(Name = "Tên đề tài")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lĩnh vực nghiên cứu không được để trống")]
        [StringLength(250, ErrorMessage = "Lĩnh vực nghiên cứu tối đa 250 ký tự")]
        [Display(Name = "Lĩnh vực nghiên cứu")]
        public string Field { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại hình đề tài")]
        [Display(Name = "Loại hình đề tài")]
        public ThesisType ThesisType { get; set; } = ThesisType.LuanVan;

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày dự kiến hoàn thành")]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.Date.AddMonths(12);

        [Required(ErrorMessage = "Vui lòng chọn học viên thực hiện")]
        [Display(Name = "Học viên thực hiện")]
        public int StudentId { get; set; }

        [Display(Name = "Đề tài tốt nghiệp sau đại học")]
        public bool IsGraduationCapstone { get; set; } = true;

        [Display(Name = "Tóm tắt nghiên cứu")]
        public string? Abstract { get; set; }

        /// <summary>
        /// Danh sách học viên khả dụng để chọn trong form
        /// </summary>
        public List<SelectListItem> AvailableStudents { get; set; } = new List<SelectListItem>();
    }

    /// <summary>
    /// ViewModel chỉnh sửa đề tài sau đại học.
    /// </summary>
    public class EditThesisViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã đề tài không được để trống")]
        [StringLength(50, ErrorMessage = "Mã đề tài tối đa 50 ký tự")]
        [Display(Name = "Mã đề tài")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên đề tài không được để trống")]
        [StringLength(500, ErrorMessage = "Tên đề tài tối đa 500 ký tự")]
        [Display(Name = "Tên đề tài")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lĩnh vực nghiên cứu không được để trống")]
        [StringLength(250, ErrorMessage = "Lĩnh vực nghiên cứu tối đa 250 ký tự")]
        [Display(Name = "Lĩnh vực nghiên cứu")]
        public string Field { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn loại hình đề tài")]
        [Display(Name = "Loại hình đề tài")]
        public ThesisType ThesisType { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái đề tài")]
        [Display(Name = "Trạng thái đề tài")]
        public ThesisStatus ThesisStatus { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày dự kiến hoàn thành")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Tóm tắt nghiên cứu")]
        public string? Abstract { get; set; }

        [Display(Name = "Họ tên học viên")]
        public string? StudentName { get; set; }

        [Display(Name = "Cờ tốt nghiệp")]
        public bool IsGraduationCapstone { get; set; } = true;
    }
}
