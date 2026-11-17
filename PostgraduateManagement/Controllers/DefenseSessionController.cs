// File: Controllers/DefenseSessionController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Repositories.Interfaces;
using PostgraduateManagement.Services.DTOs;
using PostgraduateManagement.Services.Interfaces;
using PostgraduateManagement.ViewModels;

namespace PostgraduateManagement.Controllers
{
    /// <summary>
    /// Controller quản lý tổ chức phiên bảo vệ đề tài sau đại học và ghi nhận kết quả đánh giá.
    /// Thực thi quy tắc thẩm định ValidateForDefenseAsync chặn đề tài NCKH tham gia phiên bảo vệ tốt nghiệp.
    /// Lập lịch và Ghi nhận kết quả chỉ được thực hiện bởi Quản lý (Manager), Ban quản trị (Admin) hoặc Ủy viên hội đồng được phân công.
    /// </summary>
    [Authorize]
    public class DefenseSessionController : Controller
    {
        private readonly IDefenseAndDocumentService _defenseService;
        private readonly IThesisService _thesisService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DefenseSessionController> _logger;

        public DefenseSessionController(
            IDefenseAndDocumentService defenseService,
            IThesisService thesisService,
            IUnitOfWork unitOfWork,
            ILogger<DefenseSessionController> logger)
        {
            _defenseService = defenseService;
            _thesisService = thesisService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Hiển thị danh sách các phiên bảo vệ đề tài sau đại học.
        /// YÊU CẦU BẮT BUỘC (CRITICAL): Chỉ định tường minh đường dẫn view:
        /// return View("~/Views/Defense/Index.cshtml", model);
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? search, DefenseStatus? status)
        {
            var sessions = await _unitOfWork.DefenseSessions.GetAllAsync();
            var detailedSessions = new List<DefenseSessionItemViewModel>();

            foreach (var s in sessions)
            {
                var full = await _unitOfWork.DefenseSessions.GetSessionWithDetailsAsync(s.Id);
                if (full == null) continue;

                var student = full.Thesis?.Student;
                var item = new DefenseSessionItemViewModel
                {
                    Id = full.Id,
                    ThesisId = full.ThesisId,
                    ThesisCode = full.Thesis?.Code ?? string.Empty,
                    ThesisTitle = full.Thesis?.Title ?? string.Empty,
                    ThesisType = full.Thesis?.ThesisType ?? ThesisType.LuanVan,
                    Field = full.Thesis?.Field ?? string.Empty,
                    StudentName = student?.FullName ?? string.Empty,
                    StudentCode = student?.SharedStudentCode ?? string.Empty,
                    ScheduledAt = full.ScheduledAt,
                    Location = full.Location,
                    Status = full.Status,
                    CommitteeNotes = full.CommitteeNotes,
                    Result = full.DefenseResult != null ? new DefenseResultItemViewModel
                    {
                        Id = full.DefenseResult.Id,
                        SessionId = full.DefenseResult.SessionId,
                        Score = full.DefenseResult.Score,
                        Pass = full.DefenseResult.Pass,
                        Comments = full.DefenseResult.Comments,
                        RecordedBy = full.DefenseResult.RecordedBy,
                        RecordedAt = full.DefenseResult.RecordedAt
                    } : null
                };

                detailedSessions.Add(item);
            }

            // Áp dụng tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                detailedSessions = detailedSessions.Where(s =>
                    s.ThesisCode.ToLowerInvariant().Contains(term) ||
                    s.ThesisTitle.ToLowerInvariant().Contains(term) ||
                    s.StudentName.ToLowerInvariant().Contains(term) ||
                    s.Location.ToLowerInvariant().Contains(term) ||
                    s.StudentCode.ToLowerInvariant().Contains(term)
                ).ToList();
            }

            // Áp dụng bộ lọc trạng thái
            if (status.HasValue)
            {
                detailedSessions = detailedSessions.Where(s => s.Status == status.Value).ToList();
            }

            // Sắp xếp theo ngày bảo vệ
            detailedSessions = detailedSessions.OrderByDescending(s => s.ScheduledAt).ToList();

            var model = new DefenseIndexViewModel
            {
                Search = search,
                Status = status,
                TotalCount = detailedSessions.Count,
                Sessions = detailedSessions,
                StatusOptions = GetDefenseStatusOptions(status)
            };

