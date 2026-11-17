// File: Services/DTOs/ThesisDetailDto.cs
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Services.DTOs
{
    /// <summary>
    /// Đối tượng truyền tải chi tiết toàn diện của một đề tài sau đại học.
    /// Bao gồm thông tin người học, phân công chuyên gia, thẩm định điều kiện,
    /// các phiên bảo vệ kèm kết quả, tài liệu số hóa và tiến độ số hóa M05.
    /// </summary>
    public class ThesisDetailDto
    {
        /// <summary>
        /// Mã định danh đề tài
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Mã số đề tài (ví dụ: LV-2024-001)
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên đầy đủ của đề tài nghiên cứu
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Loại hình đề tài tốt nghiệp
        /// </summary>
        public ThesisType ThesisType { get; set; }

        /// <summary>
        /// Tên loại hình đề tài hiển thị
        /// </summary>
        public string ThesisTypeName => ThesisType switch
        {
            ThesisType.DeAn => "Đề án thạc sĩ",
            ThesisType.LuanVan => "Luận văn thạc sĩ",
            ThesisType.LuanAn => "Luận án tiến sĩ",
            _ => ThesisType.ToString()
        };

        /// <summary>
        /// Lĩnh vực nghiên cứu chuyên sâu
        /// </summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Ngày bắt đầu thực hiện đề tài
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày dự kiến hoàn thành đề tài
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Trạng thái vòng đời hiện tại của đề tài
        /// </summary>
        public ThesisStatus ThesisStatus { get; set; }

        /// <summary>
        /// Tên trạng thái hiển thị tiếng Việt
        /// </summary>
        public string ThesisStatusName => ThesisStatus switch
        {
            ThesisStatus.Draft => "Dự thảo",
            ThesisStatus.Registered => "Đã đăng ký",
            ThesisStatus.Approved => "Đã phê duyệt",
            ThesisStatus.InProgress => "Đang thực hiện",
            ThesisStatus.SubmittedForReview => "Chờ phản biện & bảo vệ",
            ThesisStatus.Defended => "Đã bảo vệ",
            ThesisStatus.Completed => "Hoàn thành",
            ThesisStatus.Archived => "Đã lưu trữ",
            _ => ThesisStatus.ToString()
        };

        /// <summary>
        /// Cờ nghiệp vụ cốt lõi: true = Đề tài tốt nghiệp sau đại học; false = Đề tài NCKH
        /// </summary>
        public bool IsGraduationCapstone { get; set; }

        /// <summary>
        /// Tóm tắt nội dung và mục tiêu nghiên cứu
        /// </summary>
        public string? Abstract { get; set; }

        /// <summary>
        /// Mã định danh học viên
        /// </summary>
        public int StudentId { get; set; }

        /// <summary>
        /// Họ và tên học viên
        /// </summary>
        public string StudentName { get; set; } = string.Empty;

        /// <summary>
        /// Mã học viên dùng chung
        /// </summary>
        public string StudentCode { get; set; } = string.Empty;

        /// <summary>
        /// Email học viên
        /// </summary>
        public string StudentEmail { get; set; } = string.Empty;

        /// <summary>
        /// Chuyên ngành đào tạo của học viên
        /// </summary>
        public string StudentMajor { get; set; } = string.Empty;

        /// <summary>
        /// Năm nhập học của học viên
        /// </summary>
        public int StudentEnrollmentYear { get; set; }

        /// <summary>
        /// Danh sách các chuyên gia được phân công (hướng dẫn, phản biện, hội đồng)
        /// </summary>
        public List<ThesisAssignmentDto> Assignments { get; set; } = new List<ThesisAssignmentDto>();

        /// <summary>
        /// Lịch sử các lần thẩm định điều kiện quy chế
        /// </summary>
        public List<EligibilityCheckResultDto> EligibilityChecks { get; set; } = new List<EligibilityCheckResultDto>();

        /// <summary>
        /// Danh sách các phiên bảo vệ kèm kết quả đánh giá
        /// </summary>
        public List<DefenseSessionDto> DefenseSessions { get; set; } = new List<DefenseSessionDto>();

        /// <summary>
        /// Danh sách các tài liệu số hóa đã nộp
        /// </summary>
        public List<SubmittedDocumentDto> SubmittedDocuments { get; set; } = new List<SubmittedDocumentDto>();

        /// <summary>
        /// Báo cáo tiến độ số hóa theo danh mục 03 tài liệu bắt buộc M05
        /// </summary>
        public DigitizationProgressDto? DigitizationProgress { get; set; }
    }
}
