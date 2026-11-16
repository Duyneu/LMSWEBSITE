// File: Models/Enums/AcademicDegree.cs
namespace PostgraduateManagement.Models.Enums
{
    /// <summary>
    /// Học hàm và học vị khoa học của cán bộ, giảng viên và chuyên gia.
    /// Sử dụng trong thẩm định điều kiện hướng dẫn, phản biện và tham gia hội đồng.
    /// </summary>
    public enum AcademicDegree
    {
        /// <summary>
        /// Tiến sĩ (Doctor of Philosophy - PhD)
        /// </summary>
        TS = 1,

        /// <summary>
        /// Phó Giáo sư (Associate Professor)
        /// </summary>
        PGS = 2,

        /// <summary>
        /// Giáo sư (Full Professor)
        /// </summary>
        GS = 3
    }
}
