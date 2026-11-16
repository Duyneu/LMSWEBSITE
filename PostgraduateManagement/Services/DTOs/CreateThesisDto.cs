// File: Services/DTOs/CreateThesisDto.cs
using System.ComponentModel.DataAnnotations;
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Services.DTOs
{
    /// <summary>
    /// Đối tượng truyền tải dữ liệu đăng ký hoặc tạo mới đề tài sau đại học.
    /// </summary>
    public class CreateThesisDto
    {
        /// <summary>
        /// Mã định danh đề tài (ví dụ: LV-2024-001)
        /// </summary>
        [Required(ErrorMessage = "Mã đề tài không được để trống")]
        [StringLength(50, ErrorMessage = "Mã đề tài tối đa 50 ký tự")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên đầy đủ của đề tài nghiên cứu
        /// </summary>
        [Required(ErrorMessage = "Tên đề tài không được để trống")]
        [StringLength(500, ErrorMessage = "Tên đề tài tối đa 500 ký tự")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Loại hình đề tài tốt nghiệp (Đề án, Luận văn, Luận án)
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn loại hình đề tài")]
        public ThesisType ThesisType { get; set; } = ThesisType.LuanVan;

        /// <summary>
        /// Lĩnh vực hoặc chuyên ngành nghiên cứu của đề tài
        /// </summary>
        [Required(ErrorMessage = "Lĩnh vực nghiên cứu không được để trống")]
        [StringLength(250, ErrorMessage = "Lĩnh vực nghiên cứu tối đa 250 ký tự")]
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Ngày bắt đầu thực hiện đề tài
        /// </summary>
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày dự kiến hoàn thành đề tài
        /// </summary>
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(12);

        /// <summary>
        /// Mã định danh học viên thực hiện đề tài
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn học viên thực hiện")]
        public int StudentId { get; set; }

        /// <summary>
        /// Cờ nghiệp vụ: Đề tài tốt nghiệp sau đại học (true) vs Đề tài NCKH (false).
        /// Mặc định là true cho các đề tài tốt nghiệp M05.
        /// </summary>
        public bool IsGraduationCapstone { get; set; } = true;

        /// <summary>
        /// Tóm tắt nội dung và mục tiêu nghiên cứu của đề tài
        /// </summary>
        public string? Abstract { get; set; }
    }
}
