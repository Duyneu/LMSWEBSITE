// File: Models/Enums/DocumentType.cs
namespace PostgraduateManagement.Models.Enums
{
    /// <summary>
    /// Danh mục 3 loại tài liệu bắt buộc số hóa trong phạm vi Module M05.
    /// Báo cáo tỷ lệ số hóa chỉ được tính trên 3 loại tài liệu này.
    /// </summary>
    public enum DocumentType
    {
        /// <summary>
        /// Thuyết minh đề tài / Đề cương chi tiết nghiên cứu
        /// </summary>
        ThuyetMinh = 1,

        /// <summary>
        /// Toàn văn luận văn, đề án thạc sĩ hoặc luận án tiến sĩ
        /// </summary>
        ToanVan = 2,

        /// <summary>
        /// Biên bản giải trình và tiếp thu chỉnh sửa sau phiên bảo vệ
        /// </summary>
        BienBanSuaDoi = 3
    }
}
