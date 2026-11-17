// File: Services/Implementations/EligibilityAndAssignmentService.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Repositories.Interfaces;
using PostgraduateManagement.Services.DTOs;
using PostgraduateManagement.Services.Interfaces;

namespace PostgraduateManagement.Services.Implementations
{
    /// <summary>
    /// Lớp triển khai dịch vụ thẩm định điều kiện chuyên môn và phân công chuyên gia (EligibilityAndAssignmentService).
    /// Đối soát học hàm, học vị, lĩnh vực nghiên cứu và phiên bản quy chế (Thông tư 18/2021/TT-BGDĐT & Thông tư 23/2021/TT-BGDĐT).
    /// Quản lý phân công chuyên gia cơ hữu và chuyên gia ngoài trường (IsExternal).
    /// Thực thi kiểm tra xung đột lợi ích (Conflict of Interest): Người hướng dẫn không được làm phản biện hoặc hội đồng, và ngược lại.
    /// Ghi vết kiểm toán (Audit Logging) khi phân công.
    /// </summary>
    public class EligibilityAndAssignmentService : IEligibilityAndAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EligibilityAndAssignmentService> _logger;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        private const string PhDRegulation = "Thông tư 18/2021/TT-BGDĐT về Quy chế tuyển sinh và đào tạo tiến sĩ";
        private const string MasterRegulation = "Thông tư 23/2021/TT-BGDĐT về Quy chế tuyển sinh và đào tạo thạc sĩ";

