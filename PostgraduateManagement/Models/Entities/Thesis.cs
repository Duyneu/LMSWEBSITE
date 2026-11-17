// File: Models/Entities/Thesis.cs
using System.ComponentModel.DataAnnotations;
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Models.Entities
{
    /// <summary>
    /// Thực thể quản lý đề tài tốt nghiệp sau đại học (Đề án, Luận văn, Luận án).
    /// Chứa cờ IsGraduationCapstone để phân biệt đề tài tốt nghiệp với đề tài NCKH.
    /// </summary>
    public class Thesis
    {
        /// <summary>
        /// Khóa chính định danh đề tài
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Mã định danh đề tài (ví dụ: LV-2024-001, LA-2023-012)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên đầy đủ của đề tài nghiên cứu
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Loại hình đề tài (Đề án, Luận văn, Luận án)
        /// </summary>
        [Required]
        public ThesisType ThesisType { get; set; }

        /// <summary>
        /// Lĩnh vực hoặc chuyên ngành nghiên cứu của đề tài
        /// </summary>
        [Required]
        [StringLength(250)]
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Ngày bắt đầu thực hiện đề tài
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày dự kiến hoàn thành đề tài
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Trạng thái vòng đời hiện tại của đề tài
        /// </summary>
        [Required]
        public ThesisStatus ThesisStatus { get; set; } = ThesisStatus.Draft;

        /// <summary>
        /// Khóa ngoại tham chiếu người học thực hiện đề tài
        /// </summary>
        public int StudentId { get; set; }

        /// <summary>
        /// Đối tượng người học sau đại học
        /// </summary>
        public virtual PostgraduateStudent? Student { get; set; }

        /// <summary>
        /// Cờ nghiệp vụ cốt lõi: Phân biệt đề tài tốt nghiệp sau đại học (true)
        /// với đề tài Nghiên cứu khoa học thường (false).
        /// Mặc định là true. Đề tài có giá trị false sẽ bị chặn tuyệt đối vào luồng bảo vệ M05.
        /// </summary>
        public bool IsGraduationCapstone { get; set; } = true;

        /// <summary>
        /// Tóm tắt nội dung và mục tiêu nghiên cứu của đề tài
        /// </summary>
        public string? Abstract { get; set; }

        /// <summary>
        /// Danh sách các phân công chuyên gia (người hướng dẫn, phản biện, hội đồng)
        /// </summary>
        public virtual ICollection<ThesisAssignment> Assignments { get; set; } = new List<ThesisAssignment>();

        /// <summary>
        /// Lịch sử các lần thẩm định điều kiện quy chế cho đề tài
        /// </summary>
        public virtual ICollection<EligibilityCheck> EligibilityChecks { get; set; } = new List<EligibilityCheck>();

        /// <summary>
        /// Danh sách các phiên bảo vệ được tổ chức cho đề tài
        /// </summary>
        public virtual ICollection<DefenseSession> DefenseSessions { get; set; } = new List<DefenseSession>();

        /// <summary>
        /// Danh mục các tài liệu số hóa đã nộp thuộc danh mục bắt buộc M05
        /// </summary>
        public virtual ICollection<SubmittedDocument> SubmittedDocuments { get; set; } = new List<SubmittedDocument>();

        /// <summary>
        /// Các báo cáo tiến độ thực hiện đề tài
        /// </summary>
        public virtual ICollection<ProgressReport> ProgressReports { get; set; } = new List<ProgressReport>();
    }
}
