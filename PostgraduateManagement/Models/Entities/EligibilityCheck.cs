// File: Models/Entities/EligibilityCheck.cs
using System.ComponentModel.DataAnnotations;
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Models.Entities
{
    /// <summary>
    /// Thực thể lưu vết lịch sử thẩm định điều kiện chuyên môn theo quy chế đào tạo.
    /// Đối soát học hàm, học vị, lĩnh vực nghiên cứu và tỷ lệ chuyên gia ngoài trường.
    /// </summary>
    public class EligibilityCheck
    {
        /// <summary>
        /// Khóa chính định danh lần kiểm tra
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Khóa ngoại tham chiếu đề tài
        /// </summary>
        public int ThesisId { get; set; }

        /// <summary>
        /// Đối tượng đề tài
        /// </summary>
        public virtual Thesis? Thesis { get; set; }

        /// <summary>
        /// Khóa ngoại tham chiếu chuyên gia được thẩm định
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Đối tượng chuyên gia được thẩm định
        /// </summary>
        public virtual AcademicPerson? Person { get; set; }

        /// <summary>
        /// Vai trò chuyên môn được kiểm tra (Người hướng dẫn, Phản biện, Hội đồng)
        /// </summary>
        [Required]
        public AssignmentRole Role { get; set; }

        /// <summary>
        /// Kết quả thẩm định: Eligible (Đạt), Ineligible (Không đạt), Conditional (Có điều kiện)
        /// </summary>
        [Required]
        public CheckResult Result { get; set; }

        /// <summary>
        /// Căn cứ pháp lý và lý giải kết luận thẩm định
        /// </summary>
        [Required]
        [StringLength(1000)]
        public string Basis { get; set; } = string.Empty;

        /// <summary>
        /// Phiên bản quy chế đào tạo áp dụng (ví dụ: Thông tư 18/2021/TT-BGDĐT)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string RegulationVersion { get; set; } = string.Empty;

        /// <summary>
        /// Thời điểm tiến hành thẩm định
        /// </summary>
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }
}
