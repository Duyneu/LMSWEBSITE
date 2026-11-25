// File: Repositories/Implementations/ThesisRepository.cs
using Microsoft.EntityFrameworkCore;
using PostgraduateManagement.Data;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Repositories.Interfaces;

namespace PostgraduateManagement.Repositories.Implementations
{
    /// <summary>
    /// Triển khai kho lưu trữ chuyên biệt cho đề tài tốt nghiệp sau đại học (Thesis).
    /// Hỗ trợ tìm kiếm đa tiêu chí, phân trang và truy vấn eager loading toàn bộ quan hệ liên kết.
    /// </summary>
    public class ThesisRepository : Repository<Thesis>, IThesisRepository
    {
        public ThesisRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy danh sách đề tài kèm thông tin người học, phân công và phiên bảo vệ có phân trang và lọc dữ liệu
        /// </summary>
        public async Task<IEnumerable<Thesis>> GetThesesWithDetailsAsync(
            string? search,
            ThesisStatus? status,
            ThesisType? type,
            int page,
            int pageSize,
            IReadOnlyCollection<int>? allowedIds)
        {
            var query = _context.Theses.AsQueryable();

            // Phạm vi truy cập: null = tất cả, ngược lại chỉ các đề tài trong danh sách được phép
            if (allowedIds != null)
            {
                query = query.Where(t => allowedIds.Contains(t.Id));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(t => t.Code.Contains(term) ||
                                         t.Title.Contains(term) ||
                                         t.Field.Contains(term) ||
                                         (t.Student != null && (t.Student.FullName.Contains(term) || t.Student.SharedStudentCode.Contains(term))));
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.ThesisStatus == status.Value);
            }

            if (type.HasValue)
            {
                query = query.Where(t => t.ThesisType == type.Value);
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            return await query
                .Include(t => t.Student)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.Person)
                .Include(t => t.DefenseSessions)
                    .ThenInclude(d => d.DefenseResult)
                .Include(t => t.SubmittedDocuments)
                .OrderByDescending(t => t.Id)
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Đếm tổng số lượng đề tài thỏa mãn điều kiện tìm kiếm và bộ lọc
        /// </summary>
        public async Task<int> GetThesesCountAsync(string? search, ThesisStatus? status, ThesisType? type, IReadOnlyCollection<int>? allowedIds)
        {
            var query = _context.Theses.AsQueryable();

            // Phạm vi truy cập: null = tất cả, ngược lại chỉ các đề tài trong danh sách được phép
            if (allowedIds != null)
            {
                query = query.Where(t => allowedIds.Contains(t.Id));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(t => t.Code.Contains(term) ||
                                         t.Title.Contains(term) ||
                                         t.Field.Contains(term) ||
                                         (t.Student != null && (t.Student.FullName.Contains(term) || t.Student.SharedStudentCode.Contains(term))));
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.ThesisStatus == status.Value);
            }

            if (type.HasValue)
            {
                query = query.Where(t => t.ThesisType == type.Value);
            }

            return await query.CountAsync();
        }

        /// <summary>
        /// Truy vấn chi tiết đầy đủ của một đề tài với toàn bộ các bảng liên kết
        /// </summary>
        public async Task<Thesis?> GetThesisWithFullDetailsAsync(int id)
        {
            return await _context.Theses
                .Include(t => t.Student)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.Person)
                .Include(t => t.EligibilityChecks)
                    .ThenInclude(e => e.Person)
                .Include(t => t.DefenseSessions)
                    .ThenInclude(d => d.DefenseResult)
                .Include(t => t.SubmittedDocuments)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Tìm đề tài theo mã định danh duy nhất (Code)
        /// </summary>
        public async Task<Thesis?> GetThesisByCodeAsync(string code)
        {
            return await _context.Theses
                .Include(t => t.Student)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.Person)
                .FirstOrDefaultAsync(t => t.Code == code);
        }
    }
}
