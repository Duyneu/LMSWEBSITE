// File: Models/Entities/ThesisAssignment.cs
using System.ComponentModel.DataAnnotations;
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Models.Entities
{
    /// <summary>
    /// Thực thể phân công vai trò chuyên gia (hướng dẫn, phản biện, hội đồng) cho đề tài.
    /// Lưu vết độc lập trạng thái chuyên gia ngoài trường tại thời điểm phân công.
    /// </summary>
    public class ThesisAssignment
    {
        /// <summary>
        /// Khóa chính định danh phân công
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Khóa ngoại tham chiếu đề tài
        /// </summary>
        public int ThesisId { get; set; }

        /// <summary>
        /// Đối tượng đề tài được phân công
        /// </summary>
        public virtual Thesis? Thesis { get; set; }

        /// <summary>
        /// Khóa ngoại tham chiếu chuyên gia / cán bộ
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Đối tượng chuyên gia / cán bộ được phân công
        /// </summary>
        public virtual AcademicPerson? Person { get; set; }

        /// <summary>
        /// Vai trò được phân công (Supervisor, Reviewer, Committee)
        /// </summary>
        [Required]
        public AssignmentRole Role { get; set; }

        /// <summary>
        /// Đánh dấu chuyên gia ngoài trường tại thời điểm phân công
        /// </summary>
        public bool IsExternal { get; set; }

        /// <summary>
        /// Thời điểm ban hành hoặc phê duyệt phân công
        /// </summary>
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ghi chú chi tiết hoặc số quyết định phân công
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
