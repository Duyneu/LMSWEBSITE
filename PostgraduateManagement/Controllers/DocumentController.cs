// File: Controllers/DocumentController.cs
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
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
    /// Controller quản lý nộp, lưu trữ và phê duyệt tài liệu số hóa nộp lưu chiểu.
    /// Giới hạn nghiêm ngặt trong phạm vi danh mục 3 loại tài liệu bắt buộc của Module M05:
    /// Thuyết minh đề tài, Toàn văn luận văn/luận án, Biên bản sửa đổi sau bảo vệ.
    /// Quản lý tiến độ số hóa kèm thông điệp cảnh báo ranh giới quy chuẩn đào tạo.
    /// </summary>
    [Authorize]
    public class DocumentController : Controller
    {
        private const long MaxFileSize = 20 * 1024 * 1024; // 20 MB
        private static readonly string[] AllowedExtensions = { ".pdf", ".docx" };
        private static readonly byte[] PdfMagicBytes = { 0x25, 0x50, 0x44, 0x46 }; // %PDF
        private static readonly byte[] DocxMagicBytes = { 0x50, 0x4B, 0x03, 0x04 }; // PK zip

        private readonly IDefenseAndDocumentService _defenseService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DocumentController> _logger;

        public DocumentController(
            IDefenseAndDocumentService defenseService,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            ILogger<DocumentController> logger)
        {
            _defenseService = defenseService;
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Lấy và đảm bảo thư mục lưu trữ tài liệu an toàn bên ngoài wwwroot.
        /// Đọc từ IConfiguration["Storage:UploadPath"] hoặc fallback về thư mục PostgraduateStorage/uploads.
        /// </summary>
        private string GetUploadDirectory()
        {
            var configuredPath = _configuration["Storage:UploadPath"];
            string storagePath;

            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                storagePath = Path.IsPathRooted(configuredPath)
                    ? configuredPath
                    : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), configuredPath));
            }
            else
            {
                storagePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "PostgraduateStorage", "uploads"));
            }

            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            return storagePath;
        }

        /// <summary>
        /// Xác thực tệp tin: kiểm tra whitelist đuôi tệp (.pdf, .docx), giới hạn kích thước (20MB) và chữ ký byte (magic bytes).
        /// </summary>
        private async Task<bool> ValidateUploadedFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Vui lòng chọn tệp tin tài liệu hợp lệ.");
                return false;
            }

            if (file.Length > MaxFileSize)
            {
                ModelState.AddModelError("File", "Dung lượng tệp tin vượt quá giới hạn tối đa cho phép (20 MB).");
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("File", "Định dạng tệp không được hỗ trợ. Hệ thống chỉ cho phép tải lên tệp .pdf hoặc .docx.");
                return false;
            }

            byte[] header = new byte[4];
            using (var stream = file.OpenReadStream())
            {
                int bytesRead = await stream.ReadAsync(header, 0, 4);
                if (bytesRead < 4)
                {
                    ModelState.AddModelError("File", "Tệp tin không hợp lệ hoặc bị hỏng.");
                    return false;
                }

                if (extension == ".pdf")
                {
                    if (!header.SequenceEqual(PdfMagicBytes))
                    {
                        ModelState.AddModelError("File", "Nội dung tệp không hợp lệ. Chữ ký tệp (magic bytes) không khớp định dạng PDF chuẩn.");
                        return false;
                    }
                }
                else if (extension == ".docx")
                {
                    if (!header.SequenceEqual(DocxMagicBytes))
                    {
                        ModelState.AddModelError("File", "Nội dung tệp không hợp lệ. Chữ ký tệp (magic bytes) không khớp định dạng DOCX chuẩn.");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Phạm vi truy cập tài liệu của một đề tài (Scope Check), dùng chung cho mọi thao tác tài liệu:
        /// - Admin / Manager: toàn bộ đề tài.
        /// - Student: chỉ đề tài của chính mình.
        /// - Faculty: chỉ đề tài mình là Người hướng dẫn.
        /// - Committee: chỉ đề tài mình được phân công Phản biện hoặc Hội đồng.
        /// Đề tài phải được nạp kèm Student và Assignments.Person.
        /// </summary>
        private bool CanAccessThesis(Thesis thesis)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                return true;
            }

            var userName = User.Identity?.Name ?? string.Empty;
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? userName;
            bool IsMe(string? email, string? sharedCode) =>
                string.Equals(email, userName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(sharedCode, userName, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrEmpty(userEmail) && string.Equals(email, userEmail, StringComparison.OrdinalIgnoreCase));

            if (User.IsInRole("Student"))
            {
                return thesis.Student != null && IsMe(thesis.Student.Email, thesis.Student.SharedStudentCode);
            }

            if (User.IsInRole("Faculty"))
            {
                return thesis.Assignments.Any(a => a.Role == AssignmentRole.Supervisor
                    && a.Person != null && IsMe(a.Person.Email, a.Person.SharedPersonCode));
            }

            if (User.IsInRole("Committee"))
            {
                return thesis.Assignments.Any(a => (a.Role == AssignmentRole.Reviewer || a.Role == AssignmentRole.Committee)
                    && a.Person != null && IsMe(a.Person.Email, a.Person.SharedPersonCode));
            }

            return false;
        }

        /// <summary>
        /// Hiển thị màn hình quản lý tài liệu số hóa và dashboard tiến độ số hóa 3 loại văn bản bắt buộc M05.
        /// Chỉ liệt kê và hiển thị các đề tài thuộc phạm vi truy cập của người dùng.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int? thesisId)
        {
            List<Thesis> allTheses;
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                allTheses = (await _unitOfWork.Theses.GetAllAsync()).OrderBy(t => t.Code).ToList();
            }
            else
            {
                // shortcut: one query per thesis to load its owner and assignments; fine for a department's few hundred theses, move to a filtered query if the list grows
                allTheses = new List<Thesis>();
                foreach (var t in await _unitOfWork.Theses.GetAllAsync())
                {
                    var detailed = await _unitOfWork.Theses.GetThesisWithFullDetailsAsync(t.Id);
                    if (detailed != null && CanAccessThesis(detailed)) allTheses.Add(detailed);
                }
                allTheses = allTheses.OrderBy(t => t.Code).ToList();
            }

            if (thesisId.HasValue && !allTheses.Any(t => t.Id == thesisId.Value))
            {
                _logger.LogWarning("Từ chối truy cập: Người dùng '{User}' không có quyền xem tài liệu của đề tài {ThesisId}", User.Identity?.Name, thesisId.Value);
                return Forbid();
            }

            if (!thesisId.HasValue && allTheses.Any())
            {
                thesisId = allTheses.First().Id;
            }

            var model = new DocumentIndexViewModel
            {
                ThesisId = thesisId ?? 0,
                AvailableTheses = allTheses.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"[{t.Code}] {t.Title}",
                    Selected = thesisId.HasValue && t.Id == thesisId.Value
                }).ToList()
            };

            if (thesisId.HasValue)
            {
                var thesis = await _unitOfWork.Theses.GetThesisWithFullDetailsAsync(thesisId.Value);
                if (thesis != null)
                {
                    model.ThesisCode = thesis.Code;
                    model.ThesisTitle = thesis.Title;
                    model.StudentName = thesis.Student?.FullName ?? string.Empty;
                    model.StudentCode = thesis.Student?.SharedStudentCode ?? string.Empty;

                    // Lấy báo cáo tiến độ số hóa 3 loại tài liệu bắt buộc M05
                    var progress = await _defenseService.GetDigitizationProgressAsync(thesisId.Value);
                    model.DigitizationProgress = progress;

                    // Lấy danh sách toàn bộ tài liệu đã nộp
                    var documents = await _unitOfWork.SubmittedDocuments.GetByThesisIdAsync(thesisId.Value);
                    model.Documents = documents.Select(d => new DocumentItemViewModel
                    {
                        Id = d.Id,
                        ThesisId = d.ThesisId,
                        DocumentType = d.DocumentType,
                        FileName = d.FileName,
                        FilePath = d.FilePath,
                        Sha256Hash = d.Sha256Hash,
                        Version = d.Version,
                        UploadedAt = d.UploadedAt,
                        ConfirmedBy = d.ConfirmedBy,
                        ConfirmedAt = d.ConfirmedAt
                    }).ToList();

                    model.UploadForm = new UploadDocumentViewModel
                    {
                        ThesisId = thesisId.Value,
                        ThesisCode = thesis.Code,
                        ThesisTitle = thesis.Title
                    };
                }
            }

            return View(model);
        }

        /// <summary>
        /// Giao diện tải lên tài liệu số hóa nộp lưu chiểu.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Upload(int thesisId)
        {
            var thesis = await _unitOfWork.Theses.GetThesisWithFullDetailsAsync(thesisId);
            if (thesis == null)
            {
                _logger.LogWarning("Không tìm thấy đề tài với ID = {ThesisId}", thesisId);
                return NotFound();
            }

            if (!CanAccessThesis(thesis))
            {
                return Forbid();
            }

            var model = new UploadDocumentViewModel
            {
                ThesisId = thesis.Id,
                ThesisCode = thesis.Code,
                ThesisTitle = thesis.Title,
                DocumentType = DocumentType.ThuyetMinh
            };

            return View(model);
        }

        /// <summary>
        /// Xử lý tải lên tài liệu số hóa nộp lưu chiểu.
        /// Lưu trữ an toàn ngoài wwwroot với tên GUID, tính mã băm SHA-256 và xác thực magic bytes.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(UploadDocumentViewModel model)
        {
            var targetThesis = await _unitOfWork.Theses.GetThesisWithFullDetailsAsync(model.ThesisId);
            if (targetThesis == null)
            {
                return NotFound();
            }

            if (!CanAccessThesis(targetThesis))
            {
                _logger.LogWarning("Từ chối tải lên: Người dùng '{User}' không có quyền với đề tài {ThesisId}", User.Identity?.Name, model.ThesisId);
                return Forbid();
            }

            if (targetThesis.ThesisStatus == ThesisStatus.Archived)
            {
                TempData["ErrorMessage"] = "Đề tài đã lưu trữ, không nhận thêm tài liệu.";
                return RedirectToAction(nameof(Index), new { thesisId = model.ThesisId });
            }

            await ValidateUploadedFileAsync(model.File);

            if (!ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.ThesisCode))
                {
                    var thesis = await _unitOfWork.Theses.GetByIdAsync(model.ThesisId);
                    if (thesis != null)
                    {
                        model.ThesisCode = thesis.Code;
                        model.ThesisTitle = thesis.Title;
                    }
                }
                return View(model);
            }

            string? savedFilePath = null;
            try
            {
                // Thư mục lưu trữ an toàn ngoài wwwroot
                var uploadsFolder = GetUploadDirectory();

                // Tên tệp máy chủ tạo bằng GUID để tránh xung đột và path traversal
                var originalFileName = Path.GetFileName(model.File!.FileName);
                var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
                var diskFileName = $"{Guid.NewGuid()}{extension}";
                savedFilePath = Path.Combine(uploadsFolder, diskFileName);

                // Tính toán mã băm SHA-256 của tệp tin
                string sha256Hex;
                using (var sha256 = SHA256.Create())
                {
                    using (var stream = model.File.OpenReadStream())
                    {
                        var hashBytes = await sha256.ComputeHashAsync(stream);
                        sha256Hex = Convert.ToHexString(hashBytes).ToLowerInvariant();
                    }
                }

                // Lưu tệp vật lý vào ổ đĩa
                using (var fileStream = new FileStream(savedFilePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(fileStream);
                }

                var dto = new UploadDocumentDto
                {
                    ThesisId = model.ThesisId,
                    DocumentType = model.DocumentType,
                    FileName = originalFileName,
                    FilePath = savedFilePath,
                    Sha256Hash = sha256Hex
                };

                var uploadedDoc = await _defenseService.UploadDocumentAsync(dto);

                TempData["SuccessMessage"] = $"Tải lên tài liệu thành công: [{uploadedDoc.DocumentTypeName}] phiên bản v{uploadedDoc.Version}.";
                return RedirectToAction(nameof(Index), new { thesisId = model.ThesisId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải lên tài liệu số hóa: {Message}", ex.Message);
                if (!string.IsNullOrEmpty(savedFilePath) && System.IO.File.Exists(savedFilePath))
                {
                    try { System.IO.File.Delete(savedFilePath); } catch { /* ignore */ }
                }

                ModelState.AddModelError(string.Empty, $"Lỗi tải lên: {ex.Message}");
                if (string.IsNullOrEmpty(model.ThesisCode))
                {
                    var thesis = await _unitOfWork.Theses.GetByIdAsync(model.ThesisId);
                    if (thesis != null)
                    {
                        model.ThesisCode = thesis.Code;
                        model.ThesisTitle = thesis.Title;
                    }
                }
                return View(model);
            }
        }

        /// <summary>
        /// Tải xuống tài liệu số hóa nộp lưu chiểu với kiểm soát truy cập theo phạm vi người dùng (Checklist A1 Scope Check).
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var document = await _unitOfWork.SubmittedDocuments.GetDocumentWithDetailsAsync(id);
            if (document == null)
            {
                _logger.LogWarning("Không tìm thấy tài liệu số hóa với ID = {Id}", id);
                return NotFound();
            }

            // Tìm đường dẫn tệp vật lý
            string fullFilePath;
            if (Path.IsPathRooted(document.FilePath) && System.IO.File.Exists(document.FilePath))
            {
                fullFilePath = document.FilePath;
            }
            else
            {
                var uploadDir = GetUploadDirectory();
                var fileName = Path.GetFileName(document.FilePath);
                var candidatePath = Path.Combine(uploadDir, fileName);

                if (System.IO.File.Exists(candidatePath))
                {
                    fullFilePath = candidatePath;
                }
                else
                {
                    // Fallback kiểm tra wwwroot đối với dữ liệu seed cũ
                    var webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
                    var legacyPath = Path.Combine(webRoot, document.FilePath.TrimStart('/', '\\'));
                    if (System.IO.File.Exists(legacyPath))
                    {
                        fullFilePath = legacyPath;
                    }
                    else
                    {
                        fullFilePath = candidatePath;
                    }
                }
            }

            if (!System.IO.File.Exists(fullFilePath))
            {
                _logger.LogWarning("Tệp tin tài liệu #{Id} không tồn tại trên hệ thống lưu trữ tại {Path}", id, fullFilePath);
                return NotFound();
            }

            // Phân quyền truy cập theo phạm vi đề tài (Scope Check)
            if (document.Thesis == null || !CanAccessThesis(document.Thesis))
            {
                _logger.LogWarning("Người dùng {User} không có quyền tải tài liệu #{Id} của đề tài {ThesisCode}", User.Identity?.Name, id, document.Thesis?.Code);
                return Forbid();
            }

            var ext = Path.GetExtension(fullFilePath).ToLowerInvariant();
            var contentType = ext switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };

            var downloadFileName = !string.IsNullOrWhiteSpace(document.FileName)
                ? document.FileName
                : Path.GetFileName(fullFilePath);

            return PhysicalFile(fullFilePath, contentType, downloadFileName);
        }

        /// <summary>
        /// Xác nhận phê duyệt tính hợp lệ của tài liệu số hóa nộp lưu chiểu.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Committee")]
        public async Task<IActionResult> Confirm(int documentId, int thesisId)
        {
            var document = await _unitOfWork.SubmittedDocuments.GetDocumentWithDetailsAsync(documentId);
            if (document == null)
            {
                return NotFound();
            }

            // Thành viên hội đồng chỉ duyệt tài liệu của đề tài mình được phân công
            if (document.Thesis == null || !CanAccessThesis(document.Thesis))
            {
                _logger.LogWarning("Từ chối duyệt: Người dùng '{User}' không có quyền với tài liệu #{DocumentId}", User.Identity?.Name, documentId);
                return Forbid();
            }

            try
            {
                var confirmer = User.Identity?.Name ?? "Hội đồng Sau đại học";
                var success = await _defenseService.ConfirmDocumentAsync(documentId, confirmer);

                if (success)
                {
                    TempData["SuccessMessage"] = "Xác nhận phê duyệt tài liệu số hóa thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể phê duyệt tài liệu. Vui lòng kiểm tra lại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác nhận tài liệu #{DocumentId}: {Message}", documentId, ex.Message);
                TempData["ErrorMessage"] = $"Lỗi xác nhận: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { thesisId });
        }

        /// <summary>
        /// Báo cáo tiến độ số hóa tài liệu cho đề tài sau đại học.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DigitizationReport(int thesisId)
        {
            var thesis = await _unitOfWork.Theses.GetThesisWithFullDetailsAsync(thesisId);
            if (thesis == null)
            {
                return NotFound();
            }

            if (!CanAccessThesis(thesis))
            {
                return Forbid();
            }

            var progress = await _defenseService.GetDigitizationProgressAsync(thesisId);

            var model = new DigitizationReportViewModel
            {
                ThesisId = thesis.Id,
                ThesisCode = thesis.Code,
                ThesisTitle = thesis.Title,
                StudentName = thesis.Student?.FullName ?? string.Empty,
                Progress = progress
            };

            return View(model);
        }
    }
}