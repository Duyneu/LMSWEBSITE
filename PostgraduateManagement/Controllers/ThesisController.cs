// File: Controllers/ThesisController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Repositories.Interfaces;
using PostgraduateManagement.Services.DTOs;
using PostgraduateManagement.Services.Interfaces;
using PostgraduateManagement.ViewModels;

namespace PostgraduateManagement.Controllers
{
    /// <summary>
    /// Controller quản lý vòng đời đề tài sau đại học (Đề án thạc sĩ, Luận văn thạc sĩ, Luận án tiến sĩ).
    /// Hỗ trợ tìm kiếm, lọc đa tiêu chí, phân trang, hiển thị chi tiết, tạo mới, chỉnh sửa,
    /// phê duyệt đề cương và chuyển trạng thái đề tài sang bảo vệ.
    /// Áp dụng kiểm soát truy cập theo phân quyền và phạm vi sở hữu (Ownership / Scope Enforcement).
    /// </summary>
    [Authorize]
    public class ThesisController : Controller
    {
        private readonly IThesisService _thesisService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ThesisController> _logger;

        public ThesisController(
            IThesisService thesisService,
            IUnitOfWork unitOfWork,
            ILogger<ThesisController> logger)
        {
            _thesisService = thesisService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Hiển thị danh sách đề tài sau đại học kèm tìm kiếm, lọc theo trạng thái/loại hình và phân trang.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            ThesisStatus? status,
            ThesisType? type,
            int page = 1,
            int pageSize = 10)
        {
            var filter = new ThesisFilterDto
            {
                Search = search,
                Status = status,
                Type = type,
                PageNumber = page,
                PageSize = pageSize
            };

            var pagedResult = await _thesisService.GetThesesAsync(filter);

            var viewModel = new ThesisListViewModel
            {
                Search = search,
                Status = status,
                Type = type,
                Page = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalItems,
                Theses = pagedResult.Items.Select(t => new ThesisItemViewModel
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
                    StudentName = t.StudentName,
                    StudentCode = t.StudentCode,
                    IsGraduationCapstone = t.IsGraduationCapstone,
                    Supervisors = t.Supervisors,
                    SubmittedDocumentsCount = t.SubmittedDocumentsCount,
                    HasScheduledDefense = t.HasScheduledDefense
                }).ToList(),
                StatusOptions = GetStatusOptions(status),
                TypeOptions = GetTypeOptions(type)
            };

            return View(viewModel);
        }

        /// <summary>
        /// Hiển thị hồ sơ chi tiết toàn diện của một đề tài sau đại học.
        /// Sinh viên chỉ xem đề tài của mình, Cán bộ hướng dẫn xem đề tài hướng dẫn, Ban quản trị / Quản lý xem toàn bộ.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var detailDto = await _thesisService.GetThesisByIdAsync(id);
            if (detailDto == null)
            {
                _logger.LogWarning("Không tìm thấy đề tài với ID = {Id}", id);
                return NotFound();
            }

            // Kiểm soát quyền truy cập theo phạm vi sở hữu (Ownership / Scope Check)
            if (!await CanAccessThesisAsync(detailDto.Id, detailDto.StudentEmail, isEdit: false))
            {
                _logger.LogWarning("Từ chối truy cập: Người dùng '{User}' không có quyền xem chi tiết đề tài {Id}", User.Identity?.Name, id);
                return Forbid();
            }

