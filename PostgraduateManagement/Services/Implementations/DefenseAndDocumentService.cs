// File: Services/Implementations/DefenseAndDocumentService.cs
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
    /// Lớp triển khai dịch vụ tổ chức bảo vệ đề tài và số hóa tài liệu (DefenseAndDocumentService).
    /// Quản lý lập lịch bảo vệ (kiểm tra ValidateForDefenseAsync), ghi nhận kết quả đánh giá của hội đồng;
    /// nộp tài liệu số hóa giới hạn trong 3 loại bắt buộc (Thuyết minh, Toàn văn, Biên bản sửa đổi);
    /// tính toán tiến độ số hóa tài liệu strictly trong phạm vi danh mục Module M05.
    /// Ghi vết kiểm toán (Audit Logging) cho kết quả bảo vệ và nộp/xác nhận tài liệu.
    /// </summary>
    public class DefenseAndDocumentService : IDefenseAndDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IThesisService _thesisService;
        private readonly ILogger<DefenseAndDocumentService> _logger;
        private readonly IHttpContextAccessor? _httpContextAccessor;

        private const string DigitizationDisclaimer = "Tỷ lệ số hóa này chỉ áp dụng trong phạm vi danh mục 03 tài liệu bắt buộc của Module M05, không suy diễn toàn bộ quy trình đào tạo sau đại học đạt 100%.";

        public DefenseAndDocumentService(
            IUnitOfWork unitOfWork,
            IThesisService thesisService,
            ILogger<DefenseAndDocumentService> logger,
            IHttpContextAccessor? httpContextAccessor = null)
        {
            _unitOfWork = unitOfWork;
            _thesisService = thesisService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Lập lịch phiên bảo vệ đề tài sau đại học.
        /// BẮT BUỘC: Thẩm định điều kiện bảo vệ qua ThesisService.ValidateForDefenseAsync trước khi lên lịch.
        /// Chặn đề tài NCKH (IsGraduationCapstone == false) vào luồng bảo vệ.
        /// </summary>
        public async Task<DefenseSessionDto> ScheduleDefenseAsync(ScheduleDefenseDto dto)
        {
            // BƯỚC 1: Thẩm định điều kiện bảo vệ đề tài (chặn đề tài NCKH, kiểm tra trạng thái)
            await _thesisService.ValidateForDefenseAsync(dto.ThesisId);

            var thesis = await _unitOfWork.Theses.GetByIdAsync(dto.ThesisId);
            if (thesis == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đề tài với mã định danh {dto.ThesisId}.");
            }

            var student = thesis.Student ?? await _unitOfWork.PostgraduateStudents.GetByIdAsync(thesis.StudentId);

            // BƯỚC 2: Tạo phiên bảo vệ mới
            var session = new DefenseSession
            {
                ThesisId = dto.ThesisId,
                ScheduledAt = dto.ScheduledAt,
                Location = dto.Location.Trim(),
                Status = DefenseStatus.Scheduled,
                CommitteeNotes = dto.CommitteeNotes?.Trim()
            };

            await _unitOfWork.DefenseSessions.AddAsync(session);

            // BƯỚC 3: Cập nhật trạng thái đề tài sang SubmittedForReview nếu chưa chuyển
            if (thesis.ThesisStatus == ThesisStatus.Approved || thesis.ThesisStatus == ThesisStatus.InProgress)
            {
                thesis.ThesisStatus = ThesisStatus.SubmittedForReview;
                _unitOfWork.Theses.Update(thesis);
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Lập lịch phiên bảo vệ thành công cho đề tài {Code} vào lúc {Time} tại {Location}",
                thesis.Code, session.ScheduledAt, session.Location);

            return new DefenseSessionDto
            {
                Id = session.Id,
                ThesisId = session.ThesisId,
                ThesisCode = thesis.Code,
                ThesisTitle = thesis.Title,
                ThesisType = thesis.ThesisType,
                Field = thesis.Field,
                StudentName = student?.FullName ?? string.Empty,
                StudentCode = student?.SharedStudentCode ?? string.Empty,
                ScheduledAt = session.ScheduledAt,
                Location = session.Location,
                Status = session.Status,
                CommitteeNotes = session.CommitteeNotes
            };
        }

        /// <summary>
        /// Ghi nhận kết quả đánh giá, điểm số và nhận xét của hội đồng bảo vệ.
        /// </summary>
        public async Task<DefenseResultDto> RecordDefenseResultAsync(RecordDefenseResultDto dto, string recordedBy)
        {
            var session = await _unitOfWork.DefenseSessions.GetSessionWithDetailsAsync(dto.SessionId);
            if (session == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy phiên bảo vệ với mã định danh {dto.SessionId}.");
            }

            // Tạo bản ghi kết quả bảo vệ
            var result = new DefenseResult
            {
                SessionId = session.Id,
                Score = dto.Score,
                Pass = dto.Pass,
                Comments = dto.Comments.Trim(),
                RecordedBy = string.IsNullOrWhiteSpace(recordedBy) ? "Thư ký hội đồng" : recordedBy.Trim(),
                RecordedAt = DateTime.UtcNow
            };

            await _unitOfWork.DefenseResults.AddAsync(result);

            // Cập nhật trạng thái phiên bảo vệ sang Defended
            session.Status = DefenseStatus.Defended;
            _unitOfWork.DefenseSessions.Update(session);

            // Cập nhật trạng thái đề tài nếu đạt yêu cầu
            var thesis = session.Thesis ?? await _unitOfWork.Theses.GetByIdAsync(session.ThesisId);
            if (thesis != null && dto.Pass)
            {
                thesis.ThesisStatus = ThesisStatus.Defended;
                _unitOfWork.Theses.Update(thesis);
            }

            // Ghi vết kiểm toán (Audit Logging)
            var user = _httpContextAccessor?.HttpContext?.User;
            var userId = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userName = !string.IsNullOrWhiteSpace(recordedBy) ? recordedBy.Trim() : (user?.Identity?.Name ?? "System");

            if (_unitOfWork.AuditLogs != null)
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    UserName = userName,
                    Action = "RecordDefenseResult",
                    EntityName = "DefenseResult",
                    EntityId = session.Id.ToString(),
                    Details = $"Ghi nhận kết quả bảo vệ phiên #{session.Id} cho đề tài '{thesis?.Code}': Điểm {result.Score:F1}, Kết quả: {(result.Pass ? "Đạt" : "Không đạt")}.",
                    Timestamp = DateTime.UtcNow
                };
                await _unitOfWork.AuditLogs.AddAsync(auditLog);
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Ghi nhận kết quả bảo vệ phiên {SessionId}: Điểm {Score}, Kết quả {Pass} cho đề tài {Code}",
                session.Id, result.Score, result.Pass ? "Đạt" : "Không đạt", thesis?.Code);

            return new DefenseResultDto
            {
                Id = result.Id,
                SessionId = result.SessionId,
                Comments = result.Comments,
                Score = result.Score,
                Pass = result.Pass,
                RecordedBy = result.RecordedBy,
                RecordedAt = result.RecordedAt
            };
        }

        /// <summary>
        /// Tải lên tài liệu số hóa nộp lưu chiểu cho đề tài sau đại học.
        /// Giới hạn nghiêm ngặt trong 3 loại tài liệu bắt buộc của Module M05:
        /// Thuyết minh, Toàn văn, Biên bản sửa đổi.
        /// Ghi vết kiểm toán (Audit Logging).
        /// </summary>
        public async Task<SubmittedDocumentDto> UploadDocumentAsync(UploadDocumentDto dto)
        {
            var thesis = await _unitOfWork.Theses.GetByIdAsync(dto.ThesisId);
            if (thesis == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đề tài với mã định danh {dto.ThesisId}.");
            }

            // KIỂM TRA RANH GIỚI: Loại tài liệu bắt buộc phải nằm trong danh mục M05
            if (!Enum.IsDefined(typeof(DocumentType), dto.DocumentType))
            {
                throw new ArgumentException("Loại tài liệu không hợp lệ. Module M05 chỉ chấp nhận 03 loại tài liệu bắt buộc: Thuyết minh, Toàn văn, Biên bản sửa đổi.");
            }

            // Tự động xác định số phiên bản tiếp theo cho cùng loại tài liệu
            var latestDoc = await _unitOfWork.SubmittedDocuments.GetLatestVersionAsync(dto.ThesisId, dto.DocumentType);
            var nextVersion = (latestDoc?.Version ?? 0) + 1;

            var document = new SubmittedDocument
            {
                ThesisId = dto.ThesisId,
                DocumentType = dto.DocumentType,
                FileName = dto.FileName.Trim(),
                FilePath = dto.FilePath.Trim(),
                Sha256Hash = dto.Sha256Hash?.Trim(),
                Version = nextVersion,
                UploadedAt = DateTime.UtcNow
            };

            await _unitOfWork.SubmittedDocuments.AddAsync(document);

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
                    Action = "UploadDocument",
                    EntityName = "SubmittedDocument",
                    EntityId = dto.ThesisId.ToString(),
                    Details = $"Tải lên tài liệu số hóa '{document.FileName}' (Loại: {document.DocumentType}, v{document.Version}) cho đề tài '{thesis.Code}'.",
                    Timestamp = DateTime.UtcNow
                };
                await _unitOfWork.AuditLogs.AddAsync(auditLog);
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Tải lên tài liệu số hóa: Đề tài {Code}, Loại {DocType}, Phiên bản v{Ver}",
                thesis.Code, document.DocumentType, document.Version);

            return new SubmittedDocumentDto
            {
                Id = document.Id,
                ThesisId = document.ThesisId,
                DocumentType = document.DocumentType,
                FileName = document.FileName,
                FilePath = document.FilePath,
                Sha256Hash = document.Sha256Hash,
                Version = document.Version,
                UploadedAt = document.UploadedAt,
                ConfirmedBy = document.ConfirmedBy
            };
        }

        /// <summary>
        /// Xác nhận phê duyệt tính hợp lệ của tài liệu số hóa đã nộp.
        /// Ghi vết kiểm toán (Audit Logging).
        /// </summary>
        public async Task<bool> ConfirmDocumentAsync(int documentId, string confirmedBy)
        {
            var document = await _unitOfWork.SubmittedDocuments.GetByIdAsync(documentId);
            if (document == null)
            {
                _logger.LogWarning("Không tìm thấy tài liệu số hóa có mã định danh {DocumentId}.", documentId);
                return false;
            }

            document.ConfirmedBy = string.IsNullOrWhiteSpace(confirmedBy) ? "Hội đồng sau đại học" : confirmedBy.Trim();
            _unitOfWork.SubmittedDocuments.Update(document);

            // Kiểm tra nếu đề tài đã Defended và cả 3 loại tài liệu bắt buộc đều đã được nộp và duyệt,
            // hoàn tất vòng đời đề tài (chuyển sang Completed)
            var thesis = await _unitOfWork.Theses.GetByIdAsync(document.ThesisId);
            if (thesis != null && thesis.ThesisStatus == ThesisStatus.Defended)
            {
                var progress = await GetDigitizationProgressAsync(thesis.Id);
                if (progress.ConfirmedDocumentsCount == 3)
                {
                    thesis.ThesisStatus = ThesisStatus.Completed;
                    _unitOfWork.Theses.Update(thesis);
                    _logger.LogInformation("Đề tài {Code} hoàn tất toàn bộ quy trình nộp số hóa lưu chiểu, chuyển sang Completed.", thesis.Code);
                }
            }

            // Ghi vết kiểm toán (Audit Logging)
            var user = _httpContextAccessor?.HttpContext?.User;
            var userId = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userName = !string.IsNullOrWhiteSpace(confirmedBy) ? confirmedBy.Trim() : (user?.Identity?.Name ?? "System");

            if (_unitOfWork.AuditLogs != null)
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    UserName = userName,
                    Action = "ConfirmDocument",
                    EntityName = "SubmittedDocument",
                    EntityId = document.Id.ToString(),
                    Details = $"Xác nhận phê duyệt tài liệu số hóa #{document.Id} ('{document.FileName}', loại {document.DocumentType} v{document.Version}) cho đề tài '{thesis?.Code}' bởi '{document.ConfirmedBy}'.",
                    Timestamp = DateTime.UtcNow
                };
                await _unitOfWork.AuditLogs.AddAsync(auditLog);
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Xác nhận tài liệu số hóa #{DocId} thành công bởi {ConfirmedBy}",
                document.Id, document.ConfirmedBy);

            return true;
        }

        /// <summary>
        /// Báo cáo tiến độ số hóa tài liệu của đề tài sau đại học.
        /// QUY TẮC RANH GIỚI BẮT BUỘC:
        /// Tỷ lệ phần trăm tính STRICTLY trên 03 loại tài liệu bắt buộc của Module M05:
        /// 3/3 = 100%, 2/3 = 66.7%, 1/3 = 33.3%, 0/3 = 0%.
        /// Đi kèm thông điệp cảnh báo không suy diễn toàn bộ quy trình đào tạo sau đại học đạt 100%.
        /// </summary>
        public async Task<DigitizationProgressDto> GetDigitizationProgressAsync(int thesisId)
        {
            var thesis = await _unitOfWork.Theses.GetByIdAsync(thesisId);
            if (thesis == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đề tài với mã định danh {thesisId}.");
            }

            var allDocs = (await _unitOfWork.SubmittedDocuments.GetByThesisIdAsync(thesisId)).ToList();

            var hasThuyetMinh = allDocs.Any(d => d.DocumentType == DocumentType.ThuyetMinh);
            var hasToanVan = allDocs.Any(d => d.DocumentType == DocumentType.ToanVan);
            var hasBienBanSuaDoi = allDocs.Any(d => d.DocumentType == DocumentType.BienBanSuaDoi);

            var uploadedCount = (hasThuyetMinh ? 1 : 0) + (hasToanVan ? 1 : 0) + (hasBienBanSuaDoi ? 1 : 0);

            var confirmedCount = allDocs
                .Where(d => !string.IsNullOrEmpty(d.ConfirmedBy))
                .Select(d => d.DocumentType)
                .Distinct()
                .Count();

            // Tính tỷ lệ nghiêm ngặt theo 3 loại tài liệu bắt buộc M05
            var percentage = uploadedCount switch
            {
                3 => 100.0,
                2 => 66.7,
                1 => 33.3,
                _ => 0.0
            };

            var docDtos = allDocs.Select(d => new SubmittedDocumentDto
            {
                Id = d.Id,
                ThesisId = d.ThesisId,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                FilePath = d.FilePath,
                Sha256Hash = d.Sha256Hash,
                Version = d.Version,
                UploadedAt = d.UploadedAt,
                ConfirmedBy = d.ConfirmedBy
            }).ToList();

            return new DigitizationProgressDto
            {
                ThesisId = thesis.Id,
                ThesisCode = thesis.Code,
                ThesisTitle = thesis.Title,
                TotalRequiredDocuments = 3,
                UploadedDocumentsCount = uploadedCount,
                ConfirmedDocumentsCount = confirmedCount,
                ProgressPercentage = percentage,
                HasThuyetMinh = hasThuyetMinh,
                HasToanVan = hasToanVan,
                HasBienBanSuaDoi = hasBienBanSuaDoi,
                DisclaimerNote = DigitizationDisclaimer,
                UploadedDocuments = docDtos
            };
        }
    }
}
