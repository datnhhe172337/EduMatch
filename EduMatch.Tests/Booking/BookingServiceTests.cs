using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.Booking;
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
using BookingEntity = EduMatch.DataAccessLayer.Entities.Booking;

namespace EduMatch.Tests.Booking
{
	/// <summary>
	/// Test class cho BookingService
	/// </summary>
	public class BookingServiceTests
	{
		private Mock<IBookingRepository> _bookingRepositoryMock;
		private Mock<ITutorSubjectRepository> _tutorSubjectRepositoryMock;
		private Mock<ISystemFeeRepository> _systemFeeRepositoryMock;
		private Mock<IUserRepository> _userRepositoryMock;
		private Mock<IScheduleRepository> _scheduleRepositoryMock;
		private Mock<IMeetingSessionRepository> _meetingSessionRepositoryMock;
		private Mock<ITutorAvailabilityRepository> _tutorAvailabilityRepositoryMock;
		private Mock<IGoogleCalendarService> _googleCalendarServiceMock;
		private IMapper _mapper;
		private BookingService _service;

		/// <summary>
		/// Khởi tạo các mock objects và service trước mỗi test
		/// </summary>
		[SetUp]
		public void Setup()
		{
			_bookingRepositoryMock = new Mock<IBookingRepository>();
			_tutorSubjectRepositoryMock = new Mock<ITutorSubjectRepository>();
			_systemFeeRepositoryMock = new Mock<ISystemFeeRepository>();
			_userRepositoryMock = new Mock<IUserRepository>();
			_scheduleRepositoryMock = new Mock<IScheduleRepository>();
			_meetingSessionRepositoryMock = new Mock<IMeetingSessionRepository>();
			_tutorAvailabilityRepositoryMock = new Mock<ITutorAvailabilityRepository>();
			_googleCalendarServiceMock = new Mock<IGoogleCalendarService>();

			var config = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = config.CreateMapper();

			_service = new BookingService(
				_bookingRepositoryMock.Object,
				_tutorSubjectRepositoryMock.Object,
				_systemFeeRepositoryMock.Object,
				_userRepositoryMock.Object,
				_mapper,
				_scheduleRepositoryMock.Object,
				_meetingSessionRepositoryMock.Object,
				_tutorAvailabilityRepositoryMock.Object,
				_googleCalendarServiceMock.Object
			);
		}

		/// <summary>
		/// Test GetAllByLearnerEmailNoPagingAsync với email khác nhau
		/// </summary>
		[Test]
		[TestCase("learner1@example.com", 2)]
		[TestCase("learner2@example.com", 0)]
		public async Task GetAllByLearnerEmailNoPagingAsync_WithDifferentEmails_ReturnsExpectedList(string email, int expectedCount)
		{
			// Arrange
			var bookings = expectedCount > 0
				? new List<BookingEntity>
				{
					FakeDataFactory.CreateFakeBooking(1, email),
					FakeDataFactory.CreateFakeBooking(2, email)
				}
				: new List<BookingEntity>();

			_bookingRepositoryMock
				.Setup(r => r.GetAllByLearnerEmailNoPagingAsync(email, null, null))
				.ReturnsAsync(bookings);

			// Act
			var result = await _service.GetAllByLearnerEmailNoPagingAsync(email, null, null);

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Count, Is.EqualTo(expectedCount));
		}

		/// <summary>
		/// Test GetAllByLearnerEmailNoPagingAsync với email rỗng - throw Exception
		/// </summary>
		[Test]
		[TestCase("")]
		[TestCase("   ")]
		[TestCase(null)]
		public void GetAllByLearnerEmailNoPagingAsync_WithInvalidEmail_ThrowsException(string? email)
		{
			// Act & Assert
			Assert.ThrowsAsync<Exception>(async () => await _service.GetAllByLearnerEmailNoPagingAsync(email!, null, null));
		}

