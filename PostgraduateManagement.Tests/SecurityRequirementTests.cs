// File: PostgraduateManagement.Tests/SecurityRequirementTests.cs
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PostgraduateManagement.Controllers;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Repositories.Interfaces;
using PostgraduateManagement.Services.DTOs;
using PostgraduateManagement.Services.Implementations;
using PostgraduateManagement.Services.Interfaces;
using PostgraduateManagement.ViewModels;
using Xunit;

namespace PostgraduateManagement.Tests
{
    /// <summary>
    /// Kiểm thử các yêu cầu bảo mật: phạm vi danh sách đề tài, chính sách mật khẩu, giới hạn trang và kiểm tra tệp tải lên.
    /// </summary>
    public class SecurityRequirementTests
    {
        private static ClaimsPrincipal User(string name, string role) =>
            new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, name), new Claim(ClaimTypes.Role, role) }, "test"));

        private static Thesis ThesisOf(string studentEmail, string? supervisorEmail = null)
        {
            var thesis = new Thesis { Id = 1, Code = "T-1", Student = new PostgraduateStudent { Email = studentEmail } };
            if (supervisorEmail != null)
            {
                thesis.Assignments.Add(new ThesisAssignment
                {
                    Role = AssignmentRole.Supervisor,
                    Person = new AcademicPerson { Email = supervisorEmail }
                });
            }
            return thesis;
        }

        [Fact]
        public void ThesisAccess_StudentSeesOnlyOwnThesis()
        {
            var thesis = ThesisOf("an@student.lms.edu.vn");
            Assert.True(ThesisAccess.CanAccess(User("an@student.lms.edu.vn", "Student"), thesis));
            Assert.False(ThesisAccess.CanAccess(User("nam@student.lms.edu.vn", "Student"), thesis));
        }

        [Fact]
        public void ThesisAccess_FacultySeesOnlySupervisedThesis()
        {
            var thesis = ThesisOf("an@student.lms.edu.vn", "gv1@lms.edu.vn");
            Assert.True(ThesisAccess.CanAccess(User("gv1@lms.edu.vn", "Faculty"), thesis));
            Assert.False(ThesisAccess.CanAccess(User("gv2@lms.edu.vn", "Faculty"), thesis));
        }

        [Fact]
        public void ThesisAccess_ManagerSeesEverything()
        {
            Assert.True(ThesisAccess.CanAccess(User("m@lms.edu.vn", "Manager"), ThesisOf("an@student.lms.edu.vn")));
        }

        [Theory]
        [InlineData("short1!A", false)]
        [InlineData("alllowercase123!", false)]
        [InlineData("NoDigitsHere!!!", false)]
        [InlineData("NoSymbols12345A", false)]
        [InlineData("Str0ng!Passw0rd", true)]
        public async Task PasswordPolicy_RejectsWeakAndAcceptsStrong(string password, bool expected)
        {
            // Dùng đúng giá trị cấu hình như Program.cs
            var options = new IdentityOptions();
            options.Password.RequiredLength = 12;
            var manager = new UserManager<IdentityUser>(Mock.Of<IUserStore<IdentityUser>>(), Options.Create(options),
                null!, null!, null!, null!, null!, null!, null!);

            var result = await new PasswordValidator<IdentityUser>().ValidateAsync(manager, new IdentityUser("u"), password);

            Assert.Equal(expected, result.Succeeded);
        }

        [Fact]
        public async Task ThesisList_PageSizeIsCapped()
        {
            var uow = new Mock<IUnitOfWork>();
            var repo = new Mock<IThesisRepository>();
            uow.Setup(u => u.Theses).Returns(repo.Object);
            repo.Setup(r => r.GetThesesWithDetailsAsync(It.IsAny<string?>(), It.IsAny<ThesisStatus?>(), It.IsAny<ThesisType?>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IReadOnlyCollection<int>?>())).ReturnsAsync(new List<Thesis>());
            var service = new ThesisService(uow.Object, Mock.Of<ILogger<ThesisService>>());

            await service.GetThesesAsync(new ThesisFilterDto { PageSize = 100000 });

            repo.Verify(r => r.GetThesesWithDetailsAsync(null, null, null, 1, 50, null), Times.Once);
        }

        private static async Task<ModelStateHasError> UploadAsync(string fileName, byte[] content)
        {
            var thesis = ThesisOf("an@student.lms.edu.vn");
            var repo = new Mock<IThesisRepository>();
            repo.Setup(r => r.GetThesisWithFullDetailsAsync(1)).ReturnsAsync(thesis);
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(thesis);
            var uow = new Mock<IUnitOfWork>();
            uow.Setup(u => u.Theses).Returns(repo.Object);

            var controller = new DocumentController(Mock.Of<IDefenseAndDocumentService>(), uow.Object,
                Mock.Of<IWebHostEnvironment>(), Mock.Of<IConfiguration>(), Mock.Of<ILogger<DocumentController>>());
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = User("an@student.lms.edu.vn", "Student") }
            };

            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns(fileName);
            file.Setup(f => f.Length).Returns(content.Length);
            file.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(content));

            var result = await controller.Upload(new UploadDocumentViewModel { ThesisId = 1, File = file.Object });
            return new ModelStateHasError(result is ViewResult && !controller.ModelState.IsValid);
        }

        private record ModelStateHasError(bool Value);

        [Fact]
        public async Task Upload_TextFileRenamedToPdf_IsRejected()
        {
            Assert.True((await UploadAsync("luan-van.pdf", Encoding.UTF8.GetBytes("this is plain text"))).Value);
        }

        [Fact]
        public async Task Upload_ExecutableExtension_IsRejected()
        {
            Assert.True((await UploadAsync("virus.exe", new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03 })).Value);
        }
    }
}
