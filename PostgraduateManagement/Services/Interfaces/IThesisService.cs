// File: Services/Interfaces/IThesisService.cs
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Services.DTOs;

namespace PostgraduateManagement.Services.Interfaces
{
    /// <summary>
    /// Giao diện dịch vụ quản lý vòng đời đề tài tốt nghiệp sau đại học (Đề án, Luận văn, Luận án).
    /// Thực thi quy tắc kiểm tra điều kiện bảo vệ, phân biệt đề tài tốt nghiệp với đề tài NCKH.
    /// </summary>
    public interface IThesisService
    {
        /// <summary>
        /// Lấy danh sách đề tài sau đại học có phân trang, hỗ trợ tìm kiếm và lọc theo trạng thái, loại hình.
        /// </summary>
        /// <param name="filter">Bộ lọc và tham số phân trang</param>
        /// <returns>Đối tượng kết quả phân trang chứa danh sách ThesisDto</returns>
        Task<PagedResult<ThesisDto>> GetThesesAsync(ThesisFilterDto filter);

        /// <summary>
        /// Lấy thông tin chi tiết toàn diện của một đề tài theo mã định danh.
        /// Bao gồm người học, phân công chuyên gia, thẩm định điều kiện, phiên bảo vệ và tài liệu số hóa.
        /// </summary>
        /// <param name="id">Mã định danh đề tài</param>
        /// <returns>Đối tượng ThesisDetailDto nếu tìm thấy, ngược lại null</returns>
        Task<ThesisDetailDto?> GetThesisByIdAsync(int id);

        /// <summary>
        /// Tạo mới hoặc đăng ký đề tài sau đại học vào hệ thống.
        /// </summary>
        /// <param name="dto">Dữ liệu tạo mới đề tài</param>
        /// <returns>Đối tượng ThesisDto của đề tài vừa tạo</returns>
        Task<ThesisDto> CreateThesisAsync(CreateThesisDto dto);

        /// <summary>
        /// Phê duyệt đề cương đề tài sau đại học (chuyển trạng thái sang Approved).
        /// </summary>
        /// <param name="id">Mã định danh đề tài</param>
        /// <returns>True nếu phê duyệt thành công, ngược lại False</returns>
        Task<bool> ApproveThesisAsync(int id);

        /// <summary>
        /// Chuyển trạng thái vòng đời của đề tài theo quy trình đào tạo.
        /// </summary>
        /// <param name="id">Mã định danh đề tài</param>
        /// <param name="newStatus">Trạng thái mới cần chuyển</param>
        /// <returns>True nếu chuyển trạng thái thành công, ngược lại False</returns>
        Task<bool> TransitionStatusAsync(int id, ThesisStatus newStatus);

        /// <summary>
        /// Thẩm định điều kiện tiên quyết để được tham gia phiên bảo vệ tốt nghiệp.
        /// QUY TẮC CỐT LÕI: Phải kiểm tra IsGraduationCapstone == true.
        /// Nếu là đề tài NCKH (IsGraduationCapstone == false), ném ra InvalidOperationException chặn tuyệt đối vào luồng bảo vệ.
        /// </summary>
        /// <param name="id">Mã định danh đề tài</param>
        /// <returns>True nếu đề tài đủ điều kiện bảo vệ</returns>
        Task<bool> ValidateForDefenseAsync(int id);
    }
}