		/// <summary>
		/// Test GetAllByTutorIdNoPagingAsync với tutorId khác nhau
		/// </summary>
		[Test]
		[TestCase(1, 2)]
		[TestCase(2, 0)]
		public async Task GetAllByTutorIdNoPagingAsync_WithDifferentTutorIds_ReturnsExpectedList(int tutorId, int expectedCount)
		{
			// Arrange
			var bookings = expectedCount > 0
				? new List<BookingEntity>
				{
					FakeDataFactory.CreateFakeBooking(1, tutorSubjectId: tutorId),
					FakeDataFactory.CreateFakeBooking(2, tutorSubjectId: tutorId)
				}
				: new List<BookingEntity>();

			_bookingRepositoryMock
				.Setup(r => r.GetAllByTutorIdNoPagingAsync(tutorId, null, null))
				.ReturnsAsync(bookings);

			// Act
			var result = await _service.GetAllByTutorIdNoPagingAsync(tutorId, null, null);

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Count, Is.EqualTo(expectedCount));
		}

		/// <summary>
		/// Test GetAllByTutorIdNoPagingAsync với tutorId <= 0 - throw Exception
		/// </summary>
		[Test]
		[TestCase(0)]
		[TestCase(-1)]
		public void GetAllByTutorIdNoPagingAsync_WithInvalidTutorId_ThrowsException(int tutorId)
		{
			// Act & Assert
			Assert.ThrowsAsync<Exception>(async () => await _service.GetAllByTutorIdNoPagingAsync(tutorId, null, null));
		}

		/// <summary>
		/// Test GetByIdAsync với các ID khác nhau - trả về DTO khi tồn tại, null khi không tồn tại
		/// </summary>
		[Test]
		[TestCase(1, true)]
		[TestCase(999, false)]
		public async Task GetByIdAsync_WithDifferentIds_ReturnsExpectedResult(int id, bool shouldExist)
		{
			// Arrange
			BookingEntity? entity = shouldExist ? FakeDataFactory.CreateFakeBooking(id) : null;

			_bookingRepositoryMock
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
		/// Test GetByIdAsync với ID <= 0 - throw Exception
		/// </summary>
		[Test]
		[TestCase(0)]
		[TestCase(-1)]
		public void GetByIdAsync_WithInvalidId_ThrowsException(int id)
		{
			// Act & Assert
			Assert.ThrowsAsync<Exception>(async () => await _service.GetByIdAsync(id));
		}

		/// <summary>
		/// Test CreateAsync với các scenarios khác nhau
		/// </summary>
		[Test]
		[TestCase(true, true, true, true, true)] // Valid
		[TestCase(false, true, true, true, false)] // LearnerEmail not found
		[TestCase(true, false, true, true, false)] // TutorSubject not found
		[TestCase(true, true, false, true, false)] // TutorSubject no hourly rate
		[TestCase(true, true, true, false, false)] // SystemFee not found
		public async Task CreateAsync_WithVariousScenarios_HandlesCorrectly(
			bool learnerExists,
			bool tutorSubjectExists,
			bool hasHourlyRate,
			bool systemFeeExists,
			bool shouldSucceed)
		{
			// Arrange
			var request = new BookingCreateRequest
			{
				LearnerEmail = "learner@example.com",
				TutorSubjectId = 1,
				TotalSessions = 1
			};

			if (learnerExists)
			{
				_userRepositoryMock
					.Setup(r => r.GetUserByEmailAsync(request.LearnerEmail))
					.ReturnsAsync(FakeDataFactory.CreateFakeUser(request.LearnerEmail));
			}
			else
			{
				_userRepositoryMock
					.Setup(r => r.GetUserByEmailAsync(request.LearnerEmail))
					.ReturnsAsync((User?)null);
			}

			if (tutorSubjectExists)
			{
				var tutorSubject = FakeDataFactory.CreateFakeTutorSubject(
					id: request.TutorSubjectId,
					hourlyRate: hasHourlyRate ? 200000 : null);
				_tutorSubjectRepositoryMock
					.Setup(r => r.GetByIdFullAsync(request.TutorSubjectId))
					.ReturnsAsync(tutorSubject);
			}
			else
			{
				_tutorSubjectRepositoryMock
					.Setup(r => r.GetByIdFullAsync(request.TutorSubjectId))
					.ReturnsAsync((TutorSubject?)null);
			}

			if (systemFeeExists)
			{
				_systemFeeRepositoryMock
					.Setup(r => r.GetActiveSystemFeeAsync())
					.ReturnsAsync(FakeDataFactory.CreateFakeSystemFee());
			}
			else
			{
				_systemFeeRepositoryMock
					.Setup(r => r.GetActiveSystemFeeAsync())
					.ReturnsAsync((SystemFee?)null);
			}

			if (shouldSucceed)
			{
			_bookingRepositoryMock
				.Setup(r => r.CreateAsync(It.IsAny<BookingEntity>()))
				.Returns(Task.CompletedTask);

				// Act
				var result = await _service.CreateAsync(request);

				// Assert
				Assert.That(result, Is.Not.Null);
				Assert.That(result.LearnerEmail, Is.EqualTo(request.LearnerEmail));
				Assert.That(result.TutorSubjectId, Is.EqualTo(request.TutorSubjectId));
			}
			else
			{
				// Act & Assert
				Assert.ThrowsAsync<Exception>(async () => await _service.CreateAsync(request));
			}
		}

		/// <summary>
		/// Test UpdateAsync với các scenarios khác nhau
		/// </summary>
		[Test]
		[TestCase(true, true, true, true, true)] // Valid
		[TestCase(false, true, true, true, false)] // Booking not found
		[TestCase(true, false, true, true, false)] // LearnerEmail not found (when updating)
		[TestCase(true, true, false, true, false)] // TutorSubject not found (when updating)
		[TestCase(true, true, true, false, false)] // SystemFee not found (when recalculating)
		public async Task UpdateAsync_WithVariousScenarios_HandlesCorrectly(
			bool bookingExists,
			bool learnerExists,
			bool tutorSubjectExists,
			bool systemFeeExists,
			bool shouldSucceed)
		{
			// Arrange
			var request = new BookingUpdateRequest
			{
				Id = 1,
				LearnerEmail = "newlearner@example.com",
				TutorSubjectId = 2,
				TotalSessions = 2
			};

			if (bookingExists)
			{
				var existingBooking = FakeDataFactory.CreateFakeBooking(request.Id);
				_bookingRepositoryMock
					.Setup(r => r.GetByIdAsync(request.Id))
					.ReturnsAsync(existingBooking);

				if (learnerExists)
				{
					_userRepositoryMock
						.Setup(r => r.GetUserByEmailAsync(request.LearnerEmail!))
						.ReturnsAsync(FakeDataFactory.CreateFakeUser(request.LearnerEmail!));
				}
				else
				{
					_userRepositoryMock
						.Setup(r => r.GetUserByEmailAsync(request.LearnerEmail!))
						.ReturnsAsync((User?)null);
				}

				if (tutorSubjectExists)
				{
					_tutorSubjectRepositoryMock
						.Setup(r => r.GetByIdFullAsync(request.TutorSubjectId!.Value))
						.ReturnsAsync(FakeDataFactory.CreateFakeTutorSubject(request.TutorSubjectId.Value));
				}
				else
				{
					_tutorSubjectRepositoryMock
						.Setup(r => r.GetByIdFullAsync(request.TutorSubjectId!.Value))
						.ReturnsAsync((TutorSubject?)null);
				}

				if (systemFeeExists)
				{
					_systemFeeRepositoryMock
						.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
						.ReturnsAsync(FakeDataFactory.CreateFakeSystemFee());
				}
				else
				{
					_systemFeeRepositoryMock
						.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
						.ReturnsAsync((SystemFee?)null);
				}

				_bookingRepositoryMock
					.Setup(r => r.UpdateAsync(It.IsAny<BookingEntity>()))
					.Returns(Task.CompletedTask);
			}
			else
			{
				_bookingRepositoryMock
					.Setup(r => r.GetByIdAsync(request.Id))
					.ReturnsAsync((BookingEntity?)null);
			}

			if (shouldSucceed)
			{
				// Act
				var result = await _service.UpdateAsync(request);

				// Assert
				Assert.That(result, Is.Not.Null);
				Assert.That(result.Id, Is.EqualTo(request.Id));
			}
			else
			{
				// Act & Assert
				Assert.ThrowsAsync<Exception>(async () => await _service.UpdateAsync(request));
			}
		}

		/// <summary>
		/// Test DeleteAsync với ID hợp lệ - thành công
		/// </summary>
		[Test]
		[TestCase(1)]
		[TestCase(2)]
		public async Task DeleteAsync_WithValidId_CallsRepository(int id)
		{
			// Arrange
			_bookingRepositoryMock
				.Setup(r => r.DeleteAsync(id))
				.Returns(Task.CompletedTask);

			// Act
			await _service.DeleteAsync(id);

			// Assert
			_bookingRepositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
		}

		/// <summary>
		/// Test UpdatePaymentStatusAsync với các scenarios khác nhau
		/// </summary>
		[Test]
		[TestCase(true, true)] // Valid
		[TestCase(false, false)] // Booking not found
		public async Task UpdatePaymentStatusAsync_WithVariousScenarios_HandlesCorrectly(
			bool bookingExists,
			bool shouldSucceed)
		{
			// Arrange
			var id = 1;
			var paymentStatus = PaymentStatus.Paid;

			if (bookingExists)
			{
				var booking = FakeDataFactory.CreateFakeBooking(id);
				_bookingRepositoryMock
					.Setup(r => r.GetByIdAsync(id))
					.ReturnsAsync(booking);
				_bookingRepositoryMock
					.Setup(r => r.UpdateAsync(It.IsAny<BookingEntity>()))
					.Returns(Task.CompletedTask);
			}
			else
			{
				_bookingRepositoryMock
					.Setup(r => r.GetByIdAsync(id))
					.ReturnsAsync((BookingEntity?)null);
			}

			if (shouldSucceed)
			{
				// Act
				var result = await _service.UpdatePaymentStatusAsync(id, paymentStatus);

				// Assert
				Assert.That(result, Is.Not.Null);
				Assert.That(result.Id, Is.EqualTo(id));
			}
			else
			{
				// Act & Assert
				Assert.ThrowsAsync<Exception>(async () => await _service.UpdatePaymentStatusAsync(id, paymentStatus));
			}
		}

		/// <summary>
		/// Test UpdateStatusAsync với các scenarios khác nhau
		/// </summary>
		[Test]
		[TestCase(true, true)] // Valid
		[TestCase(false, false)] // Booking not found
		public async Task UpdateStatusAsync_WithVariousScenarios_HandlesCorrectly(
			bool bookingExists,
			bool shouldSucceed)
		{
			// Arrange
			var id = 1;
			var status = BookingStatus.Confirmed;

			if (bookingExists)
			{
				var booking = FakeDataFactory.CreateFakeBooking(id);
				_bookingRepositoryMock
					.Setup(r => r.GetByIdAsync(id))
					.ReturnsAsync(booking);
				_bookingRepositoryMock
					.Setup(r => r.UpdateAsync(It.IsAny<BookingEntity>()))
					.Returns(Task.CompletedTask);
				_scheduleRepositoryMock
					.Setup(r => r.GetAllByBookingIdOrderedAsync(id))
					.ReturnsAsync(new List<Schedule>());
			}
			else
			{
				_bookingRepositoryMock
					.Setup(r => r.GetByIdAsync(id))
					.ReturnsAsync((BookingEntity?)null);
			}

			if (shouldSucceed)
			{
				// Act
				var result = await _service.UpdateStatusAsync(id, status);

				// Assert
				Assert.That(result, Is.Not.Null);
				Assert.That(result.Id, Is.EqualTo(id));
			}
			else
			{
				// Act & Assert
				Assert.ThrowsAsync<Exception>(async () => await _service.UpdateStatusAsync(id, status));
			}
		}
	}
}

