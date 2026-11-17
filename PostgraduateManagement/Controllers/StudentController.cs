// File: Controllers/StudentController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Repositories.Interfaces;
using PostgraduateManagement.ViewModels;

namespace PostgraduateManagement.Controllers
{
    /// <summary>
    /// Controller quản lý hồ sơ người học sau đại học (học viên cao học, nghiên cứu sinh).
    /// Mã người học dùng chung (SharedStudentCode) là duy nhất trong toàn hệ thống.
    /// Chỉ Quản trị và Quản lý đào tạo được thao tác hồ sơ.
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    public class StudentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var students = string.IsNullOrWhiteSpace(search)
                ? await _unitOfWork.PostgraduateStudents.GetAllAsync()
                : await _unitOfWork.PostgraduateStudents.FindAsync(s =>
                    s.FullName.Contains(search) || s.SharedStudentCode.Contains(search) || s.Email.Contains(search));

            return View(new StudentListViewModel
            {
                Search = search,
                Students = students.OrderBy(s => s.SharedStudentCode).ToList()
            });
        }

        [HttpGet]
        public IActionResult Create() => View("Form", new StudentFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentFormViewModel model)
        {
            await ValidateUniqueCodeAsync(model);
            if (!ModelState.IsValid) return View("Form", model);

            var student = new PostgraduateStudent();
            MapToEntity(model, student);
            await _unitOfWork.PostgraduateStudents.AddAsync(student);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = $"Đã thêm hồ sơ học viên {student.FullName}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var s = await _unitOfWork.PostgraduateStudents.GetByIdAsync(id);
            if (s == null) return NotFound();

            return View("Form", new StudentFormViewModel
            {
                Id = s.Id,
                SharedStudentCode = s.SharedStudentCode,
                FullName = s.FullName,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                Major = s.Major,
                EnrollmentYear = s.EnrollmentYear
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentFormViewModel model)
        {
            if (id != model.Id) return BadRequest();
            var student = await _unitOfWork.PostgraduateStudents.GetByIdAsync(id);
            if (student == null) return NotFound();

            await ValidateUniqueCodeAsync(model);
            if (!ModelState.IsValid) return View("Form", model);

            MapToEntity(model, student);
            _unitOfWork.PostgraduateStudents.Update(student);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật hồ sơ học viên {student.FullName}.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Xóa hồ sơ học viên. Không cho xóa khi học viên đã có đề tài để giữ toàn vẹn lịch sử đào tạo.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _unitOfWork.PostgraduateStudents.GetByIdAsync(id);
            if (student == null) return NotFound();

            if ((await _unitOfWork.Theses.FindAsync(t => t.StudentId == id)).Any())
            {
                TempData["ErrorMessage"] = $"Không thể xóa {student.FullName}: học viên đã có đề tài trong hệ thống.";
                return RedirectToAction(nameof(Index));
            }

            _unitOfWork.PostgraduateStudents.Remove(student);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = $"Đã xóa hồ sơ học viên {student.FullName}.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateUniqueCodeAsync(StudentFormViewModel model)
        {
            var code = model.SharedStudentCode?.Trim() ?? string.Empty;
            if ((await _unitOfWork.PostgraduateStudents.FindAsync(s => s.SharedStudentCode == code && s.Id != model.Id)).Any())
            {
                ModelState.AddModelError(nameof(model.SharedStudentCode), "Mã người học này đã tồn tại.");
            }
        }

        private static void MapToEntity(StudentFormViewModel model, PostgraduateStudent student)
        {
            student.SharedStudentCode = model.SharedStudentCode.Trim();
            student.FullName = model.FullName.Trim();
            student.Email = model.Email.Trim();
            student.PhoneNumber = model.PhoneNumber?.Trim();
            student.Major = model.Major.Trim();
            student.EnrollmentYear = model.EnrollmentYear;
        }
    }
}
