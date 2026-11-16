// File: Services/DTOs/UploadDocumentDto.cs
using System.ComponentModel.DataAnnotations;
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Services.DTOs
{
    /// <summary>
    /// Đối tượng truyền tải dữ liệu tải lên tài liệu số hóa nộp lưu chiểu.
    /// Giới hạn nghiêm ngặt trong 3 loại tài liệu bắt buộc của Module M05:
    /// Thuyết minh, Toàn văn, Biên bản sửa đổi.
    /// </summary>
    public class UploadDocumentDto
    {
        /// <summary>
        /// Mã định danh đề tài nộp tài liệu
        /// </summary>
        [Required(ErrorMessage = "Mã đề tài không được để trống")]
        public int ThesisId { get; set; }

        /// <summary>
        /// Loại tài liệu số hóa bắt buộc (ThuyetMinh = 1, ToanVan = 2, BienBanSuaDoi = 3)
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn loại tài liệu")]
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Tên tệp tin gốc khi tải lên
        /// </summary>
        [Required(ErrorMessage = "Tên tệp không được để trống")]
        [StringLength(255, ErrorMessage = "Tên tệp tối đa 255 ký tự")]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Đường dẫn lưu trữ tệp tin trên hệ thống
        /// </summary>
        [Required(ErrorMessage = "Đường dẫn tệp không được để trống")]
        [StringLength(500, ErrorMessage = "Đường dẫn tệp tối đa 500 ký tự")]
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Mã băm SHA-256 xác thực tính toàn vẹn của tệp tin
        /// </summary>
        [StringLength(64, ErrorMessage = "Mã SHA-256 tối đa 64 ký tự")]
        public string? Sha256Hash { get; set; }
    }
}
