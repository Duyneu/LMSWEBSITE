// File: Services/DTOs/ThesisAssignmentDto.cs
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Services.DTOs
{
    /// <summary>
    /// Đối tượng truyền tải thông tin phân công vai trò chuyên môn cho đề tài.
    /// Bao gồm người hướng dẫn, người phản biện và thành viên hội đồng.
    /// </summary>
    public class ThesisAssignmentDto
    {
        /// <summary>
        /// Mã định danh phân công
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Mã định danh đề tài
        /// </summary>
        public int ThesisId { get; set; }

        /// <summary>
        /// Mã số đề tài
        /// </summary>
        public string ThesisCode { get; set; } = string.Empty;

        /// <summary>
        /// Tên đề tài
        /// </summary>
        public string ThesisTitle { get; set; } = string.Empty;

        /// <summary>
        /// Mã định danh chuyên gia được phân công
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Họ và tên chuyên gia
        /// </summary>
        public string PersonName { get; set; } = string.Empty;

        /// <summary>
        /// Mã chuyên gia dùng chung
        /// </summary>
        public string SharedPersonCode { get; set; } = string.Empty;

        /// <summary>
        /// Học hàm, học vị chuyên gia
        /// </summary>
        public AcademicDegree AcademicDegree { get; set; }

        /// <summary>
        /// Danh xưng học hàm học vị (TS., PGS.TS., GS.TS.)
        /// </summary>
        public string DegreeTitle => AcademicDegree switch
        {
            AcademicDegree.TS => "TS.",
            AcademicDegree.PGS => "PGS.TS.",
            AcademicDegree.GS => "GS.TS.",
            _ => AcademicDegree.ToString()
        };

        /// <summary>
        /// Lĩnh vực nghiên cứu của chuyên gia
        /// </summary>
        public string ResearchField { get; set; } = string.Empty;

        /// <summary>
        /// Cơ quan / đơn vị công tác
        /// </summary>
        public string? Workplace { get; set; }

        /// <summary>
        /// Vai trò được phân công (Supervisor, Reviewer, Committee)
        /// </summary>
        public AssignmentRole Role { get; set; }

        /// <summary>
        /// Tên vai trò hiển thị tiếng Việt
        /// </summary>
        public string RoleName => Role switch
        {
            AssignmentRole.Supervisor => "Người hướng dẫn",
            AssignmentRole.Reviewer => "Người phản biện",
            AssignmentRole.Committee => "Thành viên hội đồng",
            _ => Role.ToString()
        };

        /// <summary>
        /// Đánh dấu chuyên gia ngoài trường tại thời điểm phân công
        /// </summary>
        public bool IsExternal { get; set; }

        /// <summary>
        /// Thời điểm phân công
        /// </summary>
        public DateTime AssignedAt { get; set; }

        /// <summary>
        /// Ghi chú phân công
        /// </summary>
        public string? Notes { get; set; }
    }
}
