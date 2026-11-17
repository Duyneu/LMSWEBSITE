// File: PostgraduateManagement.Tests/WorkflowRuleTests.cs
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using PostgraduateManagement.Controllers;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Repositories.Interfaces;
using PostgraduateManagement.Services.Implementations;
using PostgraduateManagement.Services.Interfaces;
using Xunit;

namespace PostgraduateManagement.Tests
{
    /// <summary>
    /// Kiểm thử quy tắc quy trình M05: mỗi hội đồng một Chủ tịch, chỉ lưu trữ đề tài đã hoàn thành.
    /// </summary>
    public class WorkflowRuleTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IThesisRepository> _theses = new();
        private readonly Mock<IAcademicPersonRepository> _persons = new();
        private readonly Mock<IRepository<ThesisAssignment>> _assignments = new();

        public WorkflowRuleTests()
        {
            _uow.Setup(u => u.Theses).Returns(_theses.Object);
            _uow.Setup(u => u.AcademicPersons).Returns(_persons.Object);
            _uow.Setup(u => u.ThesisAssignments).Returns(_assignments.Object);
        }

        [Fact]
        public async Task AssignPerson_SecondChairOnSameThesis_Throws()
        {
            _theses.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Thesis { Id = 1, Code = "LV-1" });
            _persons.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new AcademicPerson { Id = 2, FullName = "B" });
            var existingChair = new ThesisAssignment { ThesisId = 1, PersonId = 9, Role = AssignmentRole.Committee, CommitteePosition = CommitteePosition.Chair };
            // Mock trả về theo đúng biểu thức lọc của service
            _assignments.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThesisAssignment, bool>>>()))
                        .ReturnsAsync((Expression<Func<ThesisAssignment, bool>> p) => new[] { existingChair }.Where(p.Compile()));

            var service = new EligibilityAndAssignmentService(_uow.Object, Mock.Of<ILogger<EligibilityAndAssignmentService>>());

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AssignPersonAsync(1, 2, AssignmentRole.Committee, false, CommitteePosition.Chair));
            Assert.Contains("Chủ tịch", ex.Message);
            _assignments.Verify(r => r.AddAsync(It.IsAny<ThesisAssignment>()), Times.Never);
        }

        [Fact]
        public async Task Archive_WhenThesisNotCompleted_DoesNotTransition()
        {
            _theses.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new Thesis { Id = 5, Code = "LV-5", ThesisStatus = ThesisStatus.Defended });
            var thesisService = new Mock<IThesisService>();
            var controller = new ThesisController(thesisService.Object, _uow.Object, Mock.Of<ILogger<ThesisController>>())
            {
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            };

            var result = await controller.Archive(5);

            Assert.IsType<RedirectToActionResult>(result);
            thesisService.Verify(s => s.TransitionStatusAsync(It.IsAny<int>(), It.IsAny<ThesisStatus>()), Times.Never);
            Assert.NotNull(controller.TempData["ErrorMessage"]);
        }
    }
}
