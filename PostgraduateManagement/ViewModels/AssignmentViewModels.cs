// File: ViewModels/AssignmentViewModels.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.ViewModels
{
    /// <summary>
    /// ViewModel chi tiết một phân công chuyên gia cho đề tài sau đại học.
    /// </summary>
    public class AssignmentItemViewModel
    {
        public int Id { get; set; }
        public int ThesisId { get; set; }
        public int PersonId { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public string? SharedPersonCode { get; set; }
        public AcademicDegree AcademicDegree { get; set; }
        public string AcademicDegreeName => AcademicDegree switch
        {
            AcademicDegree.TS => "Tiến sĩ (TS)",
            AcademicDegree.PGS => "Phó Giáo sư (PGS.TS)",
            AcademicDegree.GS => "Giáo sư (GS.TS)",
            _ => AcademicDegree.ToString()
        };
        public string ResearchField { get; set; } = string.Empty;
        public string? Workplace { get; set; }
        public AssignmentRole Role { get; set; }
        public CommitteePosition? CommitteePosition { get; set; }
        public string RoleName => Role switch
        {
            AssignmentRole.Supervisor => "Người hướng dẫn",
            AssignmentRole.Reviewer => "Người phản biện",
            AssignmentRole.Committee => CommitteePosition switch
            {
                Models.Enums.CommitteePosition.Chair => "Chủ tịch hội đồng",
                Models.Enums.CommitteePosition.Secretary => "Thư ký hội đồng",
                _ => "Ủy viên hội đồng"
            },
            _ => Role.ToString()
        };
        public bool IsExternal { get; set; }
        public DateTime AssignedAt { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// ViewModel thông tin ứng viên chuyên gia khoa học phục vụ danh sách chọn phân công.
    /// </summary>
    public class CandidatePersonViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? SharedPersonCode { get; set; }
        public AcademicDegree AcademicDegree { get; set; }
        public string AcademicDegreeName => AcademicDegree switch
        {
            AcademicDegree.TS => "Tiến sĩ",
            AcademicDegree.PGS => "Phó Giáo sư",
            AcademicDegree.GS => "Giáo sư",
            _ => AcademicDegree.ToString()
        };
        public string ResearchField { get; set; } = string.Empty;
        public string? Workplace { get; set; }
        public bool IsExternal { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// ViewModel kết quả gợi ý chuyên gia phản biện từ thuật toán AI Heuristic.
    /// Chỉ mang tính chất tham khảo (Advisory Only), không tự động phân công.
    /// </summary>
    public class AiSuggestionItemViewModel
    {
        public int PersonId { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public string? SharedPersonCode { get; set; }
        public AcademicDegree AcademicDegree { get; set; }
        public string AcademicDegreeName => AcademicDegree switch
        {
            AcademicDegree.TS => "Tiến sĩ",
            AcademicDegree.PGS => "Phó Giáo sư",
            AcademicDegree.GS => "Giáo sư",
            _ => AcademicDegree.ToString()
        };
        public string ResearchField { get; set; } = string.Empty;
        public string? Workplace { get; set; }
        public bool IsExternal { get; set; }
        public int MatchScore { get; set; }
        public int FieldScore { get; set; }
        public int DegreeScore { get; set; }
        public int ExternalScore { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public bool IsAdvisoryOnly { get; set; } = true;
        public string SuggestionState { get; set; } = "DeXuat";
        public string RecommendationNote { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel cho màn hình trung tâm phân công chuyên gia (`Views/Assignment/Index.cshtml`).
    /// Bao gồm: đề tài được chọn, danh sách phân công hiện tại, danh sách ứng viên chuyên gia,
    /// và danh sách gợi ý phản biện độc lập từ AI.
    /// </summary>
    public class AssignmentIndexViewModel
    {
        public int? SelectedThesisId { get; set; }
        public string? ThesisCode { get; set; }
        public string? ThesisTitle { get; set; }
        public ThesisType? ThesisType { get; set; }
        public string? ThesisTypeName { get; set; }
        public string? ThesisField { get; set; }
        public string? StudentName { get; set; }
        public string? StudentCode { get; set; }

        /// <summary>
        /// Danh sách đề tài để chuyển đổi nhanh trên giao diện
        /// </summary>
        public List<SelectListItem> AvailableTheses { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Danh sách các chuyên gia đã được phân công cho đề tài hiện tại
        /// </summary>
        public List<AssignmentItemViewModel> CurrentAssignments { get; set; } = new List<AssignmentItemViewModel>();

        /// <summary>
        /// Danh sách tất cả chuyên gia khả dụng trong hệ thống
        /// </summary>
        public List<CandidatePersonViewModel> CandidatePersons { get; set; } = new List<CandidatePersonViewModel>();

        /// <summary>
        /// Danh sách khuyến nghị chuyên gia phản biện từ AI Heuristic (Advisory Only)
        /// </summary>
        public List<AiSuggestionItemViewModel> AiReviewerSuggestions { get; set; } = new List<AiSuggestionItemViewModel>();

        /// <summary>
        /// Model gắn sẵn phục vụ form phân công nhanh
        /// </summary>
        public AssignPersonViewModel NewAssignment { get; set; } = new AssignPersonViewModel();
    }

    /// <summary>
    /// ViewModel gửi dữ liệu phân công chuyên gia khoa học.
    /// </summary>
    public class AssignPersonViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn đề tài cần phân công")]
        [Display(Name = "Mã đề tài")]
        public int ThesisId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chuyên gia")]
        [Display(Name = "Chuyên gia / Cán bộ")]
        public int PersonId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò phân công")]
        [Display(Name = "Vai trò chuyên môn")]
        public AssignmentRole Role { get; set; } = AssignmentRole.Supervisor;

        [Display(Name = "Chức danh trong hội đồng")]
        public CommitteePosition? CommitteePosition { get; set; }

        [Display(Name = "Chuyên gia ngoài trường")]
        public bool IsExternal { get; set; }

        [Display(Name = "Ghi chú phân công")]
        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        public string? Notes { get; set; }
    }
}
