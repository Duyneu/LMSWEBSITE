// File: Controllers/SupervisorAssignmentController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Repositories.Interfaces;
using PostgraduateManagement.Services.Interfaces;
using PostgraduateManagement.ViewModels;

namespace PostgraduateManagement.Controllers
{
    /// <summary>
    /// Controller quản lý phân công cán bộ hướng dẫn, phản biện và hội đồng đánh giá đề tài sau đại học.
    /// Tích hợp cơ chế kiểm tra điều kiện học vị/lĩnh vực (EligibilityCheck) và gợi ý phản biện AI độc lập.
    /// Phân công thành viên chỉ được thực hiện bởi Quản lý (Manager) hoặc Ban quản trị (Admin).
    /// </summary>
    [Authorize]
    public class SupervisorAssignmentController : Controller
    {
        private readonly IEligibilityAndAssignmentService _assignmentService;
        private readonly IThesisService _thesisService;
        private readonly IAiReviewerSuggestionService _aiSuggestionService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SupervisorAssignmentController> _logger;

        public SupervisorAssignmentController(
            IEligibilityAndAssignmentService assignmentService,
            IThesisService thesisService,
            IAiReviewerSuggestionService aiSuggestionService,
            IUnitOfWork unitOfWork,
            ILogger<SupervisorAssignmentController> logger)
        {
            _assignmentService = assignmentService;
            _thesisService = thesisService;
            _aiSuggestionService = aiSuggestionService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Hiển thị màn hình trung tâm phân công chuyên gia cho đề tài sau đại học.
        /// YÊU CẦU BẮT BUỘC (CRITICAL): Chỉ định tường minh đường dẫn view:
        /// return View("~/Views/Assignment/Index.cshtml", model);
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int? thesisId)
        {
            var allTheses = (await _unitOfWork.Theses.GetAllAsync()).OrderBy(t => t.Code).ToList();

            if (!thesisId.HasValue && allTheses.Any())
            {
                thesisId = allTheses.First().Id;
            }

            var model = new AssignmentIndexViewModel
            {
                SelectedThesisId = thesisId,
                AvailableTheses = allTheses.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"[{t.Code}] {t.Title}",
                    Selected = thesisId.HasValue && t.Id == thesisId.Value
                }).ToList()
            };

            if (thesisId.HasValue)
            {
                var thesis = await _thesisService.GetThesisByIdAsync(thesisId.Value);
                if (thesis != null)
                {
                    model.ThesisCode = thesis.Code;
                    model.ThesisTitle = thesis.Title;
                    model.ThesisType = thesis.ThesisType;
                    model.ThesisTypeName = thesis.ThesisTypeName;
                    model.ThesisField = thesis.Field;
                    model.StudentName = thesis.StudentName;
                    model.StudentCode = thesis.StudentCode;

                    // Lấy danh sách phân công hiện tại
                    var currentAssignments = await _assignmentService.GetAssignmentsByThesisAsync(thesisId.Value);
                    model.CurrentAssignments = currentAssignments.Select(a => new AssignmentItemViewModel
                    {
                        Id = a.Id,
                        ThesisId = a.ThesisId,
                        PersonId = a.PersonId,
                        PersonName = a.PersonName,
                        SharedPersonCode = a.SharedPersonCode,
                        AcademicDegree = a.AcademicDegree,
                        ResearchField = a.ResearchField,
                        Workplace = a.Workplace,
                        Role = a.Role,
                        IsExternal = a.IsExternal,
                        AssignedAt = a.AssignedAt,
                        Notes = a.Notes
                    }).ToList();

                    // Lấy danh sách ứng viên chuyên gia
                    var allPersons = await _unitOfWork.AcademicPersons.GetAllAsync();
                    model.CandidatePersons = allPersons.Select(p => new CandidatePersonViewModel
                    {
                        Id = p.Id,
                        FullName = p.FullName,
                        SharedPersonCode = p.SharedPersonCode,
                        AcademicDegree = p.AcademicDegree,
                        ResearchField = p.ResearchField,
                        Workplace = p.Workplace,
                        IsExternal = p.IsExternal,
                        Email = p.Email,
                        PhoneNumber = p.PhoneNumber
                    }).OrderByDescending(p => p.AcademicDegree).ThenBy(p => p.FullName).ToList();

                    // Lấy danh sách gợi ý chuyên gia phản biện từ AI Heuristic (Advisory Only)
                    var aiSuggestions = await _aiSuggestionService.SuggestReviewersAsync(thesisId.Value);
                    model.AiReviewerSuggestions = aiSuggestions.Select(s => new AiSuggestionItemViewModel
                    {
                        PersonId = s.PersonId,
                        PersonName = s.FullName,
                        SharedPersonCode = s.SharedPersonCode,
                        AcademicDegree = s.AcademicDegree,
                        ResearchField = s.ResearchField,
                        Workplace = s.Workplace,
                        IsExternal = s.IsExternal,
                        MatchScore = s.MatchScore,
                        FieldScore = s.FieldScore,
                        DegreeScore = s.DegreeScore,
                        ExternalScore = s.ExternalScore,
                        Explanation = s.RecommendationReason,
                        IsAdvisoryOnly = s.IsAdvisoryOnly,
                        SuggestionState = s.SuggestionState,
                        RecommendationNote = s.AdvisoryNote
                    }).ToList();

                    model.NewAssignment = new AssignPersonViewModel
                    {
                        ThesisId = thesisId.Value
                    };
                }
            }

