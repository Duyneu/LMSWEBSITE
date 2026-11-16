// File: PostgraduateManagement.Tests/ThesisServiceTests.cs
using Microsoft.Extensions.Logging;
using Moq;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Repositories.Interfaces;
using PostgraduateManagement.Services.DTOs;
using PostgraduateManagement.Services.Implementations;
using PostgraduateManagement.Services.Interfaces;
using Xunit;

namespace PostgraduateManagement.Tests
{
    /// <summary>
    /// Kiểm thử đơn vị cho ThesisService và quy tắc nghiệp vụ chặn đề tài NCKH (IsGraduationCapstone = false)
    /// không được phép tham gia vào quy trình bảo vệ tốt nghiệp (Module M05).
    /// </summary>
    public class ThesisServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IThesisRepository> _mockThesisRepo;
        private readonly Mock<ILogger<ThesisService>> _mockLogger;
        private readonly ThesisService _thesisService;

        public ThesisServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockThesisRepo = new Mock<IThesisRepository>();
            _mockLogger = new Mock<ILogger<ThesisService>>();

            _mockUnitOfWork.Setup(u => u.Theses).Returns(_mockThesisRepo.Object);

            _thesisService = new ThesisService(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ValidateForDefenseAsync_WhenIsGraduationCapstoneIsFalse_ThrowsInvalidOperationException()
        {
            // Arrange: Đề tài NCKH thông thường (IsGraduationCapstone = false)
            var nckhThesis = new Thesis
            {
                Id = 101,
                Code = "NCKH-2026-001",
                Title = "Nghiên cứu ứng dụng Blockchain trong quản lý đào tạo",
                ThesisType = ThesisType.LuanVan,
                Field = "Công nghệ thông tin",
                ThesisStatus = ThesisStatus.InProgress,
                IsGraduationCapstone = false,
                StudentId = 1
            };

            _mockThesisRepo.Setup(r => r.GetByIdAsync(101)).ReturnsAsync(nckhThesis);

            // Act & Assert: Phải bị từ chối với InvalidOperationException
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _thesisService.ValidateForDefenseAsync(101)
            );

            Assert.Contains("nghiên cứu khoa học không thuộc đối tượng bảo vệ tốt nghiệp", exception.Message);
        }

