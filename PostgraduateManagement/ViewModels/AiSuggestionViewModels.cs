// File: ViewModels/AiSuggestionViewModels.cs
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Services.DTOs;

namespace PostgraduateManagement.ViewModels
{
    /// <summary>
    /// ViewModel chi tiết cho thẻ ứng viên chuyên gia phản biện được AI gợi ý.
    /// Mang tính chất khuyến nghị tham khảo độc lập (Advisory Only).
    /// </summary>
    public class ReviewerCandidateCardViewModel
    {
        public int PersonId { get; set; }
        public string SharedPersonCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
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
        public string RecommendationNote { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel tổng thể cho kết quả phân tích và gợi ý phản biện AI độc lập cho đề tài.
    /// </summary>
    public class AiReviewerSuggestionViewModel
    {
        public int ThesisId { get; set; }
        public string ThesisCode { get; set; } = string.Empty;
        public string ThesisTitle { get; set; } = string.Empty;
        public string ThesisField { get; set; } = string.Empty;
        public List<ReviewerCandidateCardViewModel> Candidates { get; set; } = new List<ReviewerCandidateCardViewModel>();
        public bool IsAdvisoryOnly { get; set; } = true;
        public string DisclaimerText { get; set; } = "Gợi ý mang tính chất tham khảo độc lập, không tự động phân công vào cơ sở dữ liệu.";
    }
}
