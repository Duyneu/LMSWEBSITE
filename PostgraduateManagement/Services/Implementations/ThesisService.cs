// File: Services/Implementations/ThesisService.cs
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
    /// Lớp triển khai dịch vụ quản lý đề tài sau đại học (ThesisService).
    /// Quản lý toàn bộ vòng đời đề tài (Draft -> Registered -> Approved -> InProgress -> SubmittedForReview -> Defended -> Completed).
    /// Thực thi quy tắc kiểm tra IsGraduationCapstone, từ chối đề tài NCKH tham gia quy trình bảo vệ tốt nghiệp.
    /// Ghi vết kiểm toán (Audit Logging) và kiểm tra xung đột lợi ích bảo vệ tốt nghiệp.
    /// </summary>
    public class ThesisService : IThesisService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ThesisService> _logger;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public ThesisService(
            IUnitOfWork _unitOfWork,
            ILogger<ThesisService> logger,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            this._unitOfWork = _unitOfWork;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Lấy danh sách đề tài sau đại học có phân trang, tìm kiếm và lọc.
        /// </summary>
        public async Task<PagedResult<ThesisDto>> GetThesesAsync(ThesisFilterDto filter)
        {
            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

            var entities = await _unitOfWork.Theses.GetThesesWithDetailsAsync(
                filter.Search, filter.Status, filter.Type, pageNumber, pageSize);

            var totalCount = await _unitOfWork.Theses.GetThesesCountAsync(
                filter.Search, filter.Status, filter.Type);

            var dtos = entities.Select(t => new ThesisDto
            {
                Id = t.Id,
                Code = t.Code,
                Title = t.Title,
                ThesisType = t.ThesisType,
                Field = t.Field,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                ThesisStatus = t.ThesisStatus,
                StudentId = t.StudentId,
                StudentName = t.Student?.FullName ?? string.Empty,
                StudentCode = t.Student?.SharedStudentCode ?? string.Empty,
                IsGraduationCapstone = t.IsGraduationCapstone,
                Supervisors = t.Assignments?
                    .Where(a => a.Role == AssignmentRole.Supervisor && a.Person != null)
                    .Select(a => $"{a.Person!.FullName}")
                    .ToList() ?? new List<string>(),
                SubmittedDocumentsCount = t.SubmittedDocuments?.Count ?? 0,
                HasScheduledDefense = t.DefenseSessions?.Any(ds => ds.Status == DefenseStatus.Scheduled || ds.Status == DefenseStatus.InProgress) ?? false
            }).ToList();

            return new PagedResult<ThesisDto>(dtos, totalCount, pageNumber, pageSize);
        }

        /// <summary>
        /// Lấy chi tiết toàn diện của một đề tài kèm tất cả các quan hệ liên kết.
        /// </summary>
        public async Task<ThesisDetailDto?> GetThesisByIdAsync(int id)
        {
            var t = await _unitOfWork.Theses.GetThesisWithFullDetailsAsync(id);
            if (t == null)
            {
                return null;
            }

            var detail = new ThesisDetailDto
            {
                Id = t.Id,
                Code = t.Code,
                Title = t.Title,
                ThesisType = t.ThesisType,
                Field = t.Field,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                ThesisStatus = t.ThesisStatus,
                IsGraduationCapstone = t.IsGraduationCapstone,
                Abstract = t.Abstract,
                StudentId = t.StudentId,
                StudentName = t.Student?.FullName ?? string.Empty,
                StudentCode = t.Student?.SharedStudentCode ?? string.Empty,
                StudentEmail = t.Student?.Email ?? string.Empty,
                StudentMajor = t.Student?.Major ?? string.Empty,
                StudentEnrollmentYear = t.Student?.EnrollmentYear ?? 0,
                Assignments = t.Assignments?.Select(a => new ThesisAssignmentDto
                {
                    Id = a.Id,
                    ThesisId = a.ThesisId,
                    ThesisCode = t.Code,
                    ThesisTitle = t.Title,
                    PersonId = a.PersonId,
                    PersonName = a.Person?.FullName ?? string.Empty,
                    SharedPersonCode = a.Person?.SharedPersonCode ?? string.Empty,
                    AcademicDegree = a.Person?.AcademicDegree ?? AcademicDegree.TS,
                    ResearchField = a.Person?.ResearchField ?? string.Empty,
                    Workplace = a.Person?.Workplace,
                    Role = a.Role,
                    IsExternal = a.IsExternal,
                    AssignedAt = a.AssignedAt,
                    Notes = a.Notes
                }).ToList() ?? new List<ThesisAssignmentDto>(),
                EligibilityChecks = t.EligibilityChecks?.Select(ec => new EligibilityCheckResultDto
                {
                    Id = ec.Id,
                    ThesisId = ec.ThesisId,
                    ThesisCode = t.Code,
                    PersonId = ec.PersonId,
                    PersonName = ec.Person?.FullName ?? string.Empty,
                    AcademicDegree = ec.Person?.AcademicDegree ?? AcademicDegree.TS,
                    IsExternal = ec.Person?.IsExternal ?? false,
                    Role = ec.Role,
                    Result = ec.Result,
                    Basis = ec.Basis,
                    RegulationVersion = ec.RegulationVersion,
                    CheckedAt = ec.CheckedAt
                }).ToList() ?? new List<EligibilityCheckResultDto>(),
                DefenseSessions = t.DefenseSessions?.Select(ds => new DefenseSessionDto
                {
                    Id = ds.Id,
                    ThesisId = ds.ThesisId,
                    ThesisCode = t.Code,
                    ThesisTitle = t.Title,
                    ThesisType = t.ThesisType,
                    Field = t.Field,
                    StudentName = t.Student?.FullName ?? string.Empty,
                    StudentCode = t.Student?.SharedStudentCode ?? string.Empty,
                    ScheduledAt = ds.ScheduledAt,
                    Location = ds.Location,
                    Status = ds.Status,
                    CommitteeNotes = ds.CommitteeNotes,
                    DefenseResult = ds.DefenseResult != null ? new DefenseResultDto
                    {
                        Id = ds.DefenseResult.Id,
                        SessionId = ds.DefenseResult.SessionId,
                        Comments = ds.DefenseResult.Comments,
                        Score = ds.DefenseResult.Score,
                        Pass = ds.DefenseResult.Pass,
                        RecordedBy = ds.DefenseResult.RecordedBy,
                        RecordedAt = ds.DefenseResult.RecordedAt
                    } : null
                }).ToList() ?? new List<DefenseSessionDto>(),
                SubmittedDocuments = t.SubmittedDocuments?.Select(sd => new SubmittedDocumentDto
                {
                    Id = sd.Id,
                    ThesisId = sd.ThesisId,
                    DocumentType = sd.DocumentType,
                    FileName = sd.FileName,
                    FilePath = sd.FilePath,
                    Version = sd.Version,
                    UploadedAt = sd.UploadedAt,
                    ConfirmedBy = sd.ConfirmedBy
                }).ToList() ?? new List<SubmittedDocumentDto>()
            };

            // Tính toán tiến độ số hóa tài liệu nghiêm ngặt theo 3 loại tài liệu bắt buộc M05
            var docs = t.SubmittedDocuments ?? new List<SubmittedDocument>();
            var hasThuyetMinh = docs.Any(d => d.DocumentType == DocumentType.ThuyetMinh);
            var hasToanVan = docs.Any(d => d.DocumentType == DocumentType.ToanVan);
            var hasBienBan = docs.Any(d => d.DocumentType == DocumentType.BienBanSuaDoi);
            var uploadedCount = (hasThuyetMinh ? 1 : 0) + (hasToanVan ? 1 : 0) + (hasBienBan ? 1 : 0);
            var confirmedCount = docs.Where(d => !string.IsNullOrEmpty(d.ConfirmedBy))
                                     .Select(d => d.DocumentType)
                                     .Distinct()
                                     .Count();

            var percentage = uploadedCount switch
            {
                3 => 100.0,
                2 => 66.7,
                1 => 33.3,
                _ => 0.0
            };

            detail.DigitizationProgress = new DigitizationProgressDto
            {
                ThesisId = t.Id,
                ThesisCode = t.Code,
                ThesisTitle = t.Title,
                TotalRequiredDocuments = 3,
                UploadedDocumentsCount = uploadedCount,
                ConfirmedDocumentsCount = confirmedCount,
                ProgressPercentage = percentage,
                HasThuyetMinh = hasThuyetMinh,
                HasToanVan = hasToanVan,
                HasBienBanSuaDoi = hasBienBan,
                UploadedDocuments = detail.SubmittedDocuments
            };

            return detail;
        }

        /// <summary>
        /// Tạo mới hoặc đăng ký đề tài sau đại học vào hệ thống.
        /// </summary>
        public async Task<ThesisDto> CreateThesisAsync(CreateThesisDto dto)
        {
            var student = await _unitOfWork.PostgraduateStudents.GetByIdAsync(dto.StudentId);
            if (student == null)
            {
                throw new ArgumentException($"Không tìm thấy học viên sau đại học với mã định danh {dto.StudentId}.");
            }

            var existingCode = await _unitOfWork.Theses.GetThesisByCodeAsync(dto.Code);
            if (existingCode != null)
            {
                throw new InvalidOperationException($"Mã đề tài '{dto.Code}' đã tồn tại trong hệ thống. Vui lòng chọn mã khác.");
            }

            var thesis = new Thesis
            {
                Code = dto.Code.Trim(),
                Title = dto.Title.Trim(),
                ThesisType = dto.ThesisType,
                Field = dto.Field.Trim(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ThesisStatus = ThesisStatus.Draft,
                StudentId = dto.StudentId,
                IsGraduationCapstone = dto.IsGraduationCapstone,
                Abstract = dto.Abstract?.Trim()
            };

            await _unitOfWork.Theses.AddAsync(thesis);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Tạo mới thành công đề tài sau đại học: {Code} - {Title} cho học viên {StudentName}",
                thesis.Code, thesis.Title, student.FullName);

            return new ThesisDto
            {
                Id = thesis.Id,
                Code = thesis.Code,
                Title = thesis.Title,
                ThesisType = thesis.ThesisType,
                Field = thesis.Field,
                StartDate = thesis.StartDate,
                EndDate = thesis.EndDate,
                ThesisStatus = thesis.ThesisStatus,
                StudentId = thesis.StudentId,
                StudentName = student.FullName,
                StudentCode = student.SharedStudentCode,
                IsGraduationCapstone = thesis.IsGraduationCapstone
            };
        }

        /// <summary>
        /// Phê duyệt đề cương đề tài sau đại học.
        /// Ghi vết kiểm toán (Audit Logging).
        /// </summary>
        public async Task<bool> ApproveThesisAsync(int id)
        {
            var thesis = await _unitOfWork.Theses.GetByIdAsync(id);
            if (thesis == null)
            {
                _logger.LogWarning("Không tìm thấy đề tài có mã định danh {Id} để phê duyệt.", id);
                return false;
            }

            if (thesis.ThesisStatus == ThesisStatus.Draft || thesis.ThesisStatus == ThesisStatus.Registered)
            {
                thesis.ThesisStatus = ThesisStatus.Approved;
                _unitOfWork.Theses.Update(thesis);

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
                        Action = "ApproveThesis",
                        EntityName = "Thesis",
                        EntityId = thesis.Id.ToString(),
                        Details = $"Phê duyệt đề cương đề tài: [{thesis.Code}] {thesis.Title}",
                        Timestamp = DateTime.UtcNow
                    };
                    await _unitOfWork.AuditLogs.AddAsync(auditLog);
                }

                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Phê duyệt đề tài thành công: {Code}", thesis.Code);
                return true;
            }

            return true;
        }

        /// <summary>
        /// Chuyển đổi trạng thái vòng đời của đề tài.
        /// Ghi vết kiểm toán (Audit Logging).
        /// </summary>
        public async Task<bool> TransitionStatusAsync(int id, ThesisStatus newStatus)
        {
            var thesis = await _unitOfWork.Theses.GetByIdAsync(id);
            if (thesis == null)
            {
                _logger.LogWarning("Không tìm thấy đề tài có mã định danh {Id} để chuyển trạng thái.", id);
                return false;
            }

            // Chặn đề tài NCKH (IsGraduationCapstone = false) tham gia quy trình bảo vệ tốt nghiệp
            if ((newStatus == ThesisStatus.SubmittedForReview || newStatus == ThesisStatus.Defended) && !thesis.IsGraduationCapstone)
            {
                _logger.LogWarning("Đề tài NCKH {Code} không thể chuyển sang trạng thái bảo vệ {NewStatus}.", thesis.Code, newStatus);
                throw new InvalidOperationException("Đề tài nghiên cứu khoa học không thuộc đối tượng bảo vệ tốt nghiệp theo quy định của Module M05.");
            }

            var oldStatus = thesis.ThesisStatus;
            thesis.ThesisStatus = newStatus;
            _unitOfWork.Theses.Update(thesis);

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
                    Action = newStatus == ThesisStatus.Approved ? "ApproveThesis" : "TransitionStatus",
                    EntityName = "Thesis",
                    EntityId = thesis.Id.ToString(),
                    Details = $"Chuyển trạng thái đề tài [{thesis.Code}] từ '{oldStatus}' sang '{newStatus}'.",
                    Timestamp = DateTime.UtcNow
                };
                await _unitOfWork.AuditLogs.AddAsync(auditLog);
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Chuyển trạng thái đề tài {Code} từ {OldStatus} sang {NewStatus}",
                thesis.Code, oldStatus, newStatus);

            return true;
        }

        /// <summary>
        /// Thẩm định điều kiện tham gia phiên bảo vệ tốt nghiệp của đề tài.
        /// QUY TẮC BẮT BUỘC (CRITICAL RULE):
        /// 1. Kiểm tra thesis.IsGraduationCapstone == true. Nếu là đề tài NCKH thông thường (false),
        /// chặn tuyệt đối với thông báo lỗi quy chuẩn của Module M05:
        /// "Đề tài nghiên cứu khoa học không thuộc đối tượng bảo vệ tốt nghiệp theo quy định của Module M05."
        /// 2. Kiểm tra xung đột lợi ích: Người hướng dẫn không được đồng thời là phản biện hoặc ủy viên hội đồng.
        /// </summary>
        public async Task<bool> ValidateForDefenseAsync(int id)
        {
            var thesis = await _unitOfWork.Theses.GetByIdAsync(id);
            if (thesis == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đề tài có mã định danh {id}.");
            }

            // QUY TẮC CỐT LÕI: Chặn đề tài NCKH tham gia luồng bảo vệ tốt nghiệp
            if (!thesis.IsGraduationCapstone)
            {
                _logger.LogWarning("Đề tài {Code} là đề tài NCKH (IsGraduationCapstone = false), bị từ chối vào luồng bảo vệ M05.", thesis.Code);
                throw new InvalidOperationException("Đề tài nghiên cứu khoa học không thuộc đối tượng bảo vệ tốt nghiệp theo quy định của Module M05.");
            }

            // Kiểm tra trạng thái đề tài phải ở giai đoạn sẵn sàng bảo vệ
            if (thesis.ThesisStatus == ThesisStatus.Draft || thesis.ThesisStatus == ThesisStatus.Registered)
            {
                throw new InvalidOperationException($"Đề tài {thesis.Code} đang ở trạng thái '{thesis.ThesisStatus}', chưa được phê duyệt đề cương hoặc chưa hoàn thành giai đoạn nghiên cứu.");
            }

            // Kiểm tra xung đột lợi ích giữa người hướng dẫn và thành viên phản biện / hội đồng
            if (_unitOfWork.ThesisAssignments != null)
            {
                var assignments = (await _unitOfWork.ThesisAssignments.FindAsync(a => a.ThesisId == id))?.ToList() ?? new List<ThesisAssignment>();
                var supervisorPersonIds = assignments.Where(a => a.Role == AssignmentRole.Supervisor).Select(a => a.PersonId).ToHashSet();
                var reviewerOrCommitteeIds = assignments.Where(a => a.Role == AssignmentRole.Reviewer || a.Role == AssignmentRole.Committee).Select(a => a.PersonId).ToHashSet();
                if (supervisorPersonIds.Overlaps(reviewerOrCommitteeIds))
                {
                    throw new InvalidOperationException("Xung đột lợi ích: Phát hiện cán bộ hướng dẫn đồng thời là cán bộ phản biện hoặc thành viên hội đồng đánh giá đề tài.");
                }
            }

            return true;
        }
    }
}