        [Fact]
        public async Task ValidateForDefenseAsync_WhenIsGraduationCapstoneIsTrueAndStatusInProgress_ReturnsTrue()
        {
            // Arrange: Khóa luận tốt nghiệp hợp lệ (IsGraduationCapstone = true, Status = InProgress)
            var capstoneThesis = new Thesis
            {
                Id = 102,
                Code = "LVTHS-2026-002",
                Title = "Nghiên cứu và xây dựng hệ thống quản lý sau đại học",
                ThesisType = ThesisType.LuanVan,
                Field = "Khoa học máy tính",
                ThesisStatus = ThesisStatus.InProgress,
                IsGraduationCapstone = true,
                StudentId = 2
            };

            _mockThesisRepo.Setup(r => r.GetByIdAsync(102)).ReturnsAsync(capstoneThesis);

            // Act
            var result = await _thesisService.ValidateForDefenseAsync(102);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateForDefenseAsync_WhenThesisNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockThesisRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Thesis?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _thesisService.ValidateForDefenseAsync(999)
            );
        }

        [Theory]
        [InlineData(ThesisStatus.Draft)]
        [InlineData(ThesisStatus.Registered)]
        public async Task ValidateForDefenseAsync_WhenStatusIsDraftOrRegistered_ThrowsInvalidOperationException(ThesisStatus status)
        {
            // Arrange: Đề tài tốt nghiệp nhưng chưa qua phê duyệt đề cương
            var earlyThesis = new Thesis
            {
                Id = 103,
                Code = "LVTHS-2026-003",
                Title = "Nghiên cứu mô hình ngôn ngữ lớn cho hỗ trợ học tập",
                ThesisType = ThesisType.LuanVan,
                Field = "Trí tuệ nhân tạo",
                ThesisStatus = status,
                IsGraduationCapstone = true,
                StudentId = 3
            };

            _mockThesisRepo.Setup(r => r.GetByIdAsync(103)).ReturnsAsync(earlyThesis);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _thesisService.ValidateForDefenseAsync(103)
            );

            Assert.Contains("chưa được phê duyệt đề cương", ex.Message);
        }

        [Theory]
        [InlineData(ThesisStatus.SubmittedForReview)]
        [InlineData(ThesisStatus.Defended)]
        public async Task TransitionStatusAsync_WhenIsGraduationCapstoneIsFalseAndTargetingDefense_ThrowsInvalidOperationException(ThesisStatus targetStatus)
        {
            // Arrange: Đề tài NCKH (IsGraduationCapstone = false)
            var nckhThesis = new Thesis
            {
                Id = 104,
                Code = "NCKH-2026-004",
                Title = "Nghiên cứu các thuật toán tối ưu hóa trong phân bổ tài nguyên",
                ThesisType = ThesisType.LuanVan,
                Field = "Khoa học máy tính",
                ThesisStatus = ThesisStatus.InProgress,
                IsGraduationCapstone = false,
                StudentId = 4
            };

            _mockThesisRepo.Setup(r => r.GetByIdAsync(104)).ReturnsAsync(nckhThesis);

            // Act & Assert: Chuyển sang trạng thái liên quan đến bảo vệ phải bị chặn
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _thesisService.TransitionStatusAsync(104, targetStatus)
            );

            Assert.Contains("nghiên cứu khoa học không thuộc đối tượng bảo vệ tốt nghiệp", ex.Message);
        }

        [Fact]
        public async Task TransitionStatusAsync_WhenValidCapstoneThesis_TransitionsSuccessfully()
        {
            // Arrange
            var capstoneThesis = new Thesis
            {
                Id = 105,
                Code = "LVTHS-2026-005",
                Title = "Xây dựng hạ tầng Cloud và Microservices cho đại học thông minh",
                ThesisType = ThesisType.LuanVan,
                Field = "Kỹ thuật phần mềm",
                ThesisStatus = ThesisStatus.InProgress,
                IsGraduationCapstone = true,
                StudentId = 5
            };

            _mockThesisRepo.Setup(r => r.GetByIdAsync(105)).ReturnsAsync(capstoneThesis);
            _mockUnitOfWork.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _thesisService.TransitionStatusAsync(105, ThesisStatus.SubmittedForReview);

            // Assert
            Assert.True(result);
            Assert.Equal(ThesisStatus.SubmittedForReview, capstoneThesis.ThesisStatus);
            _mockThesisRepo.Verify(r => r.Update(capstoneThesis), Times.Once);
            _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ScheduleDefenseAsync_WhenIsGraduationCapstoneIsFalse_IsRejectedByDefenseService()
        {
            // Arrange
            var nckhThesis = new Thesis
            {
                Id = 106,
                Code = "NCKH-2026-006",
                Title = "Đánh giá hiệu năng mạng 5G trong môi trường đô thị",
                ThesisType = ThesisType.LuanVan,
                Field = "Mạng máy tính và truyền thông",
                ThesisStatus = ThesisStatus.InProgress,
                IsGraduationCapstone = false,
                StudentId = 6
            };

            _mockThesisRepo.Setup(r => r.GetByIdAsync(106)).ReturnsAsync(nckhThesis);

            var mockDefenseLogger = new Mock<ILogger<DefenseAndDocumentService>>();
            var defenseService = new DefenseAndDocumentService(_mockUnitOfWork.Object, _thesisService, mockDefenseLogger.Object);

            var scheduleDto = new ScheduleDefenseDto
            {
                ThesisId = 106,
                ScheduledAt = DateTime.UtcNow.AddDays(7),
                Location = "Phòng họp Hội đồng sau đại học A1-302",
                CommitteeNotes = "Phiên bảo vệ chính thức"
            };

            // Act & Assert: Lập lịch cho đề tài NCKH phải bị từ chối
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => defenseService.ScheduleDefenseAsync(scheduleDto)
            );

            Assert.Contains("nghiên cứu khoa học không thuộc đối tượng bảo vệ tốt nghiệp", ex.Message);
        }
    }
}
