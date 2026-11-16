// File: Repositories/Interfaces/IAcademicPersonRepository.cs
using PostgraduateManagement.Models.Entities;

namespace PostgraduateManagement.Repositories.Interfaces
{
    /// <summary>
    /// Giao diện kho lưu trữ chuyên biệt cho cán bộ, giảng viên và chuyên gia khoa học (AcademicPerson).
    /// Hỗ trợ tìm kiếm phục vụ phân công hướng dẫn, phản biện, hội đồng và gợi ý phản biện AI.
    /// </summary>
    public interface IAcademicPersonRepository : IRepository<AcademicPerson>
    {
        /// <summary>
        /// Lấy danh sách các chuyên gia đủ điều kiện phản biện theo lĩnh vực nghiên cứu (tương đồng lĩnh vực).
        /// </summary>
        /// <param name="researchField">Lĩnh vực hoặc chuyên ngành nghiên cứu</param>
        /// <returns>Danh sách chuyên gia khoa học phù hợp</returns>
        Task<IEnumerable<AcademicPerson>> GetEligibleReviewersAsync(string researchField);

        /// <summary>
        /// Tìm chuyên gia/cán bộ theo mã định danh dùng chung toàn hệ thống (SharedPersonCode).
        /// </summary>
        /// <param name="sharedPersonCode">Mã cán bộ/chuyên gia dùng chung</param>
        /// <returns>Đối tượng chuyên gia nếu tìm thấy, ngược lại null</returns>
        Task<AcademicPerson?> GetBySharedCodeAsync(string sharedPersonCode);

        /// <summary>
        /// Lấy danh sách toàn bộ chuyên gia ngoài trường (IsExternal = true) phục vụ cơ cấu hội đồng và phản biện độc lập.
        /// </summary>
        /// <returns>Danh sách chuyên gia ngoài trường</returns>
        Task<IEnumerable<AcademicPerson>> GetExternalExpertsAsync();
    }
}
