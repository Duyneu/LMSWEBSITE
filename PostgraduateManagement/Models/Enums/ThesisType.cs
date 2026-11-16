// File: Models/Enums/ThesisType.cs
namespace PostgraduateManagement.Models.Enums
{
    /// <summary>
    /// Loại hình đề tài tốt nghiệp sau đại học.
    /// Xác định cấp độ đào tạo và các tiêu chuẩn thẩm định tương ứng.
    /// </summary>
    public enum ThesisType
    {
        /// <summary>
        /// Đề án thạc sĩ (Chương trình đào tạo thạc sĩ định hướng ứng dụng)
        /// </summary>
        DeAn = 1,

        /// <summary>
        /// Luận văn thạc sĩ (Chương trình đào tạo thạc sĩ định hướng nghiên cứu)
        /// </summary>
        LuanVan = 2,

        /// <summary>
        /// Luận án tiến sĩ (Chương trình đào tạo nghiên cứu sinh tiến sĩ)
        /// </summary>
        LuanAn = 3
    }
}
