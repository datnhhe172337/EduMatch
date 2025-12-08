using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests;
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
    public class ClassRquestsUnitTests
    {
        private readonly Mock<IClassRequestRepository> _repo;
        private readonly ClassRequestService _service;

        public ClassRquestsUnitTests()
        {
            _repo = new Mock<IClassRequestRepository>();
            _service = new ClassRequestService(
                _repo.Object,
                new FakeEmailService()
             );
        }

        [Fact]
        public async Task CreateRequest_Success_WithValidData_UnitTest()
        {
            // Arrange
            var dto = new ClassCreateRequest
            {
                SubjectId = 1,
                Title = "Math Grade 9",
                LevelId = 2,
                LearningGoal = "Improve math",
                TutorRequirement = "Experienced tutor",
                Mode = 1,
                ProvinceId = 10,
                SubDistrictId = 20,
                AddressLine = "123 Hanoi",
                ExpectedStartDate = DateOnly.FromDayNumber(3),
                ExpectedSessions = 20,
                TargetUnitPriceMin = 150000,
                TargetUnitPriceMax = 250000,
                Slots = new List<ClassRequestSlotAvailabilityDto>
                {
                    new ClassRequestSlotAvailabilityDto{ DayOfWeek = 2, SlotId = 3 }
                }
            };

            var expected = new ClassRequest { Id = 999, LearnerEmail = "test@gmail.com" };

            _repo.Setup(r => r.CreateRequestToOpenClassAsync(It.IsAny<ClassRequest>(), It.IsAny<List<ClassRequestSlotsAvailability>>()))
                 .ReturnsAsync(expected);

            // Act
            var result = await _service.CreateRequestToOpenClassAsync(dto, "test@gmail.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(999, result.Id);
            _repo.Verify(r => r.CreateRequestToOpenClassAsync(It.IsAny<ClassRequest>(), It.IsAny<List<ClassRequestSlotsAvailability>>()), Times.Once);
        }

        [Fact]
        public async Task CreateRequest_Success_WithoutSlots_UnitTest()
        {
            var dto = new ClassCreateRequest { SubjectId = 1, Slots = null };

            _repo.Setup(r => r.CreateRequestToOpenClassAsync(It.IsAny<ClassRequest>(), It.Is<List<ClassRequestSlotsAvailability>>(s => s.Count == 0)))
                 .ReturnsAsync(new ClassRequest());

            var result = await _service.CreateRequestToOpenClassAsync(dto, "learner@gmail.com");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateRequest_RepoThrowsException_UnitTest()
        {
            var dto = new ClassCreateRequest { SubjectId = 1 };

            _repo.Setup(r => r.CreateRequestToOpenClassAsync(It.IsAny<ClassRequest>(), It.IsAny<List<ClassRequestSlotsAvailability>>()))
                 .ThrowsAsync(new Exception("DB error"));

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.CreateRequestToOpenClassAsync(dto, "learner@gmail.com")
            );

            Assert.Equal("DB error", ex.Message);
        }

        [Fact]
        public async Task CancelClassRequest_Throws_WhenRequestNotFound_UnitTest()
        {
            // Arrange
            _repo.Setup(r => r.GetClassRequestByIdAsync(1))
                 .ReturnsAsync((ClassRequest)null);

            // Act + Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.CancelClassRequestAsync(1, "user@gmail.com", new CancelClassRequestDto()));
        }

        [Fact]
        public async Task CancelClassRequest_Throws_WhenUserIsNotOwner_UnitTest()
        {
            var request = new ClassRequest { LearnerEmail = "owner@gmail.com", Status = ClassRequestStatus.Open };

            _repo.Setup(r => r.GetClassRequestByIdAsync(1)).ReturnsAsync(request);

            await Assert.ThrowsAsync<Exception>(() =>
                _service.CancelClassRequestAsync(1, "other@gmail.com", new CancelClassRequestDto()));
        }

        [Fact]
        public async Task CancelClassRequest_Throws_WhenStatusNotOpen_UnitTest()
        {
            var request = new ClassRequest { LearnerEmail = "user@gmail.com", Status = ClassRequestStatus.Cancelled };

            _repo.Setup(r => r.GetClassRequestByIdAsync(1)).ReturnsAsync(request);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CancelClassRequestAsync(1, "user@gmail.com", new CancelClassRequestDto()));
        }

        [Fact]
        public async Task CancelClassRequest_Success_WithCustomReason_UnitTest()
        {
            var request = new ClassRequest
            {
                Id = 1,
                LearnerEmail = "user@gmail.com",
                Status = ClassRequestStatus.Open
            };

            var dto = new CancelClassRequestDto { Reason = "No longer needed" };

            _repo.Setup(r => r.GetClassRequestByIdAsync(1)).ReturnsAsync(request);
            _repo.Setup(r => r.UpdateStatusAsync(It.IsAny<ClassRequest>())).Returns(Task.CompletedTask);

            await _service.CancelClassRequestAsync(1, "user@gmail.com", dto);

            Assert.Equal(ClassRequestStatus.Cancelled, request.Status);
            Assert.Equal("No longer needed", request.CancelReason);

            _repo.Verify(r => r.UpdateStatusAsync(request), Times.Once);
        }

        [Fact]
        public async Task CancelClassRequest_Success_DefaultReason_UnitTest()
        {
            var request = new ClassRequest
            {
                LearnerEmail = "user@gmail.com",
                Status = ClassRequestStatus.Open
            };

            _repo.Setup(r => r.GetClassRequestByIdAsync(1)).ReturnsAsync(request);
            _repo.Setup(r => r.UpdateStatusAsync(It.IsAny<ClassRequest>())).Returns(Task.CompletedTask);

            await _service.CancelClassRequestAsync(1, "user@gmail.com", new CancelClassRequestDto());

            Assert.Equal("Cancelled by learner.", request.CancelReason);
            Assert.Equal(ClassRequestStatus.Cancelled, request.Status);

            _repo.Verify(r => r.UpdateStatusAsync(request), Times.Once);
        }

        [Fact]
        public async Task DeleteClassRequest_Throws_WhenRequestNotFound_UnitTest()
        {
            // Arrange
            _repo.Setup(r => r.GetClassRequestByIdAsync(1))
                 .ReturnsAsync((ClassRequest)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.DeleteClassRequestAsync(1, "user@gmail.com"));
        }

        [Fact]
        public async Task DeleteClassRequest_Throws_WhenUserIsNotOwner_UnitTest()
        {
            var request = new ClassRequest { LearnerEmail = "owner@gmail.com" };

            _repo.Setup(r => r.GetClassRequestByIdAsync(1)).ReturnsAsync(request);

            await Assert.ThrowsAsync<Exception>(() =>
                _service.DeleteClassRequestAsync(1, "other@gmail.com"));
        }

        [Fact]
        public async Task DeleteClassRequest_Success_UnitTest()
        {
            var request = new ClassRequest { LearnerEmail = "user@gmail.com" };

            _repo.Setup(r => r.GetClassRequestByIdAsync(1)).ReturnsAsync(request);
            _repo.Setup(r => r.DeleteRequestToOpenClassAsync(request)).Returns(Task.CompletedTask);

            var result = await _service.DeleteClassRequestAsync(1, "user@gmail.com");

            Assert.True(result);
            _repo.Verify(r => r.DeleteRequestToOpenClassAsync(request), Times.Once);
        }

        [Fact]
        public async Task UpdateClassRequest_Throws_WhenRequestNotFound_UnitTest()
        {
            // Arrange
            _repo.Setup(r => r.GetClassRequestByIdAsync(1)).ReturnsAsync((ClassRequest)null);

            var updateDto = new UpdateClassRequest { Title = "Updated Title", SubjectId = 1 };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateClassRequestAsync(1, updateDto, "user@gmail.com"));
        }

        [Fact]
        public async Task UpdateClassRequest_Throws_WhenUserIsNotOwner_UnitTest()
        {
            var existing = new ClassRequest { LearnerEmail = "owner@gmail.com" };

            _repo.Setup(r => r.GetClassRequestByIdAsync(1)).ReturnsAsync(existing);

            var updateDto = new UpdateClassRequest { Title = "Updated Title", SubjectId = 1 };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.UpdateClassRequestAsync(1, updateDto, "other@gmail.com"));
        }

        [Fact]
        public async Task UpdateClassRequest_Success_UnitTest()
        {
            var existing = new ClassRequest { Id = 1, LearnerEmail = "user@gmail.com" };

            _repo.Setup(r => r.GetClassRequestByIdAsync(1)).ReturnsAsync(existing);
            _repo.Setup(r => r.UpdateRequestToOpenClassAsync(existing, It.IsAny<List<ClassRequestSlotsAvailability>>()))
                 .Returns(Task.CompletedTask);

            var updateDto = new UpdateClassRequest
            {
                SubjectId = 2,
                Title = "Updated Title",
                LevelId = 3,
                LearningGoal = "Improve",
                TutorRequirement = "Experienced",
                Mode = 1,
                ProvinceId = 10,
                SubDistrictId = 20,
                AddressLine = "123 Hanoi",
                ExpectedStartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                ExpectedSessions = 10,
                TargetUnitPriceMin = 100000,
                TargetUnitPriceMax = 200000,
                Slots = new List<ClassRequestSlotAvailabilityDto>
                {
                    new ClassRequestSlotAvailabilityDto { DayOfWeek = 1, SlotId = 2 }
                }
            };

            var result = await _service.UpdateClassRequestAsync(1, updateDto, "user@gmail.com");

            Assert.True(result);
            _repo.Verify(r => r.UpdateRequestToOpenClassAsync(
                It.Is<ClassRequest>(c => c.Title == "Updated Title" && c.SubjectId == 2),
                It.Is<List<ClassRequestSlotsAvailability>>(s => s.Count == 1)
            ), Times.Once);
        }

    }
}
