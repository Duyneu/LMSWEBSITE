// File: Models/Enums/CommitteePosition.cs
namespace PostgraduateManagement.Models.Enums
{
    /// <summary>
    /// Chức danh của thành viên trong hội đồng đánh giá (chỉ dùng khi vai trò phân công là Committee).
    /// Mỗi hội đồng có tối đa một Chủ tịch và một Thư ký.
    /// </summary>
    public enum CommitteePosition
    {
        /// <summary>
        /// Chủ tịch hội đồng
        /// </summary>
        Chair = 1,

        /// <summary>
        /// Thư ký hội đồng
        /// </summary>
        Secretary = 2,

        /// <summary>
        /// Ủy viên hội đồng
        /// </summary>
        Member = 3
    }
}
