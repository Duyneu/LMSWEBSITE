// File: Services/Interfaces/IEligibilityAndAssignmentService.cs
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Services.DTOs;

namespace PostgraduateManagement.Services.Interfaces
{
    /// <summary>
    /// Giao diện dịch vụ thẩm định điều kiện quy chuẩn đào tạo sau đại học và phân công chuyên gia.
    /// Kiểm tra học hàm, học vị, lĩnh vực nghiên cứu và quản lý chuyên gia trong/ngoài trường.
    /// </summary>
    public interface IEligibilityAndAssignmentService
    {
        /// <summary>
        /// Thẩm định điều kiện của một chuyên gia đối với một vai trò (hướng dẫn, phản biện, hội đồng) cho đề tài cụ thể.
        /// Đối chiếu với quy chế đào tạo sau đại học hiện hành và lưu vết lịch sử kiểm tra.
        /// </summary>
        /// <param name="thesisId">Mã định danh đề tài</param>
        /// <param name="personId">Mã định danh chuyên gia</param>
        /// <param name="role">Vai trò chuyên môn cần kiểm tra (Supervisor, Reviewer, Committee)</param>
        /// <returns>Kết quả thẩm định điều kiện kèm căn cứ pháp lý</returns>
        Task<EligibilityCheckResultDto> CheckEligibilityAsync(int thesisId, int personId, AssignmentRole role);

        /// <summary>
        /// Phân công chuyên gia vào vai trò cụ thể cho đề tài sau đại học.
        /// Quản lý trạng thái chuyên gia ngoài trường (IsExternal).
        /// </summary>
        /// <param name="thesisId">Mã định danh đề tài</param>
        /// <param name="personId">Mã định danh chuyên gia</param>
        /// <param name="role">Vai trò phân công</param>
        /// <param name="isExternal">Đánh dấu chuyên gia ngoài trường</param>
        /// <returns>True nếu phân công thành công</returns>
        Task<bool> AssignPersonAsync(int thesisId, int personId, AssignmentRole role, bool isExternal);

        /// <summary>
        /// Lấy danh sách toàn bộ các phân công chuyên gia hiện có của một đề tài.
        /// </summary>
        /// <param name="thesisId">Mã định danh đề tài</param>
        /// <returns>Danh sách phân công của đề tài</returns>
        Task<IEnumerable<ThesisAssignmentDto>> GetAssignmentsByThesisAsync(int thesisId);

        /// <summary>
        /// Lấy danh sách các chuyên gia khoa học đủ điều kiện cho một vai trò nhất định đối với đề tài.
        /// </summary>
        /// <param name="thesisId">Mã định danh đề tài</param>
        /// <param name="role">Vai trò cần tìm chuyên gia phù hợp</param>
        /// <returns>Danh sách chuyên gia đủ điều kiện</returns>
        Task<IEnumerable<AcademicPersonDto>> GetEligiblePersonsForRoleAsync(int thesisId, AssignmentRole role);
    }
}
