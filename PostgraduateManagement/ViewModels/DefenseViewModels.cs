// File: ViewModels/DefenseViewModels.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.ViewModels
{
    /// <summary>
    /// ViewModel chi tiết kết quả đánh giá bảo vệ của hội đồng.
    /// </summary>
    public class DefenseResultItemViewModel
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public decimal Score { get; set; }
        public bool Pass { get; set; }
        public string Comments { get; set; } = string.Empty;
        public string RecordedBy { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; }
    }

    /// <summary>
    /// ViewModel biểu diễn một phiên bảo vệ đề tài sau đại học trong danh sách hiển thị.
    /// </summary>
    public class DefenseSessionItemViewModel
    {
        public int Id { get; set; }
        public int ThesisId { get; set; }
        public string ThesisCode { get; set; } = string.Empty;
        public string ThesisTitle { get; set; } = string.Empty;
        public ThesisType ThesisType { get; set; }
        public string ThesisTypeName => ThesisType switch
        {
            ThesisType.DeAn => "Đề án thạc sĩ",
            ThesisType.LuanVan => "Luận văn thạc sĩ",
            ThesisType.LuanAn => "Luận án tiến sĩ",
            _ => ThesisType.ToString()
        };
        public string Field { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public string Location { get; set; } = string.Empty;
        public DefenseStatus Status { get; set; }
        public string StatusName => Status switch
        {
            DefenseStatus.Scheduled => "Đã lên lịch",
            DefenseStatus.InProgress => "Đang diễn ra",
            DefenseStatus.Defended => "Đã bảo vệ",
            DefenseStatus.Cancelled => "Đã hủy",
            _ => Status.ToString()
        };
        public string? CommitteeNotes { get; set; }
        public DefenseResultItemViewModel? Result { get; set; }

        public bool CanRecordResult => Status == DefenseStatus.Scheduled || Status == DefenseStatus.InProgress;
        public bool IsPast => ScheduledAt < DateTime.UtcNow;
    }

    /// <summary>
    /// ViewModel cho màn hình danh sách các phiên bảo vệ (`Views/Defense/Index.cshtml`).
    /// </summary>
    public class DefenseIndexViewModel
    {
        /// <summary>
        /// Danh sách các phiên bảo vệ
        /// </summary>
        public List<DefenseSessionItemViewModel> Sessions { get; set; } = new List<DefenseSessionItemViewModel>();

        /// <summary>
        /// Từ khóa tìm kiếm theo tên đề tài, mã đề tài, học viên, địa điểm
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Lọc theo trạng thái phiên bảo vệ
        /// </summary>
        public DefenseStatus? Status { get; set; }

        /// <summary>
        /// Tổng số phiên bảo vệ thỏa điều kiện
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Danh sách tùy chọn trạng thái phục vụ dropdown lọc
        /// </summary>
        public List<SelectListItem> StatusOptions { get; set; } = new List<SelectListItem>();
    }

    /// <summary>
    /// ViewModel lập lịch phiên bảo vệ tốt nghiệp sau đại học.
    /// Có kiểm tra nghiêm ngặt chỉ cho phép đề tài tốt nghiệp (chặn đề tài NCKH).
    /// </summary>
    public class ScheduleDefenseViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn đề tài cần bảo vệ")]
        [Display(Name = "Đề tài sau đại học")]
        public int ThesisId { get; set; }

        public string? ThesisTitle { get; set; }
        public string? ThesisCode { get; set; }
        public string? StudentName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thời gian tổ chức bảo vệ")]
        [Display(Name = "Thời gian bảo vệ")]
        public DateTime ScheduledAt { get; set; } = DateTime.UtcNow.Date.AddDays(7).AddHours(9);

        [Required(ErrorMessage = "Vui lòng nhập địa điểm bảo vệ")]
        [StringLength(250, ErrorMessage = "Địa điểm tối đa 250 ký tự")]
        [Display(Name = "Địa điểm / Phòng bảo vệ")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Ghi chú thành phần hội đồng")]
        [StringLength(1000, ErrorMessage = "Ghi chú tối đa 1000 ký tự")]
        public string? CommitteeNotes { get; set; }

        /// <summary>
        /// Danh sách đề tài đủ điều kiện để chọn bảo vệ
        /// </summary>
        public List<SelectListItem> AvailableTheses { get; set; } = new List<SelectListItem>();
    }

    /// <summary>
    /// ViewModel ghi nhận kết quả đánh giá điểm số và nhận xét của hội đồng bảo vệ.
    /// </summary>
    public class RecordDefenseResultViewModel
    {
        [Required]
        public int SessionId { get; set; }

        public int ThesisId { get; set; }
        public string? ThesisCode { get; set; }
        public string? ThesisTitle { get; set; }
        public string? StudentName { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string? Location { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập điểm số đánh giá")]
        [Range(0.0, 10.0, ErrorMessage = "Điểm số đánh giá phải nằm trong thang điểm từ 0.0 đến 10.0")]
        [Display(Name = "Điểm số đánh giá (thang điểm 10)")]
        public decimal Score { get; set; } = 8.0m;

        [Display(Name = "Đánh giá Đạt yêu cầu bảo vệ")]
        public bool Pass { get; set; } = true;

        [Required(ErrorMessage = "Vui lòng nhập nhận xét và kết luận của hội đồng")]
        [StringLength(2000, ErrorMessage = "Nhận xét tối đa 2000 ký tự")]
        [Display(Name = "Nhận xét và kết luận của Hội đồng")]
        public string Comments { get; set; } = string.Empty;
    }
}
