// File: Repositories/Implementations/UnitOfWork.cs
using PostgraduateManagement.Data;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Repositories.Interfaces;

namespace PostgraduateManagement.Repositories.Implementations
{
    /// <summary>
    /// Triển khai Unit of Work quản lý vòng đời và tính toàn vẹn của các giao dịch trong AppDbContext.
    /// Khởi tạo theo cơ chế Lazy Initialization cho từng Repository để tối ưu hiệu năng và chia sẻ DbContext chung.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IThesisRepository? _theses;
        private IAcademicPersonRepository? _academicPersons;
        private IDefenseSessionRepository? _defenseSessions;
        private ISubmittedDocumentRepository? _submittedDocuments;
        private IRepository<PostgraduateStudent>? _postgraduateStudents;
        private IRepository<ThesisAssignment>? _thesisAssignments;
        private IRepository<EligibilityCheck>? _eligibilityChecks;
        private IRepository<DefenseResult>? _defenseResults;
        private IRepository<AuditLog>? _auditLogs;
        private IRepository<DefenseComment>? _defenseComments;
        private IRepository<ProgressReport>? _progressReports;
        private bool _disposed = false;

        public UnitOfWork(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IThesisRepository Theses =>
            _theses ??= new ThesisRepository(_context);

        public IAcademicPersonRepository AcademicPersons =>
            _academicPersons ??= new AcademicPersonRepository(_context);

        public IDefenseSessionRepository DefenseSessions =>
            _defenseSessions ??= new DefenseSessionRepository(_context);

        public ISubmittedDocumentRepository SubmittedDocuments =>
            _submittedDocuments ??= new SubmittedDocumentRepository(_context);

        public IRepository<PostgraduateStudent> PostgraduateStudents =>
            _postgraduateStudents ??= new Repository<PostgraduateStudent>(_context);

        public IRepository<ThesisAssignment> ThesisAssignments =>
            _thesisAssignments ??= new Repository<ThesisAssignment>(_context);

        public IRepository<EligibilityCheck> EligibilityChecks =>
            _eligibilityChecks ??= new Repository<EligibilityCheck>(_context);

        public IRepository<DefenseResult> DefenseResults =>
            _defenseResults ??= new Repository<DefenseResult>(_context);

        public IRepository<AuditLog> AuditLogs =>
            _auditLogs ??= new Repository<AuditLog>(_context);

        public IRepository<DefenseComment> DefenseComments =>
            _defenseComments ??= new Repository<DefenseComment>(_context);

        public IRepository<ProgressReport> ProgressReports =>
            _progressReports ??= new Repository<ProgressReport>(_context);

        /// <summary>
        /// Thực thi lưu toàn bộ thay đổi dữ liệu trong ngữ cảnh giao dịch
        /// </summary>
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
