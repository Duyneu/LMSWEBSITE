// File: Models/Enums/CheckResult.cs
namespace PostgraduateManagement.Models.Enums
{
    /// <summary>
    /// Kết quả thẩm định điều kiện quy chuẩn đào tạo sau đại học.
    /// Dùng cho người hướng dẫn, người phản biện và cơ cấu thành viên hội đồng.
    /// </summary>
    public enum CheckResult
    {
        /// <summary>
        /// Đủ điều kiện theo quy chuẩn đào tạo
        /// </summary>
        Eligible = 1,

        /// <summary>
        /// Không đủ điều kiện theo quy chuẩn đào tạo
        /// </summary>
        Ineligible = 2,

        /// <summary>
        /// Đủ điều kiện có kèm điều kiện bổ sung hoặc phê duyệt ngoại lệ
        /// </summary>
        Conditional = 3
    }
}