        public EligibilityAndAssignmentService(
            IUnitOfWork unitOfWork,
            ILogger<EligibilityAndAssignmentService> logger,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Thẩm định điều kiện của chuyên gia đối với một vai trò cho đề tài cụ thể.
        /// Kiểm tra xung đột lợi ích: Người hướng dẫn không thể làm phản biện/hội đồng, và ngược lại.
        /// Lưu vết lịch sử thẩm định vào bảng EligibilityCheck.
        /// </summary>
        public async Task<EligibilityCheckResultDto> CheckEligibilityAsync(int thesisId, int personId, AssignmentRole role)
        {
            var thesis = await _unitOfWork.Theses.GetByIdAsync(thesisId);
            if (thesis == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đề tài với mã định danh {thesisId}.");
            }

            var person = await _unitOfWork.AcademicPersons.GetByIdAsync(personId);
            if (person == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy chuyên gia với mã định danh {personId}.");
            }

            var regulationVersion = thesis.ThesisType == ThesisType.LuanAn ? PhDRegulation : MasterRegulation;

            // Kiểm tra xung đột lợi ích (Conflict of Interest Check)
            var currentAssignments = (await _unitOfWork.ThesisAssignments.FindAsync(
                a => a.ThesisId == thesisId && a.PersonId == personId)).ToList();

            if (role == AssignmentRole.Reviewer || role == AssignmentRole.Committee)
            {
                if (currentAssignments.Any(a => a.Role == AssignmentRole.Supervisor))
                {
                    var conflictBasis = "Xung đột lợi ích: Chuyên gia đã được phân công làm Người hướng dẫn cho đề tài này, không thể đảm nhiệm vai trò Phản biện hoặc Thành viên hội đồng.";
                    var conflictCheck = new EligibilityCheck
                    {
                        ThesisId = thesis.Id,
                        PersonId = person.Id,
                        Role = role,
                        Result = CheckResult.Ineligible,
                        Basis = conflictBasis,
                        RegulationVersion = regulationVersion,
                        CheckedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.EligibilityChecks.AddAsync(conflictCheck);
                    await _unitOfWork.CompleteAsync();

                    _logger.LogWarning("Phát hiện xung đột lợi ích: {Basis}", conflictBasis);

                    return new EligibilityCheckResultDto
                    {
                        Id = conflictCheck.Id,
                        ThesisId = thesis.Id,
                        ThesisCode = thesis.Code,
                        PersonId = person.Id,
                        PersonName = person.FullName,
                        AcademicDegree = person.AcademicDegree,
                        IsExternal = person.IsExternal,
                        Role = role,
                        Result = CheckResult.Ineligible,
                        Basis = conflictBasis,
                        RegulationVersion = regulationVersion,
                        CheckedAt = conflictCheck.CheckedAt
                    };
                }
            }
            else if (role == AssignmentRole.Supervisor)
            {
                if (currentAssignments.Any(a => a.Role == AssignmentRole.Reviewer || a.Role == AssignmentRole.Committee))
                {
                    var conflictBasis = "Xung đột lợi ích: Chuyên gia đã được phân công làm Phản biện hoặc Thành viên hội đồng cho đề tài này, không thể đảm nhiệm vai trò Người hướng dẫn.";
                    var conflictCheck = new EligibilityCheck
                    {
                        ThesisId = thesis.Id,
                        PersonId = person.Id,
                        Role = role,
                        Result = CheckResult.Ineligible,
                        Basis = conflictBasis,
                        RegulationVersion = regulationVersion,
                        CheckedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.EligibilityChecks.AddAsync(conflictCheck);
                    await _unitOfWork.CompleteAsync();

                    _logger.LogWarning("Phát hiện xung đột lợi ích: {Basis}", conflictBasis);

                    return new EligibilityCheckResultDto
                    {
                        Id = conflictCheck.Id,
                        ThesisId = thesis.Id,
                        ThesisCode = thesis.Code,
                        PersonId = person.Id,
                        PersonName = person.FullName,
                        AcademicDegree = person.AcademicDegree,
                        IsExternal = person.IsExternal,
                        Role = role,
                        Result = CheckResult.Ineligible,
                        Basis = conflictBasis,
                        RegulationVersion = regulationVersion,
                        CheckedAt = conflictCheck.CheckedAt
                    };
                }
            }

            var (result, basis) = EvaluateEligibilityRules(thesis, person, role);

            // Lưu vết lịch sử kiểm tra điều kiện quy chế vào cơ sở dữ liệu
            var checkRecord = new EligibilityCheck
            {
                ThesisId = thesis.Id,
                PersonId = person.Id,
                Role = role,
                Result = result,
                Basis = basis,
                RegulationVersion = regulationVersion,
                CheckedAt = DateTime.UtcNow
            };

            await _unitOfWork.EligibilityChecks.AddAsync(checkRecord);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Thẩm định điều kiện: Chuyên gia {Person} vai trò {Role} cho đề tài {Code}: {Result}",
                person.FullName, role, thesis.Code, result);

            return new EligibilityCheckResultDto
            {
                Id = checkRecord.Id,
                ThesisId = thesis.Id,
                ThesisCode = thesis.Code,
                PersonId = person.Id,
                PersonName = person.FullName,
                AcademicDegree = person.AcademicDegree,
                IsExternal = person.IsExternal,
                Role = role,
                Result = result,
                Basis = basis,
                RegulationVersion = regulationVersion,
                CheckedAt = checkRecord.CheckedAt
            };
        }

        /// <summary>
        /// Phân công chuyên gia vào vai trò chuyên môn cho đề tài sau đại học.
        /// Chặn tuyệt đối xung đột lợi ích: Supervisor không được đồng thời là Reviewer hoặc Committee, và ngược lại.
        /// Ghi vết kiểm toán (AuditLog) vào hệ thống.
        /// </summary>
        public async Task<bool> AssignPersonAsync(int thesisId, int personId, AssignmentRole role, bool isExternal, CommitteePosition? position = null)
        {
            // Chức danh hội đồng chỉ có nghĩa với vai trò Committee; mặc định là Ủy viên
            position = role == AssignmentRole.Committee ? position ?? CommitteePosition.Member : null;

            var thesis = await _unitOfWork.Theses.GetByIdAsync(thesisId);
            if (thesis == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đề tài với mã định danh {thesisId}.");
            }

            var person = await _unitOfWork.AcademicPersons.GetByIdAsync(personId);
            if (person == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy chuyên gia với mã định danh {personId}.");
            }

            // KIỂM TRA XUNG ĐỘT LỢI ÍCH (CONFLICT OF INTEREST CHECK)
            var existingAssignments = (await _unitOfWork.ThesisAssignments.FindAsync(
                a => a.ThesisId == thesisId && a.PersonId == personId)).ToList();

            if (role == AssignmentRole.Reviewer || role == AssignmentRole.Committee)
            {
                if (existingAssignments.Any(a => a.Role == AssignmentRole.Supervisor))
                {
                    throw new InvalidOperationException("Xung đột lợi ích: Chuyên gia đã được phân công làm Người hướng dẫn cho đề tài này, không thể đảm nhiệm vai trò Phản biện hoặc Thành viên hội đồng.");
                }
            }
            else if (role == AssignmentRole.Supervisor)
            {
                if (existingAssignments.Any(a => a.Role == AssignmentRole.Reviewer || a.Role == AssignmentRole.Committee))
                {
                    throw new InvalidOperationException("Xung đột lợi ích: Chuyên gia đã được phân công làm Phản biện hoặc Thành viên hội đồng cho đề tài này, không thể đảm nhiệm vai trò Người hướng dẫn.");
                }
            }

            // Mỗi hội đồng chỉ có một Chủ tịch và một Thư ký
            if (position == CommitteePosition.Chair || position == CommitteePosition.Secretary)
            {
                var taken = await _unitOfWork.ThesisAssignments.FindAsync(a =>
                    a.ThesisId == thesisId && a.PersonId != personId &&
                    a.Role == AssignmentRole.Committee && a.CommitteePosition == position);
                if (taken.Any())
                {
                    var title = position == CommitteePosition.Chair ? "Chủ tịch" : "Thư ký";
                    throw new InvalidOperationException($"Hội đồng của đề tài này đã có {title}. Hãy hủy phân công cũ trước khi chỉ định người khác.");
                }
            }

            // Kiểm tra phân công đã tồn tại hay chưa
            var existing = existingAssignments.FirstOrDefault(a => a.Role == role);
            if (existing != null)
            {
                existing.CommitteePosition = position;
                existing.IsExternal = isExternal;
                existing.AssignedAt = DateTime.UtcNow;
                _unitOfWork.ThesisAssignments.Update(existing);
            }
            else
            {
                var newAssignment = new ThesisAssignment
                {
                    ThesisId = thesisId,
                    PersonId = personId,
                    Role = role,
                    CommitteePosition = position,
                    IsExternal = isExternal,
                    AssignedAt = DateTime.UtcNow,
                    Notes = $"Phân công vai trò {role} theo quyết định chuyên môn."
                };
                await _unitOfWork.ThesisAssignments.AddAsync(newAssignment);
            }

            // Nếu phân công người hướng dẫn và đề tài đang ở trạng thái Approved, tự động chuyển sang InProgress
            if (role == AssignmentRole.Supervisor && thesis.ThesisStatus == ThesisStatus.Approved)
            {
                thesis.ThesisStatus = ThesisStatus.InProgress;
                _unitOfWork.Theses.Update(thesis);
            }

            // Ghi vết kiểm toán (Audit Logging)
            var user = _httpContextAccessor?.HttpContext?.User;
            var userId = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userName = user?.Identity?.Name ?? "System";

            if (_unitOfWork.AuditLogs != null)
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    UserName = userName,
                    Action = "AssignPerson",
                    EntityName = "ThesisAssignment",
                    EntityId = thesisId.ToString(),
                    Details = $"Phân công chuyên gia '{person.FullName}' (Mã: {person.SharedPersonCode ?? person.Id.ToString()}) vào vai trò {role} cho đề tài '{thesis.Code}' (Ngoài trường: {isExternal}).",
                    Timestamp = DateTime.UtcNow
                };
                await _unitOfWork.AuditLogs.AddAsync(auditLog);
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Phân công thành công chuyên gia {Person} vai trò {Role} cho đề tài {Code} (IsExternal={IsExt})",
                person.FullName, role, thesis.Code, isExternal);

            return true;
        }

