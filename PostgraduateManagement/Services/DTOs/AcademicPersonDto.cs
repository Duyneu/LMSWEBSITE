// File: Services/DTOs/AcademicPersonDto.cs
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Services.DTOs
{
    /// <summary>
    /// Đối tượng truyền tải thông tin cán bộ, giảng viên và chuyên gia khoa học.
    /// Sử dụng trong phân công hướng dẫn, phản biện, hội đồng và gợi ý phản biện AI.
    /// </summary>
    public class AcademicPersonDto
    {
        /// <summary>
        /// Mã định danh chuyên gia / cán bộ
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Mã định danh chuyên gia dùng chung toàn hệ thống
        /// </summary>
        public string? SharedPersonCode { get; set; }

        /// <summary>
        /// Họ và tên chuyên gia
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Học hàm, học vị cao nhất (TS, PGS, GS)
        /// </summary>
        public AcademicDegree AcademicDegree { get; set; }

        /// <summary>
        /// Tên viết tắt học hàm, học vị (TS., PGS.TS., GS.TS.)
        /// </summary>
        public string DegreeTitle => AcademicDegree switch
        {
            AcademicDegree.TS => "TS.",
            AcademicDegree.PGS => "PGS.TS.",
            AcademicDegree.GS => "GS.TS.",
            _ => AcademicDegree.ToString()
        };

        /// <summary>
        /// Lĩnh vực nghiên cứu chuyên sâu
        /// </summary>
        public string ResearchField { get; set; } = string.Empty;

        /// <summary>
        /// Đánh dấu chuyên gia ngoài trường (true) vs cán bộ cơ hữu trong trường (false)
        /// </summary>
        public bool IsExternal { get; set; }

        /// <summary>
        /// Địa chỉ thư điện tử
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại liên lạc
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Đơn vị công tác, viện nghiên cứu hoặc doanh nghiệp
        /// </summary>
        public string? Workplace { get; set; }

        /// <summary>
        /// Số lượng đề tài đang hướng dẫn hoặc tham gia
        /// </summary>
        public int ActiveAssignmentsCount { get; set; }
    }
}