            // CHỈ ĐỊNH TƯỜNG MINH ĐƯỜNG DẪN VIEW THEO ĐẶC TẢ
            return View("~/Views/Defense/Index.cshtml", model);
        }

        /// <summary>
        /// Giao diện lập lịch phiên bảo vệ tốt nghiệp.
        /// Thẩm định đề tài đủ điều kiện và chặn đề tài NCKH.
        /// Chỉ Manager, Admin hoặc thành viên hội đồng được phân công mới có quyền.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Schedule(int? thesisId)
        {
            if (thesisId.HasValue && !await CanManageDefenseAsync(thesisId.Value))
            {
                _logger.LogWarning("Từ chối truy cập: Người dùng '{User}' không có quyền lập lịch bảo vệ cho đề tài {ThesisId}", User.Identity?.Name, thesisId.Value);
                return Forbid();
            }
            else if (!thesisId.HasValue && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                return Forbid();
            }

            var model = new ScheduleDefenseViewModel
            {
                ScheduledAt = DateTime.UtcNow.Date.AddDays(7).AddHours(9),
                Location = "Phòng Bảo vệ Sau đại học A1-302"
            };

            if (thesisId.HasValue)
            {
                try
                {
                    // KIỂM TRA ĐIỀU KIỆN TIÊN QUYẾT: Chặn đề tài NCKH (IsGraduationCapstone == false)
                    await _thesisService.ValidateForDefenseAsync(thesisId.Value);

                    var thesis = await _unitOfWork.Theses.GetThesisWithFullDetailsAsync(thesisId.Value);
                    if (thesis != null)
                    {
                        model.ThesisId = thesis.Id;
                        model.ThesisCode = thesis.Code;
                        model.ThesisTitle = thesis.Title;
                        model.StudentName = thesis.Student?.FullName;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Đề tài {ThesisId} không đủ điều kiện lên lịch bảo vệ: {Message}", thesisId.Value, ex.Message);
                    TempData["ErrorMessage"] = ex.Message;
                }
            }

            await PopulateAvailableThesesDropdownAsync(model);
            return View("~/Views/Defense/Schedule.cshtml", model);
        }

        /// <summary>
        /// Xử lý lập lịch phiên bảo vệ đề tài sau đại học.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(ScheduleDefenseViewModel model)
        {
            if (!await CanManageDefenseAsync(model.ThesisId))
            {
                _logger.LogWarning("Từ chối truy cập: Người dùng '{User}' không có quyền lập lịch bảo vệ cho đề tài {ThesisId}", User.Identity?.Name, model.ThesisId);
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                await PopulateAvailableThesesDropdownAsync(model);
                return View("~/Views/Defense/Schedule.cshtml", model);
            }

            try
            {
                var dto = new ScheduleDefenseDto
                {
                    ThesisId = model.ThesisId,
                    ScheduledAt = model.ScheduledAt,
                    Location = model.Location,
                    CommitteeNotes = model.CommitteeNotes
                };

                var session = await _defenseService.ScheduleDefenseAsync(dto);
                TempData["SuccessMessage"] = $"Lập lịch bảo vệ thành công cho đề tài [{session.ThesisCode}] vào lúc {session.ScheduledAt:dd/MM/yyyy HH:mm} tại {session.Location}.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Từ chối lập lịch bảo vệ: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateAvailableThesesDropdownAsync(model);
                return View("~/Views/Defense/Schedule.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lập lịch bảo vệ: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, $"Lỗi hệ thống: {ex.Message}");
                await PopulateAvailableThesesDropdownAsync(model);
                return View("~/Views/Defense/Schedule.cshtml", model);
            }
        }

        /// <summary>
        /// Giao diện ghi nhận điểm số và kết quả đánh giá của hội đồng bảo vệ.
        /// Chỉ Manager, Admin hoặc thành viên hội đồng được phân công mới có quyền.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RecordResult(int sessionId)
        {
            var session = await _unitOfWork.DefenseSessions.GetSessionWithDetailsAsync(sessionId);
            if (session == null)
            {
                _logger.LogWarning("Không tìm thấy phiên bảo vệ với ID = {SessionId}", sessionId);
                return NotFound();
            }

            if (!await CanManageDefenseAsync(session.ThesisId))
            {
                _logger.LogWarning("Từ chối truy cập: Người dùng '{User}' không có quyền ghi nhận kết quả bảo vệ phiên {SessionId}", User.Identity?.Name, sessionId);
                return Forbid();
            }

            var model = new RecordDefenseResultViewModel
            {
                SessionId = session.Id,
                ThesisId = session.ThesisId,
                ThesisCode = session.Thesis?.Code,
                ThesisTitle = session.Thesis?.Title,
                StudentName = session.Thesis?.Student?.FullName,
                ScheduledAt = session.ScheduledAt,
                Location = session.Location,
                Score = session.DefenseResult?.Score ?? 8.0m,
                Pass = session.DefenseResult?.Pass ?? true,
                Comments = session.DefenseResult?.Comments ?? "Hội đồng đánh giá đề tài đáp ứng đầy đủ yêu cầu của một công trình nghiên cứu sau đại học."
            };

            FillMemberComments(model, session);
            return View("~/Views/Defense/RecordResult.cshtml", model);
        }

        /// <summary>
        /// Ghi nhận (hoặc cập nhật) nhận xét riêng của một phản biện / thành viên hội đồng trong phiên bảo vệ.
        /// Người nhận xét phải đang được phân công Phản biện hoặc Hội đồng cho đề tài.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMemberComment(int sessionId, int personId, string content, decimal? score)
        {
            var session = await _unitOfWork.DefenseSessions.GetSessionWithDetailsAsync(sessionId);
            if (session == null)
            {
                return NotFound();
            }

            if (!await CanManageDefenseAsync(session.ThesisId))
            {
                return Forbid();
            }

            var isMember = session.Thesis?.Assignments.Any(a => a.PersonId == personId &&
                (a.Role == AssignmentRole.Reviewer || a.Role == AssignmentRole.Committee)) ?? false;
            if (!isMember)
            {
                TempData["ErrorMessage"] = "Người nhận xét phải là phản biện hoặc thành viên hội đồng của đề tài.";
            }
            else if (string.IsNullOrWhiteSpace(content) || content.Length > 2000)
            {
                TempData["ErrorMessage"] = "Nội dung nhận xét không được để trống và tối đa 2000 ký tự.";
            }
            else if (score is < 0 or > 10)
            {
                TempData["ErrorMessage"] = "Điểm phải nằm trong thang 0 - 10.";
            }
            else
            {
                var recorder = User.Identity?.Name ?? "Thư ký Hội đồng";
                var existing = session.MemberComments.FirstOrDefault(c => c.PersonId == personId);
                if (existing != null)
                {
                    existing.Content = content.Trim();
                    existing.Score = score;
                    existing.RecordedBy = recorder;
                    existing.RecordedAt = DateTime.UtcNow;
                    _unitOfWork.DefenseComments.Update(existing);
                }
                else
                {
                    await _unitOfWork.DefenseComments.AddAsync(new Models.Entities.DefenseComment
                    {
                        SessionId = sessionId,
                        PersonId = personId,
                        Content = content.Trim(),
                        Score = score,
                        RecordedBy = recorder
                    });
                }
                await _unitOfWork.CompleteAsync();
                TempData["SuccessMessage"] = "Đã lưu nhận xét của thành viên.";
            }

            return RedirectToAction(nameof(RecordResult), new { sessionId });
        }

        /// <summary>
        /// Xử lý ghi nhận điểm số và kết luận đánh giá của hội đồng bảo vệ.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordResult(RecordDefenseResultViewModel model)
        {
            var session = await _unitOfWork.DefenseSessions.GetSessionWithDetailsAsync(model.SessionId);
            if (session == null)
            {
                return NotFound();
            }

            if (!await CanManageDefenseAsync(session.ThesisId))
            {
                _logger.LogWarning("Từ chối truy cập: Người dùng '{User}' không có quyền ghi nhận kết quả bảo vệ phiên {SessionId}", User.Identity?.Name, model.SessionId);
                return Forbid();
            }

            FillMemberComments(model, session);

            if (!ModelState.IsValid)
            {
                return View("~/Views/Defense/RecordResult.cshtml", model);
            }

            try
            {
                var dto = new RecordDefenseResultDto
                {
                    SessionId = model.SessionId,
                    Score = model.Score,
                    Pass = model.Pass,
                    Comments = model.Comments
                };

                var recorder = User.Identity?.Name ?? "Thư ký Hội đồng Sau đại học";
                var result = await _defenseService.RecordDefenseResultAsync(dto, recorder);

                TempData["SuccessMessage"] = $"Ghi nhận kết quả bảo vệ thành công: Điểm số {result.Score:F1} - Kết quả: {(result.Pass ? "Đạt" : "Không đạt")}.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi ghi nhận kết quả bảo vệ: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, $"Lỗi ghi nhận: {ex.Message}");
                return View("~/Views/Defense/RecordResult.cshtml", model);
            }
        }

        #region Helper Methods

        /// <summary>
        /// Nạp nhận xét từng thành viên và danh sách phản biện / hội đồng của đề tài vào view model.
        /// </summary>
        private static void FillMemberComments(RecordDefenseResultViewModel model, Models.Entities.DefenseSession session)
        {
            model.MemberComments = session.MemberComments.OrderBy(c => c.RecordedAt).ToList();

            var assignments = session.Thesis?.Assignments
                .Where(a => a.Role == AssignmentRole.Reviewer || a.Role == AssignmentRole.Committee)
                ?? Enumerable.Empty<Models.Entities.ThesisAssignment>();

            model.CommentablePersons = assignments
                .GroupBy(a => a.PersonId)
                .Select(g => new SelectListItem
                {
                    Value = g.Key.ToString(),
                    Text = $"{g.First().Person?.FullName} ({string.Join(", ", g.Select(a => new AssignmentItemViewModel { Role = a.Role, CommitteePosition = a.CommitteePosition }.RoleName))})"
                })
                .ToList();
        }

        /// <summary>
        /// Kiểm tra quyền quản lý phiên bảo vệ (Lập lịch / Ghi nhận kết quả):
        /// - Admin / Manager: Toàn quyền.
        /// - Committee: Được phân công làm Phản biện hoặc Hội đồng trên đề tài này.
        /// </summary>
        private async Task<bool> CanManageDefenseAsync(int thesisId)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                return true;
            }

            if (User.IsInRole("Committee"))
            {
                var userEmail = User.Identity?.Name ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var assignments = (await _unitOfWork.ThesisAssignments.FindAsync(
                        a => a.ThesisId == thesisId && (a.Role == AssignmentRole.Committee || a.Role == AssignmentRole.Reviewer))).ToList();

                    foreach (var a in assignments)
                    {
                        var person = a.Person ?? await _unitOfWork.AcademicPersons.GetByIdAsync(a.PersonId);
                        if (person != null && string.Equals(person.Email, userEmail, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private async Task PopulateAvailableThesesDropdownAsync(ScheduleDefenseViewModel model)
        {
            // Chỉ lấy các đề tài tốt nghiệp sau đại học (IsGraduationCapstone == true) và ở trạng thái hợp lệ
            var theses = (await _unitOfWork.Theses.GetAllAsync())
                .Where(t => t.IsGraduationCapstone && t.ThesisStatus != ThesisStatus.Draft && t.ThesisStatus != ThesisStatus.Registered)
                .OrderBy(t => t.Code)
                .ToList();

            model.AvailableTheses = theses.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"[{t.Code}] {t.Title} (Trạng thái: {t.ThesisStatus})",
                Selected = t.Id == model.ThesisId
            }).ToList();
        }

        private static List<SelectListItem> GetDefenseStatusOptions(DefenseStatus? selectedStatus)
        {
            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Tất cả trạng thái --", Selected = !selectedStatus.HasValue }
            };

            foreach (DefenseStatus status in Enum.GetValues(typeof(DefenseStatus)))
            {
                var text = status switch
                {
                    DefenseStatus.Scheduled => "Đã lên lịch",
                    DefenseStatus.InProgress => "Đang diễn ra",
                    DefenseStatus.Defended => "Đã bảo vệ",
                    DefenseStatus.Cancelled => "Đã hủy",
                    _ => status.ToString()
                };

                list.Add(new SelectListItem
                {
                    Value = ((int)status).ToString(),
                    Text = text,
                    Selected = selectedStatus.HasValue && selectedStatus.Value == status
                });
            }

            return list;
        }

        #endregion
    }
}
