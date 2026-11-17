// File: Services/DTOs/SubmittedDocumentDto.cs
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Services.DTOs
{
    /// <summary>
    /// Đối tượng truyền tải thông tin tài liệu số hóa đã nộp của đề tài.
    /// </summary>
    public class SubmittedDocumentDto
    {
        /// <summary>
        /// Mã định danh tài liệu đã nộp
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Mã định danh đề tài
        /// </summary>
        public int ThesisId { get; set; }

        /// <summary>
        /// Loại tài liệu số hóa
        /// </summary>
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Tên loại tài liệu tiếng Việt
        /// </summary>
        public string DocumentTypeName => DocumentType switch
        {
            DocumentType.ThuyetMinh => "Thuyết minh đề cương nghiên cứu",
            DocumentType.ToanVan => "Toàn văn luận văn / luận án / đề án",
            DocumentType.BienBanSuaDoi => "Biên bản giải trình sửa đổi sau bảo vệ",
            _ => DocumentType.ToString()
        };

        /// <summary>
        /// Tên tệp tin gốc
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Đường dẫn lưu trữ tệp tin
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Mã băm SHA-256 xác thực tính toàn vẹn của tệp tin
        /// </summary>
        public string? Sha256Hash { get; set; }

        /// <summary>
        /// Phiên bản của tài liệu (1, 2, 3...)
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Chuỗi hiển thị phiên bản (ví dụ: v1.0, v2.0)
        /// </summary>
        public string VersionLabel => $"v{Version}.0";

        /// <summary>
        /// Thời điểm tải tài liệu lên hệ thống
        /// </summary>
        public DateTime UploadedAt { get; set; }

        /// <summary>
        /// Người / Hội đồng thẩm định xác nhận tài liệu số hóa hợp lệ
        /// </summary>
        public string? ConfirmedBy { get; set; }

        /// <summary>
        /// Thời điểm xác nhận tài liệu (null nếu chưa xác nhận)
        /// </summary>
        public DateTime? ConfirmedAt { get; set; }

        /// <summary>
        /// Trạng thái đã được xác nhận hay chưa
        /// </summary>
        public bool IsConfirmed => !string.IsNullOrEmpty(ConfirmedBy);
    }
}
