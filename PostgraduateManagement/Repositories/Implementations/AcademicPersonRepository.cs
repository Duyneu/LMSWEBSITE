// File: Repositories/Implementations/AcademicPersonRepository.cs
using Microsoft.EntityFrameworkCore;
using PostgraduateManagement.Data;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Repositories.Interfaces;

namespace PostgraduateManagement.Repositories.Implementations
{
    /// <summary>
    /// Triển khai kho lưu trữ chuyên biệt cho cán bộ, giảng viên và chuyên gia khoa học (AcademicPerson).
    /// </summary>
    public class AcademicPersonRepository : Repository<AcademicPerson>, IAcademicPersonRepository
    {
        public AcademicPersonRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy danh sách các chuyên gia phù hợp cho vai trò phản biện theo lĩnh vực nghiên cứu,
        /// sắp xếp ưu tiên theo học vị khoa học cao nhất (GS > PGS > TS).
        /// </summary>
        public async Task<IEnumerable<AcademicPerson>> GetEligibleReviewersAsync(string researchField)
        {
            var query = _context.AcademicPersons.AsQueryable();

            if (!string.IsNullOrWhiteSpace(researchField))
            {
                var term = researchField.Trim();
                query = query.Where(p => p.ResearchField.Contains(term));
            }

            return await query
                .OrderByDescending(p => p.AcademicDegree)
                .ThenBy(p => p.FullName)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Tìm chuyên gia theo mã định danh dùng chung toàn hệ thống
        /// </summary>
        public async Task<AcademicPerson?> GetBySharedCodeAsync(string sharedPersonCode)
        {
            if (string.IsNullOrWhiteSpace(sharedPersonCode))
                return null;

            return await _context.AcademicPersons
                .FirstOrDefaultAsync(p => p.SharedPersonCode == sharedPersonCode.Trim());
        }

        /// <summary>
        /// Lấy danh sách chuyên gia ngoài trường (IsExternal = true)
        /// </summary>
        public async Task<IEnumerable<AcademicPerson>> GetExternalExpertsAsync()
        {
            return await _context.AcademicPersons
                .Where(p => p.IsExternal)
                .OrderBy(p => p.FullName)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
