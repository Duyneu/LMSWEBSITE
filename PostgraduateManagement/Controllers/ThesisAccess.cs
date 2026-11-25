// File: Controllers/ThesisAccess.cs
using System.Security.Claims;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Models.Enums;

namespace PostgraduateManagement.Controllers
{
    /// <summary>
    /// Quy tắc phạm vi truy cập đề tài dùng chung cho danh sách đề tài và mọi trang tài liệu.
    /// </summary>
    public static class ThesisAccess
    {
        /// <summary>
        /// Phạm vi truy cập tài liệu của một đề tài (Scope Check), dùng chung cho mọi thao tác tài liệu:
        /// - Admin / Manager: toàn bộ đề tài.
        /// - Student: chỉ đề tài của chính mình.
        /// - Faculty: chỉ đề tài mình là Người hướng dẫn.
        /// - Committee: chỉ đề tài mình được phân công Phản biện hoặc Hội đồng.
        /// Đề tài phải được nạp kèm Student và Assignments.Person.
        /// </summary>
        public static bool CanAccess(ClaimsPrincipal user, Thesis thesis)
        {
            if (user.IsInRole("Admin") || user.IsInRole("Manager"))
            {
                return true;
            }

            var userName = user.Identity?.Name ?? string.Empty;
            var userEmail = user.FindFirst(ClaimTypes.Email)?.Value ?? userName;
            bool IsMe(string? email, string? sharedCode) =>
                string.Equals(email, userName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(sharedCode, userName, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrEmpty(userEmail) && string.Equals(email, userEmail, StringComparison.OrdinalIgnoreCase));

            if (user.IsInRole("Student"))
            {
                return thesis.Student != null && IsMe(thesis.Student.Email, thesis.Student.SharedStudentCode);
            }

            if (user.IsInRole("Faculty"))
            {
                return thesis.Assignments.Any(a => a.Role == AssignmentRole.Supervisor
                    && a.Person != null && IsMe(a.Person.Email, a.Person.SharedPersonCode));
            }

            if (user.IsInRole("Committee"))
            {
                return thesis.Assignments.Any(a => (a.Role == AssignmentRole.Reviewer || a.Role == AssignmentRole.Committee)
                    && a.Person != null && IsMe(a.Person.Email, a.Person.SharedPersonCode));
            }

            return false;
        }
    }
}
