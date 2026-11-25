// File: Services/DTOs/ThesisFilterDto.cs
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Services.DTOs
{
    /// <summary>
    /// Đối tượng chứa các tham số tìm kiếm, lọc và phân trang danh sách đề tài sau đại học.
    /// </summary>
    public class ThesisFilterDto
    {
        /// <summary>
        /// Từ khóa tìm kiếm theo mã đề tài, tên đề tài, lĩnh vực hoặc thông tin học viên
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Lọc theo trạng thái đề tài (Draft, Registered, Approved, InProgress, SubmittedForReview, Defended, Completed)
        /// </summary>
        public ThesisStatus? Status { get; set; }

        /// <summary>
        /// Lọc theo loại hình đề tài (DeAn, LuanVan, LuanAn)
        /// </summary>
        public ThesisType? Type { get; set; }

        /// <summary>
        /// Số thứ tự trang cần lấy (mặc định trang 1)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Số lượng bản ghi trên một trang (mặc định 10 bản ghi)
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Chỉ trả về các đề tài có Id trong danh sách này (phạm vi truy cập của người dùng). Null = không giới hạn.
        /// </summary>
        public IReadOnlyCollection<int>? AllowedThesisIds { get; set; }
    }
}
