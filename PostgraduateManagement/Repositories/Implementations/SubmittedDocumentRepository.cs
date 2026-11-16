// File: Repositories/Implementations/SubmittedDocumentRepository.cs
using Microsoft.EntityFrameworkCore;
using PostgraduateManagement.Data;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Repositories.Interfaces;

namespace PostgraduateManagement.Repositories.Implementations
{
    /// <summary>
    /// Triển khai kho lưu trữ chuyên biệt cho tài liệu số hóa nộp lưu chiểu (SubmittedDocument).
    /// </summary>
    public class SubmittedDocumentRepository : Repository<SubmittedDocument>, ISubmittedDocumentRepository
    {
        public SubmittedDocumentRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Lấy toàn bộ tài liệu đã nộp của đề tài theo thứ tự thời gian nộp và phiên bản mới nhất
        /// </summary>
        public async Task<IEnumerable<SubmittedDocument>> GetByThesisIdAsync(int thesisId)
        {
            return await _context.SubmittedDocuments
                .Where(d => d.ThesisId == thesisId)
                .OrderByDescending(d => d.UploadedAt)
                .ThenByDescending(d => d.Version)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Lấy phiên bản tài liệu mới nhất theo loại tài liệu cụ thể của đề tài
        /// </summary>
        public async Task<SubmittedDocument?> GetLatestVersionAsync(int thesisId, DocumentType type)
        {
            return await _context.SubmittedDocuments
                .Where(d => d.ThesisId == thesisId && d.DocumentType == type)
                .OrderByDescending(d => d.Version)
                .ThenByDescending(d => d.UploadedAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy chi tiết tài liệu kèm thông tin đề tài, học viên và danh sách phân công chuyên gia phục vụ kiểm tra quyền tải
        /// </summary>
        public async Task<SubmittedDocument?> GetDocumentWithDetailsAsync(int id)
        {
            return await _context.SubmittedDocuments
                .Include(d => d.Thesis)
                    .ThenInclude(t => t!.Student)
                .Include(d => d.Thesis)
                    .ThenInclude(t => t!.Assignments)
                        .ThenInclude(a => a.Person)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}
