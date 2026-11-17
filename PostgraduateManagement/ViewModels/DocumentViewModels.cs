// File: ViewModels/DocumentViewModels.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Services.DTOs;

namespace PostgraduateManagement.ViewModels
{
    /// <summary>
    /// ViewModel chi tiết một tài liệu số hóa nộp lưu chiểu.
    /// Giới hạn nghiêm ngặt theo 3 loại tài liệu bắt buộc của Module M05:
    /// Thuyết minh đề tài, Toàn văn luận văn/luận án, Biên bản sửa đổi sau bảo vệ.
    /// </summary>
    public class DocumentItemViewModel
    {
        public int Id { get; set; }
        public int ThesisId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string DocumentTypeName => DocumentType switch
        {
            DocumentType.ThuyetMinh => "Thuyết minh đề tài",
            DocumentType.ToanVan => "Toàn văn luận văn / luận án",
            DocumentType.BienBanSuaDoi => "Biên bản sửa đổi sau bảo vệ",
            _ => DocumentType.ToString()
        };
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? Sha256Hash { get; set; }
        public int Version { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? ConfirmedBy { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public bool IsConfirmed => !string.IsNullOrEmpty(ConfirmedBy);
    }

    /// <summary>
    /// ViewModel cho màn hình quản lý nộp tài liệu số hóa (`Views/Document/Index.cshtml`).
    /// Bao gồm: thông tin đề tài, danh sách tài liệu đã nộp, và dashboard tiến độ số hóa 3 loại văn bản bắt buộc.
    /// </summary>
    public class DocumentIndexViewModel
    {
        public int ThesisId { get; set; }
        public string ThesisCode { get; set; } = string.Empty;
        public string ThesisTitle { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;

        /// <summary>
        /// Danh sách đề tài để chuyển đổi chọn đề tài xem tài liệu
        /// </summary>
        public List<SelectListItem> AvailableTheses { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Danh sách toàn bộ tài liệu số hóa đã nộp
        /// </summary>
        public List<DocumentItemViewModel> Documents { get; set; } = new List<DocumentItemViewModel>();

        /// <summary>
        /// Báo cáo tiến độ số hóa tài liệu nghiêm ngặt trong phạm vi 3 loại văn bản bắt buộc của Module M05
        /// (kèm thông điệp cảnh báo ranh giới quy chuẩn đào tạo).
        /// </summary>
        public DigitizationProgressDto? DigitizationProgress { get; set; }

        /// <summary>
        /// Form nộp tài liệu nhanh gắn kèm
        /// </summary>
        public UploadDocumentViewModel UploadForm { get; set; } = new UploadDocumentViewModel();
    }

    /// <summary>
    /// ViewModel tải lên tài liệu số hóa nộp lưu chiểu.
    /// </summary>
    public class UploadDocumentViewModel
    {
        [Required]
        public int ThesisId { get; set; }

        public string? ThesisCode { get; set; }
        public string? ThesisTitle { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại tài liệu số hóa bắt buộc")]
        [Display(Name = "Loại tài liệu bắt buộc")]
        public DocumentType DocumentType { get; set; } = DocumentType.ThuyetMinh;

        [Required(ErrorMessage = "Vui lòng chọn tệp tin cần tải lên")]
        [Display(Name = "Tệp tin đính kèm (PDF, DOCX)")]
        public IFormFile? File { get; set; }

        [Display(Name = "Ghi chú tài liệu")]
        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// ViewModel báo cáo tiến độ số hóa tài liệu cho đề tài sau đại học.
    /// </summary>
    public class DigitizationReportViewModel
    {
        public int ThesisId { get; set; }
        public string ThesisCode { get; set; } = string.Empty;
        public string ThesisTitle { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public DigitizationProgressDto Progress { get; set; } = new DigitizationProgressDto();
    }
}
