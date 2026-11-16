// File: Services/DTOs/DigitizationProgressDto.cs
namespace PostgraduateManagement.Services.DTOs
{
    /// <summary>
    /// Đối tượng truyền tải báo cáo tiến độ số hóa tài liệu của đề tài sau đại học.
    /// QUY TẮC RANH GIỚI BẮT BUỘC (CRITICAL BOUNDARY RULE):
    /// Tiến độ số hóa chỉ được tính toán STRICTLY trên phạm vi danh mục 03 loại tài liệu bắt buộc của Module M05:
    /// Thuyết minh, Toàn văn, Biên bản sửa đổi.
    /// (3/3 = 100%, 2/3 = 66.7%, 1/3 = 33.3%, 0/3 = 0%).
    /// Tuyệt đối không suy diễn toàn bộ quy trình đào tạo sau đại học đạt 100%.
    /// </summary>
    public class DigitizationProgressDto
    {
        /// <summary>
        /// Mã định danh đề tài
        /// </summary>
        public int ThesisId { get; set; }

        /// <summary>
        /// Mã số đề tài (ví dụ: LV-2024-001)
        /// </summary>
        public string ThesisCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên đầy đủ của đề tài
        /// </summary>
        public string ThesisTitle { get; set; } = string.Empty;

        /// <summary>
        /// Tổng số loại tài liệu bắt buộc quy định bởi Module M05 (cố định là 3)
        /// </summary>
        public int TotalRequiredDocuments { get; set; } = 3;

        /// <summary>
        /// Số lượng loại tài liệu bắt buộc đã được tải lên ít nhất một phiên bản (0 đến 3)
        /// </summary>
        public int UploadedDocumentsCount { get; set; }

        /// <summary>
        /// Số lượng loại tài liệu đã được phê duyệt / xác nhận hợp lệ (0 đến 3)
        /// </summary>
        public int ConfirmedDocumentsCount { get; set; }

        /// <summary>
        /// Tỷ lệ phần trăm số hóa tính theo danh mục 03 loại tài liệu bắt buộc của M05.
        /// Các giá trị hợp lệ: 0.0%, 33.3%, 66.7%, 100.0%.
        /// </summary>
        public double ProgressPercentage { get; set; }

        /// <summary>
        /// Đã nộp Thuyết minh đề cương nghiên cứu hay chưa
        /// </summary>
        public bool HasThuyetMinh { get; set; }

        /// <summary>
        /// Đã nộp Toàn văn luận văn / luận án / đề án hay chưa
        /// </summary>
        public bool HasToanVan { get; set; }

        /// <summary>
        /// Đã nộp Biên bản tiếp thu giải trình sửa đổi sau bảo vệ hay chưa
        /// </summary>
        public bool HasBienBanSuaDoi { get; set; }

        /// <summary>
        /// Thông điệp cảnh báo ranh giới bắt buộc theo quy định Module M05.
        /// </summary>
        public string DisclaimerNote { get; set; } = "Tỷ lệ số hóa này chỉ áp dụng trong phạm vi danh mục 03 tài liệu bắt buộc của Module M05, không suy diễn toàn bộ quy trình đào tạo sau đại học đạt 100%.";

        /// <summary>
        /// Danh sách chi tiết các tài liệu đã nộp (các phiên bản mới nhất hoặc toàn bộ)
        /// </summary>
        public List<SubmittedDocumentDto> UploadedDocuments { get; set; } = new List<SubmittedDocumentDto>();
    }
}
