// File: Repositories/Interfaces/IDefenseSessionRepository.cs
using PostgraduateManagement.Models.Entities;

namespace PostgraduateManagement.Repositories.Interfaces
{
    /// <summary>
    /// Giao diện kho lưu trữ chuyên biệt cho phiên bảo vệ đề tài sau đại học (DefenseSession).
    /// </summary>
    public interface IDefenseSessionRepository : IRepository<DefenseSession>
    {
        /// <summary>
        /// Lấy chi tiết phiên bảo vệ kèm thông tin đề tài, người học, hội đồng và kết quả đánh giá.
        /// </summary>
        /// <param name="id">Mã định danh phiên bảo vệ</param>
        /// <returns>Đối tượng phiên bảo vệ với đầy đủ quan hệ</returns>
        Task<DefenseSession?> GetSessionWithDetailsAsync(int id);

        /// <summary>
        /// Lấy danh sách các phiên bảo vệ sắp diễn ra (Scheduled hoặc InProgress), sắp xếp theo thời gian tăng dần.
        /// </summary>
        /// <returns>Danh sách phiên bảo vệ sắp tới</returns>
        Task<IEnumerable<DefenseSession>> GetUpcomingSessionsAsync();

        /// <summary>
        /// Tìm phiên bảo vệ mới nhất theo mã đề tài sau đại học.
        /// </summary>
        /// <param name="thesisId">Mã định danh đề tài</param>
        /// <returns>Phiên bảo vệ tương ứng của đề tài nếu có</returns>
        Task<DefenseSession?> GetSessionByThesisIdAsync(int thesisId);
    }
}
