// File: PostgraduateManagement.Tests/ProfileControllerTests.cs
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using PostgraduateManagement.Controllers;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Repositories.Interfaces;
using PostgraduateManagement.ViewModels;
using Xunit;

namespace PostgraduateManagement.Tests
{
    /// <summary>
    /// Kiểm thử quy tắc bảo toàn dữ liệu khi quản lý hồ sơ học viên và chuyên gia:
    /// không xóa bản ghi đang được tham chiếu, không trùng mã dùng chung.
    /// </summary>
    public class ProfileControllerTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IRepository<PostgraduateStudent>> _students = new();
        private readonly Mock<IThesisRepository> _theses = new();
        private readonly Mock<IAcademicPersonRepository> _experts = new();

        public ProfileControllerTests()
        {
            _uow.Setup(u => u.PostgraduateStudents).Returns(_students.Object);
            _uow.Setup(u => u.Theses).Returns(_theses.Object);
            _uow.Setup(u => u.AcademicPersons).Returns(_experts.Object);
        }

        private static T WithTempData<T>(T controller) where T : Controller
        {
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            return controller;
        }

        [Fact]
        public async Task StudentDelete_WhenStudentHasThesis_DoesNotRemove()
        {
            var student = new PostgraduateStudent { Id = 1, FullName = "Nguyễn Văn An" };
            _students.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(student);
            _theses.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Thesis, bool>>>()))
                   .ReturnsAsync(new[] { new Thesis { Id = 9, StudentId = 1 } });

            var controller = WithTempData(new StudentController(_uow.Object));
            var result = await controller.Delete(1);

            Assert.IsType<RedirectToActionResult>(result);
            _students.Verify(r => r.Remove(It.IsAny<PostgraduateStudent>()), Times.Never);
            _uow.Verify(u => u.CompleteAsync(), Times.Never);
            Assert.NotNull(controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task StudentDelete_WhenNoThesis_Removes()
        {
            var student = new PostgraduateStudent { Id = 2, FullName = "Trần Thị B" };
            _students.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(student);
            _theses.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Thesis, bool>>>()))
                   .ReturnsAsync(Array.Empty<Thesis>());

            var controller = WithTempData(new StudentController(_uow.Object));
            await controller.Delete(2);

            _students.Verify(r => r.Remove(student), Times.Once);
            _uow.Verify(u => u.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task ExpertCreate_WhenCodeAlreadyExists_ReturnsFormWithError()
        {
            _experts.Setup(r => r.FindAsync(It.IsAny<Expression<Func<AcademicPerson, bool>>>()))
                    .ReturnsAsync(new[] { new AcademicPerson { Id = 5, SharedPersonCode = "CB01" } });

            var controller = WithTempData(new ExpertController(_uow.Object));
            var model = new ExpertFormViewModel { SharedPersonCode = "CB01", FullName = "X", ResearchField = "AI", Email = "x@a.vn" };
            var result = await controller.Create(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Form", view.ViewName);
            Assert.True(controller.ModelState.ContainsKey(nameof(ExpertFormViewModel.SharedPersonCode)));
            _experts.Verify(r => r.AddAsync(It.IsAny<AcademicPerson>()), Times.Never);
        }
    }
}
