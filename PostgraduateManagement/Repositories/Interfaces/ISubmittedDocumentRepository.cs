// File: Repositories/Interfaces/ISubmittedDocumentRepository.cs
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Repositories.Interfaces
{
    /// <summary>
    /// Giao diện kho lưu trữ chuyên biệt cho tài liệu số hóa nộp lưu chiểu (SubmittedDocument).
    /// Quản lý theo danh mục bắt buộc của M05 (Thuyết minh, Toàn văn, Biên bản sửa đổi) và lịch sử phiên bản.
    /// </summary>
    public interface ISubmittedDocumentRepository : IRepository<SubmittedDocument>
    {
        /// <summary>
        /// Lấy toàn bộ danh sách tài liệu số hóa đã nộp của một đề tài, sắp xếp theo thời gian nộp và phiên bản giảm dần.
        /// </summary>
        /// <param name="thesisId">Mã định danh đề tài</param>
        /// <returns>Tập hợp các tài liệu đã nộp</returns>
        Task<IEnumerable<SubmittedDocument>> GetByThesisIdAsync(int thesisId);

        /// <summary>
        /// Lấy phiên bản tài liệu mới nhất (số version lớn nhất) theo loại tài liệu của đề tài.
        /// </summary>
        /// <param name="thesisId">Mã định danh đề tài</param>
        /// <param name="type">Loại tài liệu số hóa bắt buộc</param>
        /// <returns>Bản ghi tài liệu phiên bản mới nhất nếu có</returns>
        Task<SubmittedDocument?> GetLatestVersionAsync(int thesisId, DocumentType type);

        /// <summary>
        /// Lấy chi tiết tài liệu kèm thông tin đề tài, học viên và danh sách phân công chuyên gia.
        /// </summary>
        /// <param name="id">Mã định danh tài liệu</param>
        /// <returns>Bản ghi tài liệu kèm thông tin quan hệ liên kết</returns>
        Task<SubmittedDocument?> GetDocumentWithDetailsAsync(int id);
    }
}
