// File: Services/DTOs/ReviewerSuggestionDto.cs
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Services.DTOs
{
    /// <summary>
    /// Đối tượng truyền tải kết quả gợi ý chuyên gia phản biện của thuật toán AI heuristic.
    /// Mang tính chất tham khảo độc lập (Advisory Only), không tự động gán vào cơ sở dữ liệu.
    /// </summary>
    public class ReviewerSuggestionDto
    {
        /// <summary>
        /// Mã định danh chuyên gia
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Họ và tên chuyên gia
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Mã chuyên gia dùng chung toàn hệ thống
        /// </summary>
        public string? SharedPersonCode { get; set; }

        /// <summary>
        /// Học hàm, học vị cao nhất của chuyên gia (TS, PGS, GS)
        /// </summary>
        public AcademicDegree AcademicDegree { get; set; }

        /// <summary>
        /// Tên học hàm học vị viết tắt (TS., PGS.TS., GS.TS.)
        /// </summary>
        public string DegreeTitle => AcademicDegree switch
        {
            AcademicDegree.TS => "TS.",
            AcademicDegree.PGS => "PGS.TS.",
            AcademicDegree.GS => "GS.TS.",
            _ => AcademicDegree.ToString()
        };

        /// <summary>
        /// Lĩnh vực nghiên cứu chuyên sâu của chuyên gia
        /// </summary>
        public string ResearchField { get; set; } = string.Empty;

        /// <summary>
        /// Chuyên gia ngoài trường (true) vs cán bộ cơ hữu trong trường (false)
        /// </summary>
        public bool IsExternal { get; set; }

        /// <summary>
        /// Cơ quan / Đơn vị công tác
        /// </summary>
        public string? Workplace { get; set; }

        /// <summary>
        /// Thư điện tử
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Tổng điểm phù hợp theo thuật toán gợi ý (tối đa 100 điểm / 100%)
        /// </summary>
        public int MatchScore { get; set; }

        /// <summary>
        /// Điểm số trùng khớp từ khóa lĩnh vực nghiên cứu (0 - 50 điểm)
        /// </summary>
        public int FieldScore { get; set; }

        /// <summary>
        /// Điểm số học hàm, học vị (GS = 30đ, PGS = 20đ, TS = 10đ)
        /// </summary>
        public int DegreeScore { get; set; }

        /// <summary>
        /// Điểm cộng khuyến khích chuyên gia ngoài trường tham gia phản biện độc lập (+10đ nếu IsExternal)
        /// </summary>
        public int ExternalScore { get; set; }

        /// <summary>
        /// Giải trình tường minh lý do gợi ý và cấu phần điểm số
        /// Ví dụ: "Trùng khớp lĩnh vực 'Trí tuệ nhân tạo' (50đ) + Học hàm GS (30đ) + Chuyên gia ngoài trường (10đ) => Điểm phù hợp: 90%"
        /// </summary>
        public string RecommendationReason { get; set; } = string.Empty;

        /// <summary>
        /// Cờ chỉ định kết quả hoàn toàn mang tính chất tham khảo độc lập
        /// </summary>
        public bool IsAdvisoryOnly { get; set; } = true;

        /// <summary>
        /// Trạng thái gợi ý chuyên gia phản biện của AI ("DeXuat" - chỉ là đề xuất, không tạo phân công thực)
        /// </summary>
        public string SuggestionState { get; set; } = "DeXuat";

        /// <summary>
        /// Lưu ý về tính chất tham khảo của gợi ý
        /// </summary>
        public string AdvisoryNote { get; set; } = "Kết quả gợi ý phản biện AI mang tính chất khuyến nghị tham khảo độc lập, hỗ trợ cán bộ quản lý ra quyết định. Không tự động thực hiện phân công trong hệ thống.";
    }
}
