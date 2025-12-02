using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.UnitTests.FakeData;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.UnitTests
{
    public class TutorApplicationUnitTests
    {
        private readonly Mock<ITutorApplicationRepository> _repoMock;
        private readonly TutorApplicationService _service;

        public TutorApplicationUnitTests()
        {
            _repoMock = new Mock<ITutorApplicationRepository>();
            _service = new TutorApplicationService(_repoMock.Object, new FakeEmailService());
        }

        [Fact]
        public async Task TutorApply_Throws_WhenClassRequestNotFound_UnitTest()
        {
            _repoMock.Setup(r => r.GetClassRequestByIdAsync(1)).ReturnsAsync((ClassRequest)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.TutorApplyAsync(1, "tutor@gmail.com", "I want to apply"));
        }

        [Fact]
        public async Task TutorApply_Throws_WhenClassRequestNotOpen_UnitTest()
        {
            var classRequest = new ClassRequest { Id = 1, Status = ClassRequestStatus.Pending };
            _repoMock.Setup(r => r.GetClassRequestByIdAsync(1)).ReturnsAsync(classRequest);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.TutorApplyAsync(1, "tutor@gmail.com", "I want to apply"));
        }

        [Fact]
        public async Task TutorApply_Throws_WhenTutorNotFound_UnitTest()
        {
            var classRequest = new ClassRequest { Id = 1, Status = ClassRequestStatus.Open };
            _repoMock.Setup(r => r.GetClassRequestByIdAsync(1)).ReturnsAsync(classRequest);
            _repoMock.Setup(r => r.GetTutorByEmailAsync("tutor@gmail.com")).ReturnsAsync((TutorProfile)null);

            await Assert.ThrowsAsync<Exception>(() =>
                _service.TutorApplyAsync(1, "tutor@gmail.com", "I want to apply"));
        }

        [Fact]
        public async Task TutorApply_Throws_WhenTutorAlreadyApplied_UnitTest()
        {
            var classRequest = new ClassRequest { Id = 1, Status = ClassRequestStatus.Open };
            var tutor = new TutorProfile { Id = 10, UserEmail = "tutor@gmail.com" };

            _repoMock.Setup(r => r.GetClassRequestByIdAsync(1)).ReturnsAsync(classRequest);
            _repoMock.Setup(r => r.GetTutorByEmailAsync("tutor@gmail.com")).ReturnsAsync(tutor);
            _repoMock.Setup(r => r.HasAppliedAsync(1, tutor.Id)).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.TutorApplyAsync(1, "tutor@gmail.com", "I want to apply"));
        }

        [Fact]
        public async Task TutorApply_Success_UnitTest()
        {
            var classRequest = new ClassRequest { Id = 1, Status = ClassRequestStatus.Open };
            var tutor = new TutorProfile { Id = 10, UserEmail = "tutor@gmail.com" };

            _repoMock.Setup(r => r.GetClassRequestByIdAsync(1)).ReturnsAsync(classRequest);
            _repoMock.Setup(r => r.GetTutorByEmailAsync("tutor@gmail.com")).ReturnsAsync(tutor);
            _repoMock.Setup(r => r.HasAppliedAsync(1, tutor.Id)).ReturnsAsync(false);
            _repoMock.Setup(r => r.AddApplicationAsync(It.IsAny<TutorApplication>()))
                     .Returns(Task.CompletedTask);

            await _service.TutorApplyAsync(1, "tutor@gmail.com", "I want to apply");

            _repoMock.Verify(r => r.AddApplicationAsync(It.Is<TutorApplication>(
                a => a.ClassRequestId == 1 && a.TutorId == 10 && a.Message == "I want to apply"
            )), Times.Once);
        }
    }
}
