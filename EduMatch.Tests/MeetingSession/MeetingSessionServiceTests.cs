using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.MeetingSession;
using EduMatch.BusinessLogicLayer.Responses.GoogleCalendar;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.Tests.Common;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.Tests
{
	/// <summary>
	/// Test class cho MeetingSessionService
	/// </summary>
	public class MeetingSessionServiceTests
	{
		private Mock<IMeetingSessionRepository> _meetingSessionRepositoryMock;
		private Mock<IScheduleRepository> _scheduleRepositoryMock;
		private Mock<IGoogleTokenRepository> _googleTokenRepositoryMock;
		private Mock<IGoogleCalendarService> _googleCalendarServiceMock;
		private IOptions<GoogleCalendarSettings> _googleCalendarSettings;
		private IMapper _mapper;
		private MeetingSessionService _service;

		/// <summary>
		/// Khởi tạo các mock objects và service trước mỗi test
		/// </summary>
		[SetUp]
		public void Setup()
		{
			_meetingSessionRepositoryMock = new Mock<IMeetingSessionRepository>();
			_scheduleRepositoryMock = new Mock<IScheduleRepository>();
			_googleTokenRepositoryMock = new Mock<IGoogleTokenRepository>();
			_googleCalendarServiceMock = new Mock<IGoogleCalendarService>();

			var settings = new GoogleCalendarSettings
			{
				SystemAccountEmail = "system@edumatch.com"
			};
			_googleCalendarSettings = Options.Create(settings);

			var config = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = config.CreateMapper();

			_service = new MeetingSessionService(
				_meetingSessionRepositoryMock.Object,
				_scheduleRepositoryMock.Object,
				_googleTokenRepositoryMock.Object,
				_googleCalendarServiceMock.Object,
				_googleCalendarSettings,
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
			MeetingSession? entity = shouldExist ? FakeDataFactory.CreateFakeMeetingSession(id) : null;

			_meetingSessionRepositoryMock
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
		/// Test GetByScheduleIdAsync với ScheduleId khác nhau
		/// </summary>
		[Test]
		[TestCase(1, true)]
		[TestCase(999, false)]
		public async Task GetByScheduleIdAsync_WithDifferentScheduleIds_ReturnsExpectedResult(int scheduleId, bool shouldExist)
		{
			// Arrange
			MeetingSession? entity = shouldExist ? FakeDataFactory.CreateFakeMeetingSession(1, scheduleId) : null;

			_meetingSessionRepositoryMock
				.Setup(r => r.GetByScheduleIdAsync(scheduleId))
				.ReturnsAsync(entity);

			// Act
			var result = await _service.GetByScheduleIdAsync(scheduleId);

			// Assert
			if (shouldExist)
			{
				Assert.That(result, Is.Not.Null);
				Assert.That(result!.ScheduleId, Is.EqualTo(scheduleId));
			}
			else
			{
				Assert.That(result, Is.Null);
			}
		}

		/// <summary>
		/// Test GetByScheduleIdAsync với invalid ScheduleId
		/// </summary>
		[Test]
		[TestCase(0)]
		[TestCase(-1)]
		public void GetByScheduleIdAsync_WithInvalidScheduleId_ThrowsArgumentException(int scheduleId)
		{
			// Act & Assert
			var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetByScheduleIdAsync(scheduleId));
			Assert.That(exception.Message, Does.Contain("ScheduleId must be greater than 0"));
		}

		/// <summary>
		/// Test data cho CreateAsync
		/// </summary>
		private static IEnumerable<TestCaseData> CreateAsyncTestCases()
		{
			// Valid case
			var validSchedule = FakeDataFactory.CreateFakeSchedule(1, 1, 1);
			var validGoogleToken = FakeDataFactory.CreateFakeGoogleToken("system@edumatch.com");
			var validGoogleResponse = new GoogleEventCreatedResponse
			{
				EventId = "event123",
				HangoutLink = "https://meet.google.com/abc-defg-hij",
				ConferenceData = new ConferenceData
				{
					ConferenceId = "abc-defg-hij"
				},
				Start = new GoogleEventDateTimeResponse
				{
					DateTime = DateTime.UtcNow.AddDays(1).ToString("o")
				},
				End = new GoogleEventDateTimeResponse
				{
					DateTime = DateTime.UtcNow.AddDays(1).AddHours(2).ToString("o")
				}
			};

			yield return new TestCaseData(
				new MeetingSessionCreateRequest { ScheduleId = 1 },
				validSchedule,
				null, // existingMeetingSession
				validGoogleToken,
				validGoogleResponse,
				true,
				null
			).SetName("CreateAsync_ValidRequest_ReturnsCreatedDto");

			// Schedule không tồn tại
			yield return new TestCaseData(
				new MeetingSessionCreateRequest { ScheduleId = 999 },
				null, // schedule
				null,
				null,
				null,
				false,
				typeof(Exception)
			).SetName("CreateAsync_WhenScheduleNotExists_ThrowsException");
		}

		/// <summary>
		/// Test CreateAsync với các scenarios khác nhau
		/// </summary>
		[Test]
		[TestCaseSource(nameof(CreateAsyncTestCases))]
		public async Task CreateAsync_WithVariousScenarios_HandlesCorrectly(
			MeetingSessionCreateRequest request,
			Schedule? schedule,
			MeetingSession? existingMeetingSession,
			GoogleToken? googleToken,
			GoogleEventCreatedResponse? googleEventResponse,
			bool shouldSucceed,
			Type? expectedExceptionType)
		{
			// Arrange
			_scheduleRepositoryMock
				.Setup(r => r.GetByIdAsync(request.ScheduleId))
				.ReturnsAsync(schedule);

			if (schedule != null)
			{
				_meetingSessionRepositoryMock
					.Setup(r => r.GetByScheduleIdAsync(request.ScheduleId))
					.ReturnsAsync(existingMeetingSession);

				if (existingMeetingSession == null)
				{
					_googleTokenRepositoryMock
						.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
						.ReturnsAsync(googleToken);

					if (googleToken != null)
					{
						_scheduleRepositoryMock
							.Setup(r => r.GetAllByBookingIdOrderedAsync(It.IsAny<int>()))
							.ReturnsAsync(new List<Schedule> { schedule });

						_googleCalendarServiceMock
							.Setup(s => s.CreateEventAsync(It.IsAny<EduMatch.BusinessLogicLayer.Requests.GoogleMeeting.CreateMeetingRequest>()))
							.ReturnsAsync(googleEventResponse);

						if (googleEventResponse != null)
						{
						_meetingSessionRepositoryMock
							.Setup(r => r.CreateAsync(It.IsAny<MeetingSession>()))
							.Returns(Task.CompletedTask);
						}
					}
				}
			}

			// Act & Assert
			if (shouldSucceed)
			{
				var result = await _service.CreateAsync(request);
				Assert.That(result, Is.Not.Null);
				Assert.That(result.ScheduleId, Is.EqualTo(request.ScheduleId));
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
			var existingEntity = FakeDataFactory.CreateFakeMeetingSession(1, 1);
			var validSchedule = FakeDataFactory.CreateFakeSchedule(1, 1, 1);
			var validGoogleResponse = new GoogleEventCreatedResponse
			{
				EventId = "event123",
				HangoutLink = "https://meet.google.com/abc-defg-hij",
				Start = new GoogleEventDateTimeResponse
				{
					DateTime = DateTime.UtcNow.AddDays(1).ToString("o")
				},
				End = new GoogleEventDateTimeResponse
				{
					DateTime = DateTime.UtcNow.AddDays(1).AddHours(2).ToString("o")
				}
			};

			yield return new TestCaseData(
				new MeetingSessionUpdateRequest
				{
					Id = 1,
					ScheduleId = 1,
					MeetingType = MeetingType.Main
				},
				existingEntity,
				validSchedule,
				validGoogleResponse,
				true,
				null
			).SetName("UpdateAsync_ValidRequest_ReturnsUpdatedDto");

			// MeetingSession không tồn tại
			yield return new TestCaseData(
				new MeetingSessionUpdateRequest
				{
					Id = 999,
					ScheduleId = 1
				},
				null, // existingEntity
				null,
				null,
				false,
				typeof(Exception)
			).SetName("UpdateAsync_WhenMeetingSessionNotExists_ThrowsException");
		}

		/// <summary>
		/// Test UpdateAsync với các scenarios khác nhau
		/// </summary>
		[Test]
		[TestCaseSource(nameof(UpdateAsyncTestCases))]
		public async Task UpdateAsync_WithVariousScenarios_HandlesCorrectly(
			MeetingSessionUpdateRequest request,
			MeetingSession? existingEntity,
			Schedule? schedule,
			GoogleEventCreatedResponse? googleEventResponse,
			bool shouldSucceed,
			Type? expectedExceptionType)
		{
			// Arrange
			_meetingSessionRepositoryMock
				.Setup(r => r.GetByIdAsync(request.Id))
				.ReturnsAsync(existingEntity);

			if (existingEntity != null)
			{
				if (request.ScheduleId.HasValue)
				{
					_scheduleRepositoryMock
						.Setup(r => r.GetByIdAsync(existingEntity.ScheduleId))
						.ReturnsAsync(schedule);
				}

				if (schedule != null && !string.IsNullOrEmpty(existingEntity.EventId))
				{
					_scheduleRepositoryMock
						.Setup(r => r.GetAllByBookingIdOrderedAsync(It.IsAny<int>()))
						.ReturnsAsync(new List<Schedule> { schedule });

					_googleCalendarServiceMock
						.Setup(s => s.UpdateEventAsync(It.IsAny<string>(), It.IsAny<EduMatch.BusinessLogicLayer.Requests.GoogleMeeting.CreateMeetingRequest>()))
						.ReturnsAsync(googleEventResponse);
				}

				_meetingSessionRepositoryMock
					.Setup(r => r.UpdateAsync(It.IsAny<MeetingSession>()))
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
			MeetingSession? entity = shouldExist ? FakeDataFactory.CreateFakeMeetingSession(id) : null;

			_meetingSessionRepositoryMock
				.Setup(r => r.GetByIdAsync(id))
				.ReturnsAsync(entity);

			if (shouldExist)
			{
				_googleCalendarServiceMock
					.Setup(s => s.DeleteEventAsync(It.IsAny<string>()))
					.ReturnsAsync(true);

				_meetingSessionRepositoryMock
					.Setup(r => r.DeleteAsync(id))
					.Returns(Task.CompletedTask);
			}

			// Act
			await _service.DeleteAsync(id);

			// Assert
			if (shouldExist)
			{
				_googleCalendarServiceMock.Verify(s => s.DeleteEventAsync(It.IsAny<string>()), Times.Once);
				_meetingSessionRepositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
			}
			else
			{
				_googleCalendarServiceMock.Verify(s => s.DeleteEventAsync(It.IsAny<string>()), Times.Never);
				_meetingSessionRepositoryMock.Verify(r => r.DeleteAsync(id), Times.Never);
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

