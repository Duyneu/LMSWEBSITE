// File: PostgraduateManagement.Tests/DocumentScopeTests.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PostgraduateManagement.Controllers;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Repositories.Interfaces;
using PostgraduateManagement.Services.Interfaces;
using Xunit;

namespace PostgraduateManagement.Tests
{
    /// <summary>
    /// Kiểm thử phạm vi truy cập tài liệu: học viên chỉ xem và nộp tài liệu cho đề tài của chính mình.
    /// </summary>
    public class DocumentScopeTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IThesisRepository> _theses = new();
        private readonly Thesis _own = new() { Id = 4, Code = "LV-4", Student = new PostgraduateStudent { Email = "an.nv@student.lms.edu.vn" } };
        private readonly Thesis _other = new() { Id = 1, Code = "LA-1", Student = new PostgraduateStudent { Email = "nam.lh@student.lms.edu.vn" } };

        public DocumentScopeTests()
        {
            _uow.Setup(u => u.Theses).Returns(_theses.Object);
            _theses.Setup(r => r.GetThesisWithFullDetailsAsync(4)).ReturnsAsync(_own);
            _theses.Setup(r => r.GetThesisWithFullDetailsAsync(1)).ReturnsAsync(_other);
            _theses.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { _own, _other });
        }

        private DocumentController StudentController()
        {
            var controller = new DocumentController(
                Mock.Of<IDefenseAndDocumentService>(), _uow.Object, Mock.Of<IWebHostEnvironment>(),
                Mock.Of<IConfiguration>(), Mock.Of<ILogger<DocumentController>>());
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "an.nv@student.lms.edu.vn"),
                new Claim(ClaimTypes.Role, "Student")
            }, "test");
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };
            return controller;
        }

        [Fact]
        public async Task Index_StudentOpensAnotherThesis_IsForbidden()
        {
            Assert.IsType<ForbidResult>(await StudentController().Index(1));
        }

        [Fact]
        public async Task Upload_StudentOnAnotherThesis_IsForbidden()
        {
            Assert.IsType<ForbidResult>(await StudentController().Upload(1));
        }

        [Fact]
        public async Task Upload_StudentOnOwnThesis_ShowsForm()
        {
            Assert.IsType<ViewResult>(await StudentController().Upload(4));
        }
    }
}
