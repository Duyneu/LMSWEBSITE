// File: Repositories/Interfaces/IThesisRepository.cs
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Repositories.Interfaces
{
    /// <summary>
    /// Giao diện kho lưu trữ chuyên biệt cho đề tài tốt nghiệp sau đại học (Thesis).
    /// Mở rộng IRepository với các phương thức nghiệp vụ tìm kiếm, phân trang và truy vấn quan hệ chi tiết.
    /// </summary>
    public interface IThesisRepository : IRepository<Thesis>
    {
        /// <summary>
        /// Lấy danh sách đề tài kèm thông tin quan hệ (học viên, phân công, phiên bảo vệ) có hỗ trợ tìm kiếm, lọc và phân trang.
        /// </summary>
        /// <param name="search">Từ khóa tìm kiếm theo mã, tiêu đề, lĩnh vực hoặc tên/mã học viên</param>
        /// <param name="status">Lọc theo trạng thái đề tài</param>
        /// <param name="type">Lọc theo loại hình đề tài (Đề án, Luận văn, Luận án)</param>
        /// <param name="page">Số trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số lượng bản ghi trên một trang</param>
        /// <returns>Danh sách đề tài thỏa mãn điều kiện</returns>
        Task<IEnumerable<Thesis>> GetThesesWithDetailsAsync(string? search, ThesisStatus? status, ThesisType? type, int page, int pageSize);

        /// <summary>
        /// Đếm tổng số lượng đề tài thỏa mãn điều kiện tìm kiếm và bộ lọc để phục vụ phân trang.
        /// </summary>
        /// <param name="search">Từ khóa tìm kiếm</param>
        /// <param name="status">Lọc theo trạng thái đề tài</param>
        /// <param name="type">Lọc theo loại hình đề tài</param>
        /// <returns>Tổng số bản ghi thỏa mãn</returns>
        Task<int> GetThesesCountAsync(string? search, ThesisStatus? status, ThesisType? type);

        /// <summary>
        /// Lấy thông tin chi tiết đầy đủ của một đề tài bao gồm người học, phân công (kèm chuyên gia), thẩm định điều kiện, phiên bảo vệ (kèm kết quả) và tài liệu nộp.
        /// </summary>
        /// <param name="id">Mã định danh đề tài</param>
        /// <returns>Đối tượng đề tài với toàn bộ quan hệ liên kết</returns>
        Task<Thesis?> GetThesisWithFullDetailsAsync(int id);

        /// <summary>
        /// Tìm đề tài theo mã định danh duy nhất (Code)
        /// </summary>
        /// <param name="code">Mã đề tài (ví dụ: LV-2024-001)</param>
        /// <returns>Đối tượng đề tài nếu tìm thấy, ngược lại trả về null</returns>
        Task<Thesis?> GetThesisByCodeAsync(string code);
    }
}