            // CHỈ ĐỊNH TƯỜNG MINH ĐƯỜNG DẪN VIEW THEO ĐẶC TẢ
            return View("~/Views/Assignment/Index.cshtml", model);
        }

        /// <summary>
        /// Xử lý phân công chuyên gia khoa học (hướng dẫn, phản biện, hội đồng) cho đề tài.
        /// Chỉ Quản lý (Manager) hoặc Ban quản trị (Admin) mới có quyền thực hiện.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Assign(AssignPersonViewModel model)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                _logger.LogWarning("Từ chối truy cập: Người dùng '{User}' không có quyền phân công chuyên gia.", User.Identity?.Name);
                return Forbid();
            }
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin phân công không hợp lệ. Vui lòng kiểm tra lại.";
                return RedirectToAction(nameof(Index), new { thesisId = model.ThesisId });
            }

            try
            {
                // Thẩm định điều kiện trước khi phân công
                var check = await _assignmentService.CheckEligibilityAsync(model.ThesisId, model.PersonId, model.Role);
                if (check.Result == CheckResult.Ineligible)
                {
                    TempData["WarningMessage"] = $"Cảnh báo thẩm định: {check.Basis}";
                }

                var success = await _assignmentService.AssignPersonAsync(
                    model.ThesisId,
                    model.PersonId,
                    model.Role,
                    model.IsExternal);

                if (success)
                {
                    TempData["SuccessMessage"] = "Phân công chuyên gia thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể phân công chuyên gia.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi phân công chuyên gia: {Message}", ex.Message);
                TempData["ErrorMessage"] = $"Lỗi phân công: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { thesisId = model.ThesisId });
        }

        /// <summary>
        /// API trả về danh sách gợi ý chuyên gia phản biện từ AI Heuristic (Advisory Only).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAiSuggestions(int thesisId)
        {
            try
            {
                var suggestions = await _aiSuggestionService.SuggestReviewersAsync(thesisId);
                return Json(new { success = true, data = suggestions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy gợi ý AI cho đề tài {ThesisId}", thesisId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// API kiểm tra điều kiện quy chế của một chuyên gia đối với một vai trò cho đề tài.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckEligibility(int thesisId, int personId, AssignmentRole role)
        {
            try
            {
                var result = await _assignmentService.CheckEligibilityAsync(thesisId, personId, role);
                return Json(new
                {
                    success = true,
                    result = result.Result.ToString(),
                    resultName = result.Result switch
                    {
                        CheckResult.Eligible => "Đủ điều kiện",
                        CheckResult.Ineligible => "Không đủ điều kiện",
                        CheckResult.Conditional => "Có điều kiện",
                        _ => result.Result.ToString()
                    },
                    basis = result.Basis,
                    regulationVersion = result.RegulationVersion,
                    checkedAt = result.CheckedAt.ToString("dd/MM/yyyy HH:mm")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi kiểm tra điều kiện: {Message}", ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa phân công chuyên gia khỏi đề tài.
        /// Chỉ Quản lý (Manager) hoặc Ban quản trị (Admin) mới có quyền thực hiện.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RemoveAssignment(int assignmentId, int thesisId)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                _logger.LogWarning("Từ chối truy cập: Người dùng '{User}' không có quyền hủy phân công chuyên gia.", User.Identity?.Name);
                return Forbid();
            }
            try
            {
                var assignment = await _unitOfWork.ThesisAssignments.GetByIdAsync(assignmentId);
                if (assignment != null)
                {
                    _unitOfWork.ThesisAssignments.Remove(assignment);
                    await _unitOfWork.CompleteAsync();
                    TempData["SuccessMessage"] = "Đã hủy phân công chuyên gia.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bản ghi phân công cần hủy.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hủy phân công {AssignmentId}: {Message}", assignmentId, ex.Message);
                TempData["ErrorMessage"] = $"Lỗi khi hủy phân công: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { thesisId });
        }
    }
}