        /// <summary>
        /// Lấy danh sách toàn bộ các phân công chuyên gia hiện có của một đề tài.
        /// </summary>
        public async Task<IEnumerable<ThesisAssignmentDto>> GetAssignmentsByThesisAsync(int thesisId)
        {
            var thesis = await _unitOfWork.Theses.GetThesisWithFullDetailsAsync(thesisId);
            if (thesis == null || thesis.Assignments == null)
            {
                return Enumerable.Empty<ThesisAssignmentDto>();
            }

            return thesis.Assignments.Select(a => new ThesisAssignmentDto
            {
                Id = a.Id,
                ThesisId = a.ThesisId,
                ThesisCode = thesis.Code,
                ThesisTitle = thesis.Title,
                PersonId = a.PersonId,
                PersonName = a.Person?.FullName ?? string.Empty,
                SharedPersonCode = a.Person?.SharedPersonCode ?? string.Empty,
                AcademicDegree = a.Person?.AcademicDegree ?? AcademicDegree.TS,
                ResearchField = a.Person?.ResearchField ?? string.Empty,
                Workplace = a.Person?.Workplace,
                Role = a.Role,
                CommitteePosition = a.CommitteePosition,
                IsExternal = a.IsExternal,
                AssignedAt = a.AssignedAt,
                Notes = a.Notes
            }).ToList();
        }

