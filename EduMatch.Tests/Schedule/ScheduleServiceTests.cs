using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.Schedule;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.Tests.Common;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.Tests
{
	/// <summary>
	/// Test class cho ScheduleService
	/// </summary>
	public class ScheduleServiceTests
	{
		private Mock<IScheduleRepository> _scheduleRepositoryMock;
		private Mock<IBookingService> _bookingServiceMock;
		private Mock<IMeetingSessionService> _meetingSessionServiceMock;
		private Mock<ITutorAvailabilityService> _tutorAvailabilityServiceMock;
		private Mock<ITutorProfileService> _tutorProfileServiceMock;
		private IMapper _mapper;
		private ScheduleService _service;

		/// <summary>
		/// Khởi tạo các mock objects và service trước mỗi test
		/// </summary>
		[SetUp]
		public void Setup()
		{
			_scheduleRepositoryMock = new Mock<IScheduleRepository>();
			_bookingServiceMock = new Mock<IBookingService>();
			_meetingSessionServiceMock = new Mock<IMeetingSessionService>();
			_tutorAvailabilityServiceMock = new Mock<ITutorAvailabilityService>();
			_tutorProfileServiceMock = new Mock<ITutorProfileService>();

			var config = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = config.CreateMapper();

			_service = new ScheduleService(
				_scheduleRepositoryMock.Object,
				_bookingServiceMock.Object,
				_meetingSessionServiceMock.Object,
				_tutorAvailabilityServiceMock.Object,
				_tutorProfileServiceMock.Object,
				_mapper
			);
		}

		/// <summary>
		/// Test GetByIdAsync với ID khác nhau
		/// </summary>
		[Test]
		[TestCase(1, true)]
		[TestCase(999, false)]
		public async Task GetByIdAsync_WithDifferentIds_ReturnsExpectedResult(int id, bool shouldExist)
		{
			// Arrange
			EduMatch.DataAccessLayer.Entities.Schedule? entity = shouldExist ? FakeDataFactory.CreateFakeSchedule(id) : null;

			_scheduleRepositoryMock
				.Setup(r => r.GetByIdAsync(id))
				.ReturnsAsync(entity);

			// Act
			var result = await _service.GetByIdAsync(id);

			// Assert
			if (shouldExist)
			{
				Assert.That(result, Is.Not.Null);
				Assert.That(result!.Id, Is.EqualTo(id));
			}
			else
			{
				Assert.That(result, Is.Null);
			}
		}

		/// <summary>
		/// Test GetByIdAsync với invalid ID
		/// </summary>
		[Test]
		[TestCase(0)]
		[TestCase(-1)]
		public void GetByIdAsync_WithInvalidId_ThrowsArgumentException(int id)
		{
			// Act & Assert
			var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetByIdAsync(id));
			Assert.That(exception.Message, Does.Contain("ID must be greater than 0"));
		}

		/// <summary>
		/// Test GetByAvailabilityIdAsync với AvailabilityId khác nhau
		/// </summary>
		[Test]
		[TestCase(1, true)]
		[TestCase(999, false)]
		public async Task GetByAvailabilityIdAsync_WithDifferentAvailabilityIds_ReturnsExpectedResult(int availabilityId, bool shouldExist)
		{
			// Arrange
			EduMatch.DataAccessLayer.Entities.Schedule? entity = shouldExist ? FakeDataFactory.CreateFakeSchedule(1, availabilityId) : null;

			_scheduleRepositoryMock
				.Setup(r => r.GetByAvailabilityIdAsync(availabilityId))
				.ReturnsAsync(entity);

			// Act
			var result = await _service.GetByAvailabilityIdAsync(availabilityId);

			// Assert
			if (shouldExist)
			{
				Assert.That(result, Is.Not.Null);
				Assert.That(result!.AvailabilitiId, Is.EqualTo(availabilityId));
			}
			else
			{
				Assert.That(result, Is.Null);
			}
		}

		/// <summary>
		/// Test GetByAvailabilityIdAsync với invalid AvailabilityId
		/// </summary>
		[Test]
		[TestCase(0)]
		[TestCase(-1)]
		public void GetByAvailabilityIdAsync_WithInvalidAvailabilityId_ThrowsArgumentException(int availabilityId)
		{
			// Act & Assert
			var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetByAvailabilityIdAsync(availabilityId));
			Assert.That(exception.Message, Does.Contain("AvailabilitiId must be greater than 0"));
		}

		/// <summary>
		/// Test GetAllByBookingIdAndStatusNoPagingAsync với bookingId và status khác nhau
		/// </summary>
		[Test]
		[TestCase(1, null, 2)]
		[TestCase(1, ScheduleStatus.Upcoming, 1)]
		[TestCase(999, null, 0)]
		public async Task GetAllByBookingIdAndStatusNoPagingAsync_WithDifferentParams_ReturnsExpectedList(int bookingId, ScheduleStatus? status, int expectedCount)
		{
			// Arrange
			var schedules = expectedCount > 0
				? new List<EduMatch.DataAccessLayer.Entities.Schedule>
				{
					FakeDataFactory.CreateFakeSchedule(1, bookingId: bookingId),
					FakeDataFactory.CreateFakeSchedule(2, bookingId: bookingId)
				}.Take(expectedCount).ToList()
				: new List<EduMatch.DataAccessLayer.Entities.Schedule>();

			int? statusInt = status.HasValue ? (int?)status.Value : null;

			_scheduleRepositoryMock
				.Setup(r => r.GetAllByBookingIdAndStatusNoPagingAsync(bookingId, statusInt))
				.ReturnsAsync(schedules);

			// Act
			var result = await _service.GetAllByBookingIdAndStatusNoPagingAsync(bookingId, status);

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Count, Is.EqualTo(expectedCount));
		}

		/// <summary>
		/// Test GetAllByLearnerEmailAsync với email khác nhau
		/// </summary>
		[Test]
		[TestCase("learner@example.com", 2)]
		[TestCase("learner2@example.com", 0)]
		public async Task GetAllByLearnerEmailAsync_WithDifferentEmails_ReturnsExpectedList(string learnerEmail, int expectedCount)
		{
			// Arrange
			var schedules = expectedCount > 0
				? new List<EduMatch.DataAccessLayer.Entities.Schedule>
				{
					FakeDataFactory.CreateFakeSchedule(1, bookingId: 1),
					FakeDataFactory.CreateFakeSchedule(2, bookingId: 2)
				}
				: new List<EduMatch.DataAccessLayer.Entities.Schedule>();

			_scheduleRepositoryMock
				.Setup(r => r.GetAllByLearnerEmailAsync(learnerEmail, null, null, null))
				.ReturnsAsync(schedules);

			// Act
			var result = await _service.GetAllByLearnerEmailAsync(learnerEmail);

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Count, Is.EqualTo(expectedCount));
		}

		/// <summary>
		/// Test GetAllByTutorEmailAsync với email khác nhau
		/// </summary>
		[Test]
		[TestCase("tutor@example.com", 2)]
		[TestCase("tutor2@example.com", 0)]
		public async Task GetAllByTutorEmailAsync_WithDifferentEmails_ReturnsExpectedList(string tutorEmail, int expectedCount)
		{
			// Arrange
			var schedules = expectedCount > 0
				? new List<EduMatch.DataAccessLayer.Entities.Schedule>
				{
					FakeDataFactory.CreateFakeSchedule(1),
					FakeDataFactory.CreateFakeSchedule(2)
				}
				: new List<EduMatch.DataAccessLayer.Entities.Schedule>();

			_scheduleRepositoryMock
				.Setup(r => r.GetAllByTutorEmailAsync(tutorEmail, null, null, null))
				.ReturnsAsync(schedules);

			// Act
			var result = await _service.GetAllByTutorEmailAsync(tutorEmail);

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Count, Is.EqualTo(expectedCount));
		}

		/// <summary>
		/// Test data cho CreateAsync
		/// </summary>
		private static IEnumerable<TestCaseData> CreateAsyncTestCases()
		{
			// Valid case
			var validAvailabilityEntity = FakeDataFactory.CreateFakeTutorAvailability(1, status: (int)TutorAvailabilityStatus.Available);
			var validAvailabilityDto = new TutorAvailabilityDto
			{
				Id = 1,
				TutorId = 1,
				SlotId = 1,
				Status = TutorAvailabilityStatus.Available,
				StartDate = validAvailabilityEntity.StartDate,
				EndDate = validAvailabilityEntity.EndDate,
				CreatedAt = validAvailabilityEntity.CreatedAt,
				Slot = new TimeSlotDto
				{
					Id = 1,
					StartTime = validAvailabilityEntity.Slot!.StartTime,
					EndTime = validAvailabilityEntity.Slot.EndTime
				}
			};
			var validBookingDto = new BookingDto
			{
				Id = 1,
				LearnerEmail = "learner@example.com",
				TotalSessions = 10,
				SystemFee = new SystemFeeDto { Id = 1 }
			};

			yield return new TestCaseData(
				new ScheduleCreateRequest
				{
					AvailabilitiId = 1,
					BookingId = 1,
					IsOnline = false
				},
				validAvailabilityDto,
				validBookingDto,
				null, // tutorProfile
				0, // currentCount
				null, // existingSchedule
				true,
				null
			).SetName("CreateAsync_ValidRequest_ReturnsCreatedDto");

			// TutorAvailability không tồn tại
			yield return new TestCaseData(
				new ScheduleCreateRequest
				{
					AvailabilitiId = 999,
					BookingId = 1
				},
				null, // availability
				null,
				null,
				0,
				null,
				false,
				typeof(Exception)
			).SetName("CreateAsync_WhenTutorAvailabilityNotExists_ThrowsException");
		}

		/// <summary>
		/// Test CreateAsync với các scenarios khác nhau
		/// </summary>
		[Test]
		[TestCaseSource(nameof(CreateAsyncTestCases))]
		public async Task CreateAsync_WithVariousScenarios_HandlesCorrectly(
			ScheduleCreateRequest request,
			TutorAvailabilityDto? availability,
			BookingDto? bookingDto,
			TutorProfileDto? tutorProfile,
			int currentCount,
			EduMatch.DataAccessLayer.Entities.Schedule? existingSchedule,
			bool shouldSucceed,
			Type? expectedExceptionType)
		{
			// Arrange
			_tutorAvailabilityServiceMock
				.Setup(s => s.GetByIdFullAsync(request.AvailabilitiId))
				.ReturnsAsync(availability);

			if (bookingDto != null)
			{
				_bookingServiceMock
					.Setup(s => s.GetByIdAsync(request.BookingId))
					.ReturnsAsync(bookingDto);

				if (!string.IsNullOrWhiteSpace(bookingDto.LearnerEmail))
				{
					_tutorProfileServiceMock
						.Setup(s => s.GetByEmailFullAsync(bookingDto.LearnerEmail))
						.ReturnsAsync(tutorProfile);
				}

				_scheduleRepositoryMock
					.Setup(r => r.CountByBookingIdAndStatusAsync(request.BookingId, null))
					.ReturnsAsync(currentCount);
			}

			_scheduleRepositoryMock
				.Setup(r => r.GetByAvailabilityIdAsync(request.AvailabilitiId))
				.ReturnsAsync(existingSchedule);

			if (shouldSucceed)
			{
				_scheduleRepositoryMock
					.Setup(r => r.CreateAsync(It.IsAny<EduMatch.DataAccessLayer.Entities.Schedule>()))
					.Returns(Task.CompletedTask);

				_tutorAvailabilityServiceMock
					.Setup(s => s.UpdateStatusAsync(request.AvailabilitiId, TutorAvailabilityStatus.Booked))
					.ReturnsAsync(new TutorAvailabilityDto { Id = request.AvailabilitiId, Status = TutorAvailabilityStatus.Booked });
			}

			// Act & Assert
			if (shouldSucceed)
			{
				var result = await _service.CreateAsync(request);
				Assert.That(result, Is.Not.Null);
				Assert.That(result.BookingId, Is.EqualTo(request.BookingId));
			}
			else
			{
				var exception = Assert.ThrowsAsync<Exception>(async () => await _service.CreateAsync(request));
				Assert.That(exception, Is.Not.Null);
			}
		}

		/// <summary>
		/// Test data cho UpdateAsync
		/// </summary>
		private static IEnumerable<TestCaseData> UpdateAsyncTestCases()
		{
			// Valid case
			var existingEntity = FakeDataFactory.CreateFakeSchedule(1);
			var validBookingDto = new BookingDto
			{
				Id = 1,
				LearnerEmail = "learner@example.com",
				SystemFee = new SystemFeeDto { Id = 1 }
			};

			yield return new TestCaseData(
				new ScheduleUpdateRequest
				{
					Id = 1,
					Status = ScheduleStatus.Completed
				},
				existingEntity,
				null, // newAvailability
				validBookingDto,
				null, // existingSchedule
				true,
				null
			).SetName("UpdateAsync_ValidRequest_ReturnsUpdatedDto");

			// Schedule không tồn tại
			yield return new TestCaseData(
				new ScheduleUpdateRequest
				{
					Id = 999,
					Status = ScheduleStatus.Completed
				},
				null, // existingEntity
				null,
				null,
				null,
				false,
				typeof(Exception)
			).SetName("UpdateAsync_WhenScheduleNotExists_ThrowsException");
		}

		/// <summary>
		/// Test UpdateAsync với các scenarios khác nhau
		/// </summary>
		[Test]
		[TestCaseSource(nameof(UpdateAsyncTestCases))]
		public async Task UpdateAsync_WithVariousScenarios_HandlesCorrectly(
			ScheduleUpdateRequest request,
			EduMatch.DataAccessLayer.Entities.Schedule? existingEntity,
			TutorAvailabilityDto? newAvailability,
			BookingDto? bookingDto,
			EduMatch.DataAccessLayer.Entities.Schedule? existingSchedule,
			bool shouldSucceed,
			Type? expectedExceptionType)
		{
			// Arrange
			_scheduleRepositoryMock
				.Setup(r => r.GetByIdAsync(request.Id))
				.ReturnsAsync(existingEntity);

			if (existingEntity != null)
			{
				if (request.AvailabilitiId.HasValue)
				{
					_tutorAvailabilityServiceMock
						.Setup(s => s.GetByIdFullAsync(request.AvailabilitiId.Value))
						.ReturnsAsync(newAvailability);

					_scheduleRepositoryMock
						.Setup(r => r.GetByAvailabilityIdAsync(request.AvailabilitiId.Value))
						.ReturnsAsync(existingSchedule);
				}

				if (request.BookingId.HasValue && bookingDto != null)
				{
					_bookingServiceMock
						.Setup(s => s.GetByIdAsync(request.BookingId.Value))
						.ReturnsAsync(bookingDto);
				}

				_meetingSessionServiceMock
					.Setup(s => s.GetByScheduleIdAsync(existingEntity.Id))
					.ReturnsAsync((MeetingSessionDto?)null);

				_scheduleRepositoryMock
					.Setup(r => r.UpdateAsync(It.IsAny<EduMatch.DataAccessLayer.Entities.Schedule>()))
					.Returns(Task.CompletedTask);
			}

			// Act & Assert
			if (shouldSucceed)
			{
				var result = await _service.UpdateAsync(request);
				Assert.That(result, Is.Not.Null);
				Assert.That(result.Id, Is.EqualTo(request.Id));
			}
			else
			{
				var exception = Assert.ThrowsAsync<Exception>(async () => await _service.UpdateAsync(request));
				Assert.That(exception, Is.Not.Null);
			}
		}

		/// <summary>
		/// Test DeleteAsync với ID khác nhau
		/// </summary>
		[Test]
		[TestCase(1, true)]
		[TestCase(999, false)]
		public async Task DeleteAsync_WithDifferentIds_HandlesCorrectly(int id, bool shouldExist)
		{
			// Arrange
			EduMatch.DataAccessLayer.Entities.Schedule? entity = shouldExist ? FakeDataFactory.CreateFakeSchedule(id) : null;

			_scheduleRepositoryMock
				.Setup(r => r.GetByIdAsync(id))
				.ReturnsAsync(entity);

			_meetingSessionServiceMock
				.Setup(s => s.GetByScheduleIdAsync(id))
				.ReturnsAsync((MeetingSessionDto?)null);

			if (shouldExist)
			{
				_scheduleRepositoryMock
					.Setup(r => r.DeleteAsync(id))
					.Returns(Task.CompletedTask);
			}

			// Act
			await _service.DeleteAsync(id);

			// Assert
			if (shouldExist)
			{
				_scheduleRepositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
			}
		}

		/// <summary>
		/// Test DeleteAsync với invalid ID
		/// </summary>
		[Test]
		[TestCase(0)]
		[TestCase(-1)]
		public void DeleteAsync_WithInvalidId_ThrowsArgumentException(int id)
		{
			// Act & Assert
			var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.DeleteAsync(id));
			Assert.That(exception.Message, Does.Contain("ID must be greater than 0"));
		}
	}
}

