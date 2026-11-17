// File: Models/Entities/SubmittedDocument.cs
using System.ComponentModel.DataAnnotations;
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Models.Entities
{
    /// <summary>
    /// Thực thể quản lý tài liệu số hóa nộp lưu chiểu của đề tài sau đại học.
    /// Giới hạn nghiêm ngặt trong danh mục 3 loại tài liệu bắt buộc của Module M05:
    /// Thuyết minh, Toàn văn, Biên bản sửa đổi.
    /// </summary>
    public class SubmittedDocument
    {
        /// <summary>
        /// Khóa chính định danh tài liệu nộp
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
        /// Loại tài liệu số hóa (ThuyetMinh, ToanVan, BienBanSuaDoi)
        /// </summary>
        [Required]
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Tên tệp tin gốc khi tải lên
        /// </summary>
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Đường dẫn lưu trữ tệp tin trên hệ thống máy chủ hoặc bộ lưu trữ
        /// </summary>
        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Mã băm SHA-256 xác thực tính toàn vẹn và chống giả mạo của tệp tin tài liệu số hóa
        /// </summary>
        [StringLength(64)]
        public string? Sha256Hash { get; set; }

        /// <summary>
        /// Phiên bản của tài liệu (1, 2, 3...)
        /// Tự động tăng khi học viên nộp lại phiên bản sửa đổi cùng loại tài liệu.
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Thời điểm tải tài liệu lên hệ thống
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Cán bộ hoặc hội đồng thẩm định xác nhận tài liệu số hóa hợp lệ
        /// </summary>
        [StringLength(100)]
        public string? ConfirmedBy { get; set; }

        /// <summary>
        /// Thời điểm xác nhận tài liệu (null nếu chưa xác nhận)
        /// </summary>
        public DateTime? ConfirmedAt { get; set; }
    }
}