        /// <summary>
        /// Lấy danh sách chuyên gia khoa học đủ điều kiện cho vai trò cụ thể của đề tài.
        /// </summary>
        public async Task<IEnumerable<AcademicPersonDto>> GetEligiblePersonsForRoleAsync(int thesisId, AssignmentRole role)
        {
            var thesis = await _unitOfWork.Theses.GetByIdAsync(thesisId);
            if (thesis == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đề tài với mã định danh {thesisId}.");
            }

            var allPersons = await _unitOfWork.AcademicPersons.GetAllAsync();
            var thesisAssignments = (await _unitOfWork.ThesisAssignments.FindAsync(a => a.ThesisId == thesisId)).ToList();
            var eligiblePersons = new List<AcademicPersonDto>();

            foreach (var person in allPersons)
            {
                // Loại trừ người có xung đột lợi ích
                var isConflicted = role switch
                {
                    AssignmentRole.Reviewer or AssignmentRole.Committee => thesisAssignments.Any(a => a.PersonId == person.Id && a.Role == AssignmentRole.Supervisor),
                    AssignmentRole.Supervisor => thesisAssignments.Any(a => a.PersonId == person.Id && (a.Role == AssignmentRole.Reviewer || a.Role == AssignmentRole.Committee)),
                    _ => false
                };

                if (isConflicted)
                {
                    continue;
                }

                var (checkResult, _) = EvaluateEligibilityRules(thesis, person, role);
                if (checkResult == CheckResult.Eligible || checkResult == CheckResult.Conditional)
                {
                    eligiblePersons.Add(new AcademicPersonDto
                    {
                        Id = person.Id,
                        SharedPersonCode = person.SharedPersonCode,
                        FullName = person.FullName,
                        AcademicDegree = person.AcademicDegree,
                        ResearchField = person.ResearchField,
                        IsExternal = person.IsExternal,
                        Email = person.Email,
                        PhoneNumber = person.PhoneNumber,
                        Workplace = person.Workplace
                    });
                }
            }

            return eligiblePersons.OrderByDescending(p => p.AcademicDegree)
                                  .ThenBy(p => p.FullName);
        }

