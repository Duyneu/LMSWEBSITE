// File: Models/Entities/PostgraduateStudent.cs
using System.ComponentModel.DataAnnotations;

namespace PostgraduateManagement.Models.Entities
{
    /// <summary>
    /// Thực thể quản lý hồ sơ người học sau đại học (Học viên cao học, Nghiên cứu sinh).
    /// Liên kết mã người học dùng chung với hệ thống nhân sự / người học hiện có.
    /// </summary>
    public class PostgraduateStudent
    {
        /// <summary>
        /// Khóa chính định danh người học trong hệ thống
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Mã người học dùng chung liên kết với hệ thống tổng thể LMS
        /// </summary>
        [Required]
        [StringLength(50)]
        public string SharedStudentCode { get; set; } = string.Empty;

        /// <summary>
        /// Họ và tên đầy đủ của học viên / nghiên cứu sinh
        /// </summary>
        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ thư điện tử liên lạc
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại liên lạc
        /// </summary>
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Chuyên ngành đào tạo sau đại học (ví dụ: Khoa học máy tính, Hệ thống thông tin)
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Major { get; set; } = string.Empty;

        /// <summary>
        /// Năm nhập học của khóa đào tạo
        /// </summary>
        public int EnrollmentYear { get; set; }

        /// <summary>
        /// Danh sách đề tài (luận văn, luận án, đề án) của học viên
        /// </summary>
        public virtual ICollection<Thesis> Theses { get; set; } = new List<Thesis>();
    }
}
