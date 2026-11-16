// File: Services/Interfaces/IDefenseAndDocumentService.cs
using PostgraduateManagement.Services.DTOs;

namespace PostgraduateManagement.Services.Interfaces
{
    /// <summary>
    /// Giao diện dịch vụ quản lý tổ chức bảo vệ đề tài và số hóa tài liệu lưu chiểu.
    /// Bao gồm lập lịch bảo vệ, ghi nhận điểm và nhận xét của hội đồng;
    /// quản lý nộp tài liệu số hóa, quản lý phiên bản và tính toán tiến độ số hóa M05.
    /// </summary>
    public interface IDefenseAndDocumentService
    {
        /// <summary>
        /// Lập lịch phiên bảo vệ cho đề tài sau đại học.
        /// BẮT BUỘC: Kiểm tra thẩm định điều kiện qua ThesisService.ValidateForDefenseAsync trước khi tạo lịch.
        /// </summary>
        /// <param name="dto">Thông tin lập lịch bảo vệ</param>
        /// <returns>Đối tượng DefenseSessionDto của phiên bảo vệ vừa tạo</returns>
        Task<DefenseSessionDto> ScheduleDefenseAsync(ScheduleDefenseDto dto);

        /// <summary>
        /// Ghi nhận kết quả đánh giá, điểm số và nhận xét của hội đồng bảo vệ đề tài.
        /// Tự động cập nhật trạng thái phiên bảo vệ và đề tài sang Defended.
        /// </summary>
        /// <param name="dto">Dữ liệu kết quả đánh giá bảo vệ</param>
        /// <param name="recordedBy">Họ tên hoặc định danh cán bộ/thư ký ghi nhận</param>
        /// <returns>Đối tượng DefenseResultDto của kết quả vừa ghi nhận</returns>
        Task<DefenseResultDto> RecordDefenseResultAsync(RecordDefenseResultDto dto, string recordedBy);

        /// <summary>
        /// Tải lên tài liệu số hóa nộp lưu chiểu cho đề tài sau đại học.
        /// Giới hạn nghiêm ngặt trong danh mục 3 loại tài liệu bắt buộc của Module M05:
        /// Thuyết minh, Toàn văn, Biên bản sửa đổi.
        /// Tự động đánh số phiên bản tăng dần theo từng loại tài liệu.
        /// </summary>
        /// <param name="dto">Dữ liệu tài liệu tải lên</param>
        /// <returns>Đối tượng SubmittedDocumentDto của tài liệu vừa lưu</returns>
        Task<SubmittedDocumentDto> UploadDocumentAsync(UploadDocumentDto dto);

        /// <summary>
        /// Xác nhận phê duyệt tính hợp lệ của tài liệu số hóa đã nộp.
        /// </summary>
        /// <param name="documentId">Mã định danh tài liệu</param>
        /// <param name="confirmedBy">Họ tên cán bộ/hội đồng xác nhận</param>
        /// <returns>True nếu xác nhận thành công</returns>
        Task<bool> ConfirmDocumentAsync(int documentId, string confirmedBy);

        /// <summary>
        /// Tính toán báo cáo tiến độ số hóa tài liệu của đề tài.
        /// RANH GIỚI BẮT BUỘC: Chỉ tính trên danh mục 03 tài liệu bắt buộc của Module M05,
        /// kèm cảnh báo không suy diễn toàn bộ quy trình đào tạo sau đại học đạt 100%.
        /// </summary>
        /// <param name="thesisId">Mã định danh đề tài</param>
        /// <returns>Báo cáo tiến độ số hóa DigitizationProgressDto</returns>
        Task<DigitizationProgressDto> GetDigitizationProgressAsync(int thesisId);
    }
}