        /// <summary>
        /// Logic đánh giá điều kiện quy chế nội bộ dựa trên học vị, lĩnh vực và vai trò chuyên môn.
        /// </summary>
        private static (CheckResult Result, string Basis) EvaluateEligibilityRules(
            Thesis thesis, AcademicPerson person, AssignmentRole role)
        {
            // Kiểm tra tương đồng lĩnh vực nghiên cứu
            var isFieldMatch = IsFieldCompatible(thesis.Field, person.ResearchField);

            if (!isFieldMatch)
            {
                return (CheckResult.Ineligible,
                    $"Lĩnh vực nghiên cứu của chuyên gia '{person.ResearchField}' không phù hợp với chuyên ngành đề tài '{thesis.Field}'.");
            }

            // Quy tắc đối với Luận án tiến sĩ (PhD)
            if (thesis.ThesisType == ThesisType.LuanAn)
            {
                switch (role)
                {
                    case AssignmentRole.Supervisor:
                        if (person.AcademicDegree == AcademicDegree.GS)
                        {
                            return (CheckResult.Eligible,
                                "Giáo sư có cùng hoặc gần chuyên ngành nghiên cứu, đủ tiêu chuẩn hướng dẫn độc lập nghiên cứu sinh theo Thông tư 18/2021/TT-BGDĐT.");
                        }
                        if (person.AcademicDegree == AcademicDegree.PGS)
                        {
                            return (CheckResult.Eligible,
                                "Phó Giáo sư đúng chuyên ngành nghiên cứu, đủ tiêu chuẩn hướng dẫn độc lập nghiên cứu sinh theo Thông tư 18/2021/TT-BGDĐT.");
                        }
                        // Tiến sĩ hướng dẫn NCS: Có điều kiện (cần có đồng hướng dẫn hoặc đủ công bố quốc tế)
                        return (CheckResult.Conditional,
                            "Tiến sĩ có thể tham gia đồng hướng dẫn nghiên cứu sinh khi đáp ứng đủ tiêu chuẩn công bố khoa học theo Thông tư 18/2021/TT-BGDĐT.");

                    case AssignmentRole.Reviewer:
                        if (person.AcademicDegree == AcademicDegree.GS || person.AcademicDegree == AcademicDegree.PGS)
                        {
                            return (CheckResult.Eligible,
                                "Giáo sư/Phó Giáo sư đúng chuyên ngành, đủ điều kiện làm cán bộ phản biện độc lập luận án tiến sĩ.");
                        }
                        return (CheckResult.Eligible,
                            "Tiến sĩ đúng chuyên ngành và có công trình nghiên cứu phù hợp, đủ điều kiện phản biện độc lập luận án tiến sĩ.");

                    case AssignmentRole.Committee:
                        if (person.AcademicDegree == AcademicDegree.GS || person.AcademicDegree == AcademicDegree.PGS)
                        {
                            return (CheckResult.Eligible,
                                "Giáo sư/Phó Giáo sư đủ điều kiện đảm nhiệm chức danh Chủ tịch, Thư ký hoặc Ủy viên hội đồng đánh giá luận án tiến sĩ.");
                        }
                        return (CheckResult.Eligible,
                            "Tiến sĩ đủ điều kiện tham gia Ủy viên hoặc Thư ký hội đồng đánh giá luận án tiến sĩ.");
                }
            }

            // Quy tắc đối với Luận văn thạc sĩ và Đề án thạc sĩ (Master)
            switch (role)
            {
                case AssignmentRole.Supervisor:
                    if (person.AcademicDegree == AcademicDegree.GS || person.AcademicDegree == AcademicDegree.PGS)
                    {
                        return (CheckResult.Eligible,
                            "Giáo sư/Phó Giáo sư đủ tiêu chuẩn hướng dẫn chính luận văn/đề án thạc sĩ theo Thông tư 23/2021/TT-BGDĐT.");
                    }
                    return (CheckResult.Eligible,
                        "Tiến sĩ có ít nhất 01 năm kinh nghiệm giảng dạy/nghiên cứu sau tiến sĩ, đủ điều kiện hướng dẫn học viên cao học.");

                case AssignmentRole.Reviewer:
                    return (CheckResult.Eligible,
                        $"{person.AcademicDegree} đúng chuyên ngành, đủ điều kiện phản biện độc lập luận văn/đề án thạc sĩ.");

                case AssignmentRole.Committee:
                    return (CheckResult.Eligible,
                        $"{person.AcademicDegree} đúng chuyên ngành, đủ tiêu chuẩn tham gia hội đồng chấm luận văn/đề án thạc sĩ.");
            }

            return (CheckResult.Eligible, "Đủ điều kiện chuyên môn theo quy chế đào tạo sau đại học.");
        }

        /// <summary>
        /// Kiểm tra độ tương thích giữa lĩnh vực đề tài và lĩnh vực của chuyên gia dựa trên từ khóa.
        /// </summary>
        private static bool IsFieldCompatible(string thesisField, string personField)
        {
            if (string.IsNullOrWhiteSpace(thesisField) || string.IsNullOrWhiteSpace(personField))
            {
                return false;
            }

            var tNorm = thesisField.ToLowerInvariant();
            var pNorm = personField.ToLowerInvariant();

            // Nếu chứa chuỗi con của nhau
            if (tNorm.Contains(pNorm) || pNorm.Contains(tNorm))
            {
                return true;
            }

            // Tách từ khóa để so khớp giao thoa
            var delimiters = new[] { ' ', ',', ';', '/', '-', '&', '(', ')' };
            var tKeywords = tNorm.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                                 .Where(w => w.Length > 2)
                                 .ToHashSet();

            var pKeywords = pNorm.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                                 .Where(w => w.Length > 2);

            return pKeywords.Any(k => tKeywords.Contains(k));
        }
    }
}
