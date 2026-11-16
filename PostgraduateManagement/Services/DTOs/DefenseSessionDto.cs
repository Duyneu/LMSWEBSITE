// File: Services/DTOs/DefenseSessionDto.cs
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Services.DTOs
{
    /// <summary>
    /// Đối tượng truyền tải thông tin phiên bảo vệ đề tài sau đại học.
    /// Bao gồm thông tin đề tài, học viên, lịch trình và kết quả đánh giá (nếu có).
    /// </summary>
    public class DefenseSessionDto
    {
        /// <summary>
        /// Mã định danh phiên bảo vệ
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Mã định danh đề tài
        /// </summary>
        public int ThesisId { get; set; }

        /// <summary>
        /// Mã số đề tài
        /// </summary>
        public string ThesisCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên đầy đủ của đề tài
        /// </summary>
        public string ThesisTitle { get; set; } = string.Empty;

        /// <summary>
        /// Loại hình đề tài
        /// </summary>
        public ThesisType ThesisType { get; set; }

        /// <summary>
        /// Lĩnh vực nghiên cứu
        /// </summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Họ tên học viên bảo vệ
        /// </summary>
        public string StudentName { get; set; } = string.Empty;

        /// <summary>
        /// Mã số học viên
        /// </summary>
        public string StudentCode { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian dự kiến hoặc thực tế tổ chức phiên bảo vệ
        /// </summary>
        public DateTime ScheduledAt { get; set; }

        /// <summary>
        /// Địa điểm tổ chức phiên bảo vệ
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái phiên bảo vệ (Scheduled, InProgress, Defended, Cancelled)
        /// </summary>
        public DefenseStatus Status { get; set; }

        /// <summary>
        /// Tên trạng thái hiển thị tiếng Việt
        /// </summary>
        public string StatusName => Status switch
        {
            DefenseStatus.Scheduled => "Đã lập lịch",
            DefenseStatus.InProgress => "Đang bảo vệ",
            DefenseStatus.Defended => "Đã bảo vệ xong",
            DefenseStatus.Cancelled => "Đã hủy",
            _ => Status.ToString()
        };

        /// <summary>
        /// Ghi chú của hội đồng hoặc hướng dẫn phiên bảo vệ
        /// </summary>
        public string? CommitteeNotes { get; set; }

        /// <summary>
        /// Kết quả đánh giá của hội đồng (nếu đã bảo vệ)
        /// </summary>
        public DefenseResultDto? DefenseResult { get; set; }

        /// <summary>
        /// Danh sách thành viên hội đồng bảo vệ
        /// </summary>
        public List<ThesisAssignmentDto> CommitteeMembers { get; set; } = new List<ThesisAssignmentDto>();
    }
}