            var model = new ThesisDetailViewModel
            {
                Id = detailDto.Id,
                Code = detailDto.Code,
                Title = detailDto.Title,
                ThesisType = detailDto.ThesisType,
                Field = detailDto.Field,
                StartDate = detailDto.StartDate,
                EndDate = detailDto.EndDate,
                ThesisStatus = detailDto.ThesisStatus,
                IsGraduationCapstone = detailDto.IsGraduationCapstone,
                Abstract = detailDto.Abstract,
                StudentId = detailDto.StudentId,
                StudentName = detailDto.StudentName,
                StudentCode = detailDto.StudentCode,
                StudentEmail = detailDto.StudentEmail,
                StudentMajor = detailDto.StudentMajor,
                StudentEnrollmentYear = detailDto.StudentEnrollmentYear,
                Assignments = detailDto.Assignments.Select(a => new AssignmentItemViewModel
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
                }).ToList(),
                EligibilityChecks = detailDto.EligibilityChecks.Select(ec => new EligibilityCheckItemViewModel
                {
                    Id = ec.Id,
                    ThesisId = ec.ThesisId,
                    PersonId = ec.PersonId,
                    PersonName = ec.PersonName,
                    AcademicDegree = ec.AcademicDegree,
                    IsExternal = ec.IsExternal,
                    Role = ec.Role,
                    Result = ec.Result,
                    Basis = ec.Basis,
                    RegulationVersion = ec.RegulationVersion,
                    CheckedAt = ec.CheckedAt
                }).ToList(),
                DefenseSessions = detailDto.DefenseSessions.Select(ds => new DefenseSessionItemViewModel
                {
                    Id = ds.Id,
                    ThesisId = ds.ThesisId,
                    ThesisCode = ds.ThesisCode,
                    ThesisTitle = ds.ThesisTitle,
                    ThesisType = ds.ThesisType,
                    Field = ds.Field,
                    StudentName = ds.StudentName,
                    StudentCode = ds.StudentCode,
                    ScheduledAt = ds.ScheduledAt,
                    Location = ds.Location,
                    Status = ds.Status,
                    CommitteeNotes = ds.CommitteeNotes,
                    Result = ds.DefenseResult != null ? new DefenseResultItemViewModel
                    {
                        Id = ds.DefenseResult.Id,
                        SessionId = ds.DefenseResult.SessionId,
                        Score = ds.DefenseResult.Score,
                        Pass = ds.DefenseResult.Pass,
                        Comments = ds.DefenseResult.Comments,
                        RecordedBy = ds.DefenseResult.RecordedBy,
                        RecordedAt = ds.DefenseResult.RecordedAt
                    } : null
                }).ToList(),
                SubmittedDocuments = detailDto.SubmittedDocuments.Select(sd => new DocumentItemViewModel
                {
                    Id = sd.Id,
                    ThesisId = sd.ThesisId,
                    DocumentType = sd.DocumentType,
                    FileName = sd.FileName,
                    FilePath = sd.FilePath,
                    Version = sd.Version,
                    UploadedAt = sd.UploadedAt,
                    ConfirmedBy = sd.ConfirmedBy
                }).ToList(),
                DigitizationProgress = detailDto.DigitizationProgress
            };

            return View(model);
        }

        /// <summary>
        /// Giao diện tạo mới đề tài sau đại học.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var students = await _unitOfWork.PostgraduateStudents.GetAllAsync();
            var model = new CreateThesisViewModel
            {
                Code = $"LV-{DateTime.UtcNow.Year}-{new Random().Next(100, 999)}",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddMonths(12),
                IsGraduationCapstone = true,
                AvailableStudents = students.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.SharedStudentCode} - {s.FullName} ({s.Major})"
                }).ToList()
            };

            return View(model);
        }

        /// <summary>
        /// Xử lý tạo mới và đăng ký đề tài sau đại học vào hệ thống.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateThesisViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateStudentsDropdownAsync(model);
                return View(model);
            }

            try
            {
                var dto = new CreateThesisDto
                {
                    Code = model.Code,
                    Title = model.Title,
                    Field = model.Field,
                    ThesisType = model.ThesisType,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    StudentId = model.StudentId,
                    IsGraduationCapstone = model.IsGraduationCapstone,
                    Abstract = model.Abstract
                };

                var created = await _thesisService.CreateThesisAsync(dto);
                TempData["SuccessMessage"] = $"Đăng ký đề tài thành công: [{created.Code}] {created.Title}";
                return RedirectToAction(nameof(Details), new { id = created.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo đề tài sau đại học: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateStudentsDropdownAsync(model);
                return View(model);
            }
        }

        /// <summary>
        /// Giao diện chỉnh sửa thông tin đề tài.
        /// Chỉ Quản lý/Admin hoặc Cán bộ hướng dẫn trực tiếp đề tài mới có quyền chỉnh sửa.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var thesis = await _unitOfWork.Theses.GetThesisWithFullDetailsAsync(id);
            if (thesis == null)
            {
                _logger.LogWarning("Không tìm thấy đề tài để chỉnh sửa với ID = {Id}", id);
                return NotFound();
            }

            if (!await CanAccessThesisAsync(thesis.Id, thesis.Student?.Email ?? string.Empty, isEdit: true))
            {
                _logger.LogWarning("Từ chối truy cập: Người dùng '{User}' không có quyền chỉnh sửa đề tài {Id}", User.Identity?.Name, id);
                return Forbid();
            }

            var model = new EditThesisViewModel
            {
                Id = thesis.Id,
                Code = thesis.Code,
                Title = thesis.Title,
                Field = thesis.Field,
                ThesisType = thesis.ThesisType,
                ThesisStatus = thesis.ThesisStatus,
                StartDate = thesis.StartDate,
                EndDate = thesis.EndDate,
                Abstract = thesis.Abstract,
                StudentName = thesis.Student?.FullName,
                IsGraduationCapstone = thesis.IsGraduationCapstone
            };

            return View(model);
        }

        /// <summary>
        /// Xử lý lưu thông tin chỉnh sửa đề tài sau đại học.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditThesisViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var thesis = await _unitOfWork.Theses.GetThesisWithFullDetailsAsync(id);
            if (thesis == null)
            {
                return NotFound();
            }

            if (!await CanAccessThesisAsync(thesis.Id, thesis.Student?.Email ?? string.Empty, isEdit: true))
            {
                _logger.LogWarning("Từ chối truy cập: Người dùng '{User}' không có quyền cập nhật đề tài {Id}", User.Identity?.Name, id);
                return Forbid();
            }

            thesis.Title = model.Title.Trim();
            thesis.Field = model.Field.Trim();
            thesis.ThesisType = model.ThesisType;
            thesis.ThesisStatus = model.ThesisStatus;
            thesis.StartDate = model.StartDate;
            thesis.EndDate = model.EndDate;
            thesis.Abstract = model.Abstract?.Trim();
            thesis.IsGraduationCapstone = model.IsGraduationCapstone;

            _unitOfWork.Theses.Update(thesis);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = $"Cập nhật đề tài '{thesis.Code}' thành công.";
            return RedirectToAction(nameof(Details), new { id = thesis.Id });
        }

        /// <summary>
        /// Phê duyệt đề cương đề tài sau đại học (chuyển sang trạng thái Approved).
        /// Thao tác trọng yếu (Sensitive Action): Chỉ Quản lý (Manager) hoặc Ban quản trị (Admin) mới có quyền thực hiện.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Approve(int id)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
            {
                _logger.LogWarning("Cảnh báo an ninh: Người dùng '{User}' cố gắng phê duyệt đề tài {Id} mà không có quyền Admin/Manager.", User.Identity?.Name, id);
                return Forbid();
            }

            var result = await _thesisService.ApproveThesisAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Phê duyệt đề cương đề tài thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể phê duyệt đề tài. Vui lòng kiểm tra lại.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// Chuyển đề tài sang trạng thái Chờ phản biện và bảo vệ.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForReview(int id)
        {
            var thesis = await _unitOfWork.Theses.GetThesisWithFullDetailsAsync(id);
            if (thesis == null)
            {
                return NotFound();
            }

            if (!await CanAccessThesisAsync(thesis.Id, thesis.Student?.Email ?? string.Empty, isEdit: false))
            {
                _logger.LogWarning("Từ chối truy cập: Người dùng '{User}' không có quyền gửi đề tài {Id} đi bảo vệ", User.Identity?.Name, id);
                return Forbid();
            }

            var result = await _thesisService.TransitionStatusAsync(id, ThesisStatus.SubmittedForReview);
            if (result)
            {
                TempData["SuccessMessage"] = "Đề tài đã chuyển sang trạng thái Chờ phản biện & bảo vệ.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể chuyển trạng thái đề tài.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        #region Helper Methods

        /// <summary>
        /// Kiểm tra quyền truy cập đề tài theo phạm vi sở hữu (Ownership / Scope Check):
        /// - Admin / Manager: Toàn quyền truy cập mọi đề tài.
        /// - Student: Chỉ xem được đề tài do chính mình thực hiện; không được sửa.
        /// - Faculty: Chỉ xem và chỉnh sửa đề tài mình được phân công làm Người hướng dẫn (Supervisor).
        /// - Committee: Chỉ xem đề tài mình được phân công làm Phản biện hoặc Hội đồng.
        /// </summary>
        private async Task<bool> CanAccessThesisAsync(int thesisId, string studentEmail, bool isEdit = false)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                return true;
            }

            var userEmail = User.Identity?.Name ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return false;
            }

            if (User.IsInRole("Student"))
            {
                if (isEdit) return false;
                return string.Equals(studentEmail, userEmail, StringComparison.OrdinalIgnoreCase);
            }

            if (User.IsInRole("Faculty"))
            {
                var assignments = (await _unitOfWork.ThesisAssignments.FindAsync(
                    a => a.ThesisId == thesisId && a.Role == AssignmentRole.Supervisor)).ToList();

                foreach (var a in assignments)
                {
                    var person = a.Person ?? await _unitOfWork.AcademicPersons.GetByIdAsync(a.PersonId);
                    if (person != null && string.Equals(person.Email, userEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }

            if (User.IsInRole("Committee"))
            {
                if (isEdit) return false;
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
                return false;
            }

            return false;
        }

        private async Task PopulateStudentsDropdownAsync(CreateThesisViewModel model)
        {
            var students = await _unitOfWork.PostgraduateStudents.GetAllAsync();
            model.AvailableStudents = students.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.SharedStudentCode} - {s.FullName} ({s.Major})",
                Selected = s.Id == model.StudentId
            }).ToList();
        }

        private static List<SelectListItem> GetStatusOptions(ThesisStatus? selectedStatus)
        {
            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Tất cả trạng thái --", Selected = !selectedStatus.HasValue }
            };

            foreach (ThesisStatus status in Enum.GetValues(typeof(ThesisStatus)))
            {
                var text = status switch
                {
                    ThesisStatus.Draft => "Dự thảo",
                    ThesisStatus.Registered => "Đã đăng ký",
                    ThesisStatus.Approved => "Đã phê duyệt",
                    ThesisStatus.InProgress => "Đang thực hiện",
                    ThesisStatus.SubmittedForReview => "Chờ phản biện & bảo vệ",
                    ThesisStatus.Defended => "Đã bảo vệ",
                    ThesisStatus.Completed => "Hoàn thành",
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

        private static List<SelectListItem> GetTypeOptions(ThesisType? selectedType)
        {
            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Tất cả loại hình --", Selected = !selectedType.HasValue }
            };

            foreach (ThesisType type in Enum.GetValues(typeof(ThesisType)))
            {
                var text = type switch
                {
                    ThesisType.DeAn => "Đề án thạc sĩ",
                    ThesisType.LuanVan => "Luận văn thạc sĩ",
                    ThesisType.LuanAn => "Luận án tiến sĩ",
                    _ => type.ToString()
                };

                list.Add(new SelectListItem
                {
                    Value = ((int)type).ToString(),
                    Text = text,
                    Selected = selectedType.HasValue && selectedType.Value == type
                });
            }

            return list;
        }

        #endregion
    }
}
