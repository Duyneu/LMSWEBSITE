// File: Models/Entities/DefenseResult.cs
using System.ComponentModel.DataAnnotations;

namespace PostgraduateManagement.Models.Entities
{
    /// <summary>
    /// Thực thể ghi nhận kết quả và biên bản đánh giá của phiên bảo vệ đề tài.
    /// Bao gồm điểm số trung bình, đánh giá Đạt/Không đạt và nhận xét của hội đồng.
    /// </summary>
    public class DefenseResult
    {
        /// <summary>
        /// Khóa chính định danh kết quả bảo vệ
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Khóa ngoại tham chiếu phiên bảo vệ tương ứng (quan hệ 1-1)
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        /// Đối tượng phiên bảo vệ tương ứng
        /// </summary>
        public virtual DefenseSession? Session { get; set; }

        /// <summary>
        /// Nhận xét, đánh giá chi tiết và kết luận của hội đồng
        /// </summary>
        [Required]
        [StringLength(2000)]
        public string Comments { get; set; } = string.Empty;

        /// <summary>
        /// Điểm số đánh giá trung bình của hội đồng (thang điểm 10.0)
        /// </summary>
        [Range(0.0, 10.0)]
        public decimal Score { get; set; }

        /// <summary>
        /// Kết luận chung cuộc: Đạt (true) hoặc Không đạt (false)
        /// </summary>
        public bool Pass { get; set; }

        /// <summary>
        /// Họ tên hoặc định danh của thư ký / cán bộ ghi nhận kết quả
        /// </summary>
        [Required]
        [StringLength(100)]
        public string RecordedBy { get; set; } = string.Empty;

        /// <summary>
        /// Thời điểm lập biên bản và ghi nhận kết quả bảo vệ
        /// </summary>
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
