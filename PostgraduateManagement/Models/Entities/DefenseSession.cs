// File: Models/Entities/DefenseSession.cs
using System.ComponentModel.DataAnnotations;
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Models.Entities
{
    /// <summary>
    /// Thực thể quản lý phiên bảo vệ đề tài sau đại học.
    /// Bao gồm thời gian, địa điểm, trạng thái và kết quả đánh giá của hội đồng.
    /// </summary>
    public class DefenseSession
    {
        /// <summary>
        /// Khóa chính định danh phiên bảo vệ
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Khóa ngoại tham chiếu đề tài được bảo vệ
        /// </summary>
        public int ThesisId { get; set; }

        /// <summary>
        /// Đối tượng đề tài được bảo vệ
        /// </summary>
        public virtual Thesis? Thesis { get; set; }

        /// <summary>
        /// Thời gian dự kiến hoặc thực tế tổ chức phiên bảo vệ
        /// </summary>
        public DateTime ScheduledAt { get; set; }

        /// <summary>
        /// Địa điểm tổ chức (Phòng họp, hội trường hoặc liên kết trực tuyến)
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái phiên bảo vệ (Scheduled, InProgress, Defended, Cancelled)
        /// </summary>
        [Required]
        public DefenseStatus Status { get; set; } = DefenseStatus.Scheduled;

        /// <summary>
        /// Ghi chú của hội đồng hoặc ban thư ký phiên bảo vệ
        /// </summary>
        [StringLength(1000)]
        public string? CommitteeNotes { get; set; }

        /// <summary>
        /// Kết quả và biên bản đánh giá của phiên bảo vệ (quan hệ 1-1)
        /// </summary>
        public virtual DefenseResult? DefenseResult { get; set; }
    }
}
