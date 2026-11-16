// File: PostgraduateManagement.Tests/EligibilityAndAssignmentServiceTests.cs
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Moq;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Repositories.Interfaces;
using PostgraduateManagement.Services.Implementations;
using Xunit;

namespace PostgraduateManagement.Tests
{
    /// <summary>
    /// Kiểm thử đơn vị kiểm tra xung đột lợi ích (Conflict of Interest) trong phân công chuyên gia sau đại học.
    /// Quy tắc: Cán bộ hướng dẫn (Supervisor) KHÔNG ĐƯỢC làm Phản biện (Reviewer) hoặc Hội đồng (Committee), và ngược lại.
    /// </summary>
    public class EligibilityAndAssignmentServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IThesisRepository> _mockThesisRepo;
        private readonly Mock<IAcademicPersonRepository> _mockPersonRepo;
        private readonly Mock<IRepository<ThesisAssignment>> _mockAssignmentRepo;
        private readonly Mock<IRepository<EligibilityCheck>> _mockCheckRepo;
        private readonly Mock<IRepository<AuditLog>> _mockAuditLogRepo;
        private readonly Mock<ILogger<EligibilityAndAssignmentService>> _mockLogger;
        private readonly EligibilityAndAssignmentService _service;

        public EligibilityAndAssignmentServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockThesisRepo = new Mock<IThesisRepository>();
            _mockPersonRepo = new Mock<IAcademicPersonRepository>();
            _mockAssignmentRepo = new Mock<IRepository<ThesisAssignment>>();
            _mockCheckRepo = new Mock<IRepository<EligibilityCheck>>();
            _mockAuditLogRepo = new Mock<IRepository<AuditLog>>();
            _mockLogger = new Mock<ILogger<EligibilityAndAssignmentService>>();

            _mockUnitOfWork.Setup(u => u.Theses).Returns(_mockThesisRepo.Object);
            _mockUnitOfWork.Setup(u => u.AcademicPersons).Returns(_mockPersonRepo.Object);
            _mockUnitOfWork.Setup(u => u.ThesisAssignments).Returns(_mockAssignmentRepo.Object);
            _mockUnitOfWork.Setup(u => u.EligibilityChecks).Returns(_mockCheckRepo.Object);
            _mockUnitOfWork.Setup(u => u.AuditLogs).Returns(_mockAuditLogRepo.Object);

            _service = new EligibilityAndAssignmentService(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task AssignPersonAsync_WhenPersonIsAlreadySupervisor_AssigningAsReviewer_ThrowsInvalidOperationException()
        {
            // Arrange
            const int thesisId = 1;
            const int personId = 10;

            var thesis = new Thesis { Id = thesisId, Code = "LV-01", Field = "CNTT", ThesisStatus = ThesisStatus.InProgress };
            var person = new AcademicPerson { Id = personId, FullName = "GS. Nguyễn Văn A", ResearchField = "CNTT" };

            _mockThesisRepo.Setup(r => r.GetByIdAsync(thesisId)).ReturnsAsync(thesis);
            _mockPersonRepo.Setup(r => r.GetByIdAsync(personId)).ReturnsAsync(person);

            // Giả lập chuyên gia đã được phân công làm Supervisor
            var existing = new List<ThesisAssignment>
            {
                new ThesisAssignment { Id = 1, ThesisId = thesisId, PersonId = personId, Role = AssignmentRole.Supervisor }
            };
            _mockAssignmentRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThesisAssignment, bool>>>()))
                .ReturnsAsync(existing);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.AssignPersonAsync(thesisId, personId, AssignmentRole.Reviewer, false)
            );

            Assert.Contains("Xung đột lợi ích", ex.Message);
        }

        [Fact]
        public async Task AssignPersonAsync_WhenPersonIsAlreadyReviewer_AssigningAsSupervisor_ThrowsInvalidOperationException()
        {
            // Arrange
            const int thesisId = 1;
            const int personId = 10;

            var thesis = new Thesis { Id = thesisId, Code = "LV-01", Field = "CNTT", ThesisStatus = ThesisStatus.InProgress };
            var person = new AcademicPerson { Id = personId, FullName = "PGS. Trần Văn B", ResearchField = "CNTT" };

            _mockThesisRepo.Setup(r => r.GetByIdAsync(thesisId)).ReturnsAsync(thesis);
            _mockPersonRepo.Setup(r => r.GetByIdAsync(personId)).ReturnsAsync(person);

            // Giả lập chuyên gia đã được phân công làm Reviewer
            var existing = new List<ThesisAssignment>
            {
                new ThesisAssignment { Id = 2, ThesisId = thesisId, PersonId = personId, Role = AssignmentRole.Reviewer }
            };
            _mockAssignmentRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThesisAssignment, bool>>>()))
                .ReturnsAsync(existing);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.AssignPersonAsync(thesisId, personId, AssignmentRole.Supervisor, false)
            );

            Assert.Contains("Xung đột lợi ích", ex.Message);
        }

        [Fact]
        public async Task CheckEligibilityAsync_WhenPersonIsSupervisor_CheckingForReviewer_ReturnsIneligible()
        {
            // Arrange
            const int thesisId = 1;
            const int personId = 10;

            var thesis = new Thesis { Id = thesisId, Code = "LV-01", Field = "CNTT", ThesisType = ThesisType.LuanVan };
            var person = new AcademicPerson { Id = personId, FullName = "GS. Nguyễn Văn A", ResearchField = "CNTT" };

            _mockThesisRepo.Setup(r => r.GetByIdAsync(thesisId)).ReturnsAsync(thesis);
            _mockPersonRepo.Setup(r => r.GetByIdAsync(personId)).ReturnsAsync(person);

            var existing = new List<ThesisAssignment>
            {
                new ThesisAssignment { Id = 1, ThesisId = thesisId, PersonId = personId, Role = AssignmentRole.Supervisor }
            };
            _mockAssignmentRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThesisAssignment, bool>>>()))
                .ReturnsAsync(existing);

            // Act
            var result = await _service.CheckEligibilityAsync(thesisId, personId, AssignmentRole.Reviewer);

            // Assert
            Assert.Equal(CheckResult.Ineligible, result.Result);
            Assert.Contains("Xung đột lợi ích", result.Basis);
        }
    }
}
