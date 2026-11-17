// File: Controllers/ExpertController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Repositories.Interfaces;
using PostgraduateManagement.ViewModels;

namespace PostgraduateManagement.Controllers
{
    /// <summary>
    /// Controller quản lý chuyên gia, cán bộ khoa học: cán bộ cơ hữu và chuyên gia ngoài trường (IsExternal).
    /// Dữ liệu này là nguồn cho thẩm định điều kiện, phân công và gợi ý phản biện AI.
    /// Chỉ Quản trị và Quản lý đào tạo được thao tác.
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    public class ExpertController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExpertController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, bool? isExternal)
        {
            var experts = await _unitOfWork.AcademicPersons.FindAsync(p =>
                (isExternal == null || p.IsExternal == isExternal) &&
                (string.IsNullOrEmpty(search) || p.FullName.Contains(search) || p.ResearchField.Contains(search)
                    || (p.SharedPersonCode != null && p.SharedPersonCode.Contains(search))));

            return View(new ExpertListViewModel
            {
                Search = search,
                IsExternal = isExternal,
                Experts = experts.OrderBy(p => p.FullName).ToList()
            });
        }

        [HttpGet]
        public IActionResult Create() => View("Form", new ExpertFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpertFormViewModel model)
        {
            await ValidateUniqueCodeAsync(model);
            if (!ModelState.IsValid) return View("Form", model);

            var expert = new AcademicPerson();
            MapToEntity(model, expert);
            await _unitOfWork.AcademicPersons.AddAsync(expert);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = $"Đã thêm chuyên gia {expert.FullName}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _unitOfWork.AcademicPersons.GetByIdAsync(id);
            if (p == null) return NotFound();

            return View("Form", new ExpertFormViewModel
            {
                Id = p.Id,
                SharedPersonCode = p.SharedPersonCode,
                FullName = p.FullName,
                AcademicDegree = p.AcademicDegree,
                ResearchField = p.ResearchField,
                IsExternal = p.IsExternal,
                Email = p.Email,
                PhoneNumber = p.PhoneNumber,
                Workplace = p.Workplace
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpertFormViewModel model)
        {
            if (id != model.Id) return BadRequest();
            var expert = await _unitOfWork.AcademicPersons.GetByIdAsync(id);
            if (expert == null) return NotFound();

            await ValidateUniqueCodeAsync(model);
            if (!ModelState.IsValid) return View("Form", model);

            MapToEntity(model, expert);
            _unitOfWork.AcademicPersons.Update(expert);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật chuyên gia {expert.FullName}.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Xóa chuyên gia. Không cho xóa khi đã có phân công hoặc lịch sử thẩm định điều kiện.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var expert = await _unitOfWork.AcademicPersons.GetByIdAsync(id);
            if (expert == null) return NotFound();

            if ((await _unitOfWork.ThesisAssignments.FindAsync(a => a.PersonId == id)).Any()
                || (await _unitOfWork.EligibilityChecks.FindAsync(c => c.PersonId == id)).Any())
            {
                TempData["ErrorMessage"] = $"Không thể xóa {expert.FullName}: chuyên gia đã có phân công hoặc lịch sử thẩm định.";
                return RedirectToAction(nameof(Index));
            }

            _unitOfWork.AcademicPersons.Remove(expert);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = $"Đã xóa chuyên gia {expert.FullName}.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateUniqueCodeAsync(ExpertFormViewModel model)
        {
            var code = model.SharedPersonCode?.Trim();
            if (string.IsNullOrEmpty(code)) return;
            if ((await _unitOfWork.AcademicPersons.FindAsync(p => p.SharedPersonCode == code && p.Id != model.Id)).Any())
            {
                ModelState.AddModelError(nameof(model.SharedPersonCode), "Mã nhân sự này đã tồn tại.");
            }
        }

        private static void MapToEntity(ExpertFormViewModel model, AcademicPerson expert)
        {
            expert.SharedPersonCode = string.IsNullOrWhiteSpace(model.SharedPersonCode) ? null : model.SharedPersonCode.Trim();
            expert.FullName = model.FullName.Trim();
            expert.AcademicDegree = model.AcademicDegree;
            expert.ResearchField = model.ResearchField.Trim();
            expert.IsExternal = model.IsExternal;
            expert.Email = model.Email.Trim();
            expert.PhoneNumber = model.PhoneNumber?.Trim();
            expert.Workplace = model.Workplace?.Trim();
        }
    }
}
