// File: Services/DTOs/ScheduleDefenseDto.cs
using System.ComponentModel.DataAnnotations;

namespace PostgraduateManagement.Services.DTOs
{
    /// <summary>
    /// Đối tượng truyền tải dữ liệu lập lịch phiên bảo vệ đề tài sau đại học.
    /// </summary>
    public class ScheduleDefenseDto
    {
        /// <summary>
        /// Mã định danh đề tài cần bảo vệ
        /// </summary>
        [Required(ErrorMessage = "Mã đề tài không được để trống")]
        public int ThesisId { get; set; }

        /// <summary>
        /// Thời gian dự kiến tổ chức phiên bảo vệ
        /// </summary>
        [Required(ErrorMessage = "Thời gian bảo vệ không được để trống")]
        public DateTime ScheduledAt { get; set; }

        /// <summary>
        /// Địa điểm tổ chức (Phòng họp, hội trường hoặc link trực tuyến)
        /// </summary>
        [Required(ErrorMessage = "Địa điểm tổ chức không được để trống")]
        [StringLength(200, ErrorMessage = "Địa điểm tổ chức tối đa 200 ký tự")]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú của hội đồng hoặc hướng dẫn chuẩn bị
        /// </summary>
        [StringLength(1000, ErrorMessage = "Ghi chú tối đa 1000 ký tự")]
        public string? CommitteeNotes { get; set; }
    }
}
