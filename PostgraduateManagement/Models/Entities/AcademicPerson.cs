// File: Models/Entities/AcademicPerson.cs
using System.ComponentModel.DataAnnotations;
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Models.Entities
{
    /// <summary>
    /// Thực thể quản lý cán bộ, giảng viên, nhà khoa học và chuyên gia.
    /// Bao gồm cán bộ cơ hữu trong trường và chuyên gia ngoài trường (IsExternal).
    /// Phục vụ thẩm định điều kiện quy chế và thuật toán gợi ý phản biện AI.
    /// </summary>
    public class AcademicPerson
    {
        /// <summary>
        /// Khóa chính định danh chuyên gia / cán bộ
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Mã định danh chuyên gia / cán bộ dùng chung trong toàn hệ thống
        /// </summary>
        [StringLength(50)]
        public string? SharedPersonCode { get; set; }

        /// <summary>
        /// Họ và tên chuyên gia / nhà khoa học
        /// </summary>
        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Học hàm và học vị khoa học cao nhất (TS, PGS, GS)
        /// </summary>
        [Required]
        public AcademicDegree AcademicDegree { get; set; }

        /// <summary>
        /// Lĩnh vực nghiên cứu chuyên sâu phục vụ đối sánh chuyên môn
        /// </summary>
        [Required]
        [StringLength(300)]
        public string ResearchField { get; set; } = string.Empty;

        /// <summary>
        /// Đánh dấu chuyên gia ngoài trường (true) hoặc cán bộ cơ hữu (false).
        /// Tiêu chí bắt buộc để thẩm định cơ cấu hội đồng và gợi ý phản biện độc lập.
        /// </summary>
        public bool IsExternal { get; set; }

        /// <summary>
        /// Địa chỉ thư điện tử
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại liên lạc
        /// </summary>
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Đơn vị công tác, viện nghiên cứu hoặc doanh nghiệp
        /// </summary>
        [StringLength(250)]
        public string? Workplace { get; set; }

        /// <summary>
        /// Danh sách các phân công chuyên môn (người hướng dẫn, phản biện, hội đồng)
        /// </summary>
        public virtual ICollection<ThesisAssignment> Assignments { get; set; } = new List<ThesisAssignment>();

        /// <summary>
        /// Lịch sử các lần thẩm định điều kiện chuyên môn
        /// </summary>
        public virtual ICollection<EligibilityCheck> EligibilityChecks { get; set; } = new List<EligibilityCheck>();
    }
}
