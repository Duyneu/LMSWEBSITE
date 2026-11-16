// File: Repositories/Implementations/DefenseSessionRepository.cs
using Microsoft.EntityFrameworkCore;
using PostgraduateManagement.Data;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Repositories.Interfaces;

namespace PostgraduateManagement.Repositories.Implementations
{
    /// <summary>
    /// Triển khai kho lưu trữ chuyên biệt cho phiên bảo vệ đề tài sau đại học (DefenseSession).
    /// </summary>
    public class DefenseSessionRepository : Repository<DefenseSession>, IDefenseSessionRepository
    {
        public DefenseSessionRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy chi tiết phiên bảo vệ kèm thông tin đề tài, học viên, phân công hội đồng và kết quả đánh giá
        /// </summary>
        public async Task<DefenseSession?> GetSessionWithDetailsAsync(int id)
        {
            return await _context.DefenseSessions
                .Include(s => s.Thesis)
                    .ThenInclude(t => t!.Student)
                .Include(s => s.Thesis)
                    .ThenInclude(t => t!.Assignments)
                        .ThenInclude(a => a.Person)
                .Include(s => s.DefenseResult)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Lấy danh sách các phiên bảo vệ sắp diễn ra hoặc đang tiến hành, sắp xếp theo thời gian sớm nhất
        /// </summary>
        public async Task<IEnumerable<DefenseSession>> GetUpcomingSessionsAsync()
        {
            return await _context.DefenseSessions
                .Include(s => s.Thesis)
                    .ThenInclude(t => t!.Student)
                .Include(s => s.DefenseResult)
                .Where(s => s.Status == DefenseStatus.Scheduled || s.Status == DefenseStatus.InProgress || s.ScheduledAt >= DateTime.UtcNow.Date)
                .OrderBy(s => s.ScheduledAt)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Tìm phiên bảo vệ mới nhất theo mã đề tài sau đại học
        /// </summary>
        public async Task<DefenseSession?> GetSessionByThesisIdAsync(int thesisId)
        {
            return await _context.DefenseSessions
                .Include(s => s.Thesis)
                    .ThenInclude(t => t!.Student)
                .Include(s => s.DefenseResult)
                .Where(s => s.ThesisId == thesisId)
                .OrderByDescending(s => s.ScheduledAt)
                .FirstOrDefaultAsync();
        }
    }
}
