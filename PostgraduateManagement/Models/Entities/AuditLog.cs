// File: Models/Entities/AuditLog.cs
using System.ComponentModel.DataAnnotations;

namespace PostgraduateManagement.Models.Entities
{
    /// <summary>
    /// Thực thể lưu trữ vết kiểm toán (Audit Log) các thao tác trọng yếu trên hệ thống sau đại học.
    /// Giúp truy vết sự kiện, bảo mật dữ liệu và đáp ứng yêu cầu thanh tra kiểm định.
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// Khóa chính định danh bản ghi nhật ký kiểm toán
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Mã định danh tài khoản người dùng thực hiện thao tác (nếu có)
        /// </summary>
        [StringLength(450)]
        public string? UserId { get; set; }

        /// <summary>
        /// Tên đăng nhập người dùng thực hiện thao tác
        /// </summary>
        [StringLength(256)]
        public string? UserName { get; set; }

        /// <summary>
        /// Hành vi / Thao tác thực hiện (ví dụ: Create, Update, Delete, DefenseResultApprove, DocumentUpload...)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Tên thực thể / Phân hệ tác động (ví dụ: Thesis, DefenseSession, SubmittedDocument...)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty;

        /// <summary>
        /// Khóa chính hoặc mã định danh của thực thể bị tác động
        /// </summary>
        [StringLength(100)]
        public string? EntityId { get; set; }

        /// <summary>
        /// Chi tiết nội dung thay đổi hoặc nhật ký tham số thao tác
        /// </summary>
        [StringLength(4000)]
        public string? Details { get; set; }

        /// <summary>
        /// Thời điểm ghi nhận thao tác kiểm toán (UTC)
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
