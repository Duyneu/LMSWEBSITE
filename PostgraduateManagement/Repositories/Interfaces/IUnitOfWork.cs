// File: Repositories/Interfaces/IUnitOfWork.cs
using PostgraduateManagement.Models.Entities;

namespace PostgraduateManagement.Repositories.Interfaces
{
    /// <summary>
    /// Giao diện Unit of Work điều phối các thao tác dữ liệu trên toàn bộ các Repository,
    /// đảm bảo tính nhất quán và toàn vẹn giao dịch (ACID transaction) trong cơ sở dữ liệu.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Kho lưu trữ chuyên biệt đề tài tốt nghiệp sau đại học
        /// </summary>
        IThesisRepository Theses { get; }

        /// <summary>
        /// Kho lưu trữ chuyên biệt cán bộ, giảng viên và chuyên gia khoa học
        /// </summary>
        IAcademicPersonRepository AcademicPersons { get; }

        /// <summary>
        /// Kho lưu trữ chuyên biệt phiên bảo vệ đề tài
        /// </summary>
        IDefenseSessionRepository DefenseSessions { get; }

        /// <summary>
        /// Kho lưu trữ chuyên biệt tài liệu số hóa nộp lưu chiểu
        /// </summary>
        ISubmittedDocumentRepository SubmittedDocuments { get; }

        /// <summary>
        /// Kho lưu trữ tổng quát học viên sau đại học
        /// </summary>
        IRepository<PostgraduateStudent> PostgraduateStudents { get; }

        /// <summary>
        /// Kho lưu trữ tổng quát phân công vai trò chuyên gia
        /// </summary>
        IRepository<ThesisAssignment> ThesisAssignments { get; }

        /// <summary>
        /// Kho lưu trữ tổng quát thẩm định điều kiện quy chế đào tạo
        /// </summary>
        IRepository<EligibilityCheck> EligibilityChecks { get; }

        /// <summary>
        /// Kho lưu trữ tổng quát kết quả và biên bản phiên bảo vệ
        /// </summary>
        IRepository<DefenseResult> DefenseResults { get; }

        /// <summary>
        /// Kho lưu trữ tổng quát nhật ký kiểm toán bảo mật hệ thống
        /// </summary>
        IRepository<AuditLog> AuditLogs { get; }

        /// <summary>
        /// Kho lưu trữ nhận xét của từng thành viên trong phiên bảo vệ
        /// </summary>
        IRepository<DefenseComment> DefenseComments { get; }

        /// <summary>
        /// Kho lưu trữ báo cáo tiến độ thực hiện đề tài
        /// </summary>
        IRepository<ProgressReport> ProgressReports { get; }

        /// <summary>
        /// Lưu tất cả thay đổi trong phiên làm việc vào cơ sở dữ liệu
        /// </summary>
        /// <returns>Số lượng bản ghi bị ảnh hưởng</returns>
        Task<int> CompleteAsync();
    }
}
