// File: Models/Entities/ProgressReport.cs
using System.ComponentModel.DataAnnotations;

namespace PostgraduateManagement.Models.Entities
{
    /// <summary>
    /// Báo cáo tiến độ thực hiện đề tài (bước "theo dõi thực hiện" trong quy trình M05).
    /// Học viên hoặc cán bộ báo cáo phần việc đã làm; người hướng dẫn ghi nhận xét phản hồi.
    /// </summary>
    public class ProgressReport
    {
        public int Id { get; set; }

        public int ThesisId { get; set; }
        public virtual Thesis? Thesis { get; set; }

        /// <summary>
        /// Nội dung công việc đã thực hiện trong kỳ báo cáo
        /// </summary>
        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Mức độ hoàn thành ước tính (0 - 100%)
        /// </summary>
        [Range(0, 100)]
        public int PercentComplete { get; set; }

        /// <summary>
        /// Tài khoản gửi báo cáo
        /// </summary>
        [StringLength(100)]
        public string SubmittedBy { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Nhận xét phản hồi của người hướng dẫn (null nếu chưa phản hồi)
        /// </summary>
        [StringLength(2000)]
        public string? SupervisorFeedback { get; set; }

        [StringLength(100)]
        public string? FeedbackBy { get; set; }

        public DateTime? FeedbackAt { get; set; }
    }
}
