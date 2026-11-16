// File: Models/Enums/AssignmentRole.cs
namespace PostgraduateManagement.Models.Enums
{
    /// <summary>
    /// Vai trò phân công chuyên môn trong quản lý đề tài sau đại học.
    /// Bao gồm người hướng dẫn, cán bộ phản biện và thành viên hội đồng đánh giá.
    /// </summary>
    public enum AssignmentRole
    {
        /// <summary>
        /// Người hướng dẫn khoa học (chính hoặc phụ)
        /// </summary>
        Supervisor = 1,

        /// <summary>
        /// Cán bộ phản biện độc lập
        /// </summary>
        Reviewer = 2,

        /// <summary>
        /// Thành viên hội đồng bảo vệ (Chủ tịch, Thư ký, Ủy viên)
        /// </summary>
        Committee = 3
    }
}
