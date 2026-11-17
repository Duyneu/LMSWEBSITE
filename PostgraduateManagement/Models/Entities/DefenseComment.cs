// File: Models/Entities/DefenseComment.cs
using System.ComponentModel.DataAnnotations;

namespace PostgraduateManagement.Models.Entities
{
    /// <summary>
    /// Nhận xét và điểm của từng thành viên (phản biện, thành viên hội đồng) trong một phiên bảo vệ.
    /// Tách riêng khỏi kết luận chung của hội đồng (DefenseResult). Mỗi người một nhận xét cho mỗi phiên.
    /// </summary>
    public class DefenseComment
    {
        public int Id { get; set; }

        /// <summary>
        /// Phiên bảo vệ được nhận xét
        /// </summary>
        public int SessionId { get; set; }
        public virtual DefenseSession? Session { get; set; }

        /// <summary>
        /// Chuyên gia đưa ra nhận xét (phải được phân công phản biện hoặc hội đồng cho đề tài)
        /// </summary>
        public int PersonId { get; set; }
        public virtual AcademicPerson? Person { get; set; }

        /// <summary>
        /// Nội dung nhận xét
        /// </summary>
        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Điểm của thành viên (thang 10, có thể bỏ trống)
        /// </summary>
        [Range(0.0, 10.0)]
        public decimal? Score { get; set; }

        /// <summary>
        /// Tài khoản ghi nhận nhận xét (thư ký hoặc chính thành viên)
        /// </summary>
        [StringLength(100)]
        public string RecordedBy { get; set; } = string.Empty;

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
