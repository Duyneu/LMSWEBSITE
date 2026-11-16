// File: Services/DTOs/DefenseResultDto.cs
namespace PostgraduateManagement.Services.DTOs
{
    /// <summary>
    /// Đối tượng truyền tải kết quả đánh giá của phiên bảo vệ đề tài.
    /// </summary>
    public class DefenseResultDto
    {
        /// <summary>
        /// Mã định danh kết quả bảo vệ
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Mã định danh phiên bảo vệ tương ứng
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        /// Nhận xét, đánh giá chi tiết và kết luận của hội đồng
        /// </summary>
        public string Comments { get; set; } = string.Empty;

        /// <summary>
        /// Điểm số đánh giá trung bình của hội đồng (thang điểm 10.0)
        /// </summary>
        public decimal Score { get; set; }

        /// <summary>
        /// Kết luận: Đạt (true) hoặc Không đạt (false)
        /// </summary>
        public bool Pass { get; set; }

        /// <summary>
        /// Chuỗi hiển thị kết luận đạt / không đạt
        /// </summary>
        public string ResultSummary => Pass ? "Đạt yêu cầu bảo vệ" : "Không đạt yêu cầu";

        /// <summary>
        /// Thư ký hoặc cán bộ ghi nhận kết quả
        /// </summary>
        public string RecordedBy { get; set; } = string.Empty;

        /// <summary>
        /// Thời điểm lập biên bản và ghi nhận kết quả
        /// </summary>
        public DateTime RecordedAt { get; set; }
    }
}
