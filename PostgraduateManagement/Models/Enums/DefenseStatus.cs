// File: Models/Enums/DefenseStatus.cs
namespace PostgraduateManagement.Models.Enums
{
    /// <summary>
    /// Trạng thái của phiên bảo vệ đề tài sau đại học.
    /// Quản lý vòng đời tổ chức đánh giá luận văn, luận án, đề án.
    /// </summary>
    public enum DefenseStatus
    {
        /// <summary>
        /// Phiên bảo vệ đã được lập lịch và thành lập hội đồng
        /// </summary>
        Scheduled = 1,

        /// <summary>
        /// Phiên bảo vệ đang được tiến hành
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// Phiên bảo vệ đã hoàn thành và có kết quả đánh giá
        /// </summary>
        Defended = 3,

        /// <summary>
        /// Phiên bảo vệ bị hủy hoặc tạm hoãn
        /// </summary>
        Cancelled = 4
    }
}
