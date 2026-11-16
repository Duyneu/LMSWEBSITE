// File: Services/DTOs/EligibilityCheckResultDto.cs
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Services.DTOs
{
    /// <summary>
    /// Đối tượng truyền tải kết quả thẩm định điều kiện chuyên môn của chuyên gia
    /// cho vai trò hướng dẫn, phản biện hoặc hội đồng theo quy chế đào tạo sau đại học.
    /// </summary>
    public class EligibilityCheckResultDto
    {
        /// <summary>
        /// Mã định danh lần kiểm tra điều kiện (0 nếu chưa lưu vào DB)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Mã định danh đề tài được thẩm định
        /// </summary>
        public int ThesisId { get; set; }

        /// <summary>
        /// Mã số đề tài
        /// </summary>
        public string ThesisCode { get; set; } = string.Empty;

        /// <summary>
        /// Mã định danh chuyên gia được thẩm định
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Họ và tên chuyên gia
        /// </summary>
        public string PersonName { get; set; } = string.Empty;

        /// <summary>
        /// Học hàm, học vị chuyên gia
        /// </summary>
        public AcademicDegree AcademicDegree { get; set; }

        /// <summary>
        /// Chuyên gia ngoài trường (true) vs trong trường (false)
        /// </summary>
        public bool IsExternal { get; set; }

        /// <summary>
        /// Vai trò chuyên môn được thẩm định (Supervisor, Reviewer, Committee)
        /// </summary>
        public AssignmentRole Role { get; set; }

        /// <summary>
        /// Tên vai trò hiển thị
        /// </summary>
        public string RoleName => Role switch
        {
            AssignmentRole.Supervisor => "Người hướng dẫn",
            AssignmentRole.Reviewer => "Người phản biện",
            AssignmentRole.Committee => "Thành viên hội đồng",
            _ => Role.ToString()
        };

        /// <summary>
        /// Kết quả thẩm định: Eligible (Đủ điều kiện), Ineligible (Không đủ), Conditional (Có điều kiện)
        /// </summary>
        public CheckResult Result { get; set; }

        /// <summary>
        /// Cờ kiểm tra nhanh: Đủ điều kiện hoàn toàn
        /// </summary>
        public bool IsEligible => Result == CheckResult.Eligible;

        /// <summary>
        /// Tên kết quả hiển thị tiếng Việt
        /// </summary>
        public string ResultName => Result switch
        {
            CheckResult.Eligible => "Đủ điều kiện",
            CheckResult.Ineligible => "Không đủ điều kiện",
            CheckResult.Conditional => "Đủ điều kiện (Kèm điều kiện bổ sung)",
            _ => Result.ToString()
        };

        /// <summary>
        /// Căn cứ pháp lý, giải trình chi tiết về tiêu chuẩn học vị và lĩnh vực nghiên cứu
        /// </summary>
        public string Basis { get; set; } = string.Empty;

        /// <summary>
        /// Phiên bản quy chế đào tạo áp dụng (ví dụ: Thông tư 18/2021/TT-BGDĐT, Thông tư 23/2021/TT-BGDĐT)
        /// </summary>
        public string RegulationVersion { get; set; } = string.Empty;

        /// <summary>
        /// Thời điểm tiến hành thẩm định
        /// </summary>
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }
}
