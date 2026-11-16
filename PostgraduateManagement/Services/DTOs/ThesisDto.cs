// File: Services/DTOs/ThesisDto.cs
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Services.DTOs
{
    /// <summary>
    /// Đối tượng truyền tải dữ liệu danh sách đề tài sau đại học.
    /// Bao gồm thông tin cơ bản của đề tài, học viên thực hiện và người hướng dẫn.
    /// </summary>
    public class ThesisDto
    {
        /// <summary>
        /// Mã định danh duy nhất của đề tài
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
        /// Loại hình đề tài (Đề án, Luận văn, Luận án)
        /// </summary>
        public ThesisType ThesisType { get; set; }

        /// <summary>
        /// Tên loại hình đề tài hiển thị cho người dùng
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
        /// Tên trạng thái đề tài hiển thị tiếng Việt
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
            _ => ThesisStatus.ToString()
        };

        /// <summary>
        /// Mã định danh học viên thực hiện đề tài
        /// </summary>
        public int StudentId { get; set; }

        /// <summary>
        /// Họ và tên học viên thực hiện
        /// </summary>
        public string StudentName { get; set; } = string.Empty;

        /// <summary>
        /// Mã số học viên dùng chung toàn hệ thống
        /// </summary>
        public string StudentCode { get; set; } = string.Empty;

        /// <summary>
        /// Cờ nghiệp vụ: Đề tài tốt nghiệp sau đại học (true) vs Đề tài NCKH (false)
        /// </summary>
        public bool IsGraduationCapstone { get; set; }

        /// <summary>
        /// Danh sách họ tên những người hướng dẫn khoa học của đề tài
        /// </summary>
        public List<string> Supervisors { get; set; } = new List<string>();

        /// <summary>
        /// Chuỗi hiển thị tên người hướng dẫn nối tiếp
        /// </summary>
        public string SupervisorNames => Supervisors.Count > 0 ? string.Join(", ", Supervisors) : "Chưa phân công";

        /// <summary>
        /// Số lượng tài liệu đã nộp
        /// </summary>
        public int SubmittedDocumentsCount { get; set; }

        /// <summary>
        /// Đã lên lịch phiên bảo vệ hay chưa
        /// </summary>
        public bool HasScheduledDefense { get; set; }
    }
}
