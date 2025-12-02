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

namespace EduMatch.Tests
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
		private Mock<IUnitOfWork> _unitOfWorkMock;
		private Mock<IWalletRepository> _walletRepositoryMock;
		private Mock<IWalletTransactionRepository> _walletTransactionRepositoryMock;
		private Mock<INotificationService> _notificationServiceMock;
		private Mock<ILearnerTrialLessonService> _learnerTrialLessonServiceMock;
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
			_unitOfWorkMock = new Mock<IUnitOfWork>();
			_walletRepositoryMock = new Mock<IWalletRepository>();
			_walletTransactionRepositoryMock = new Mock<IWalletTransactionRepository>();
			_notificationServiceMock = new Mock<INotificationService>();
			_learnerTrialLessonServiceMock = new Mock<ILearnerTrialLessonService>();

			_unitOfWorkMock.SetupGet(u => u.Wallets).Returns(_walletRepositoryMock.Object);
			_unitOfWorkMock.SetupGet(u => u.WalletTransactions).Returns(_walletTransactionRepositoryMock.Object);
			_unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

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
				_googleCalendarServiceMock.Object,
				_unitOfWorkMock.Object,
				_notificationServiceMock.Object,
				_learnerTrialLessonServiceMock.Object
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
		/// Tạo các test cases cho CreateAsync - bao phủ các kịch bản: valid, learner not found, tutorSubject not found, no hourly rate, systemFee not found
		/// </summary>
		private static IEnumerable<TestCaseData> CreateAsyncTestCases()
		{
			// Valid case
			yield return new TestCaseData(
				new BookingCreateRequest
				{
					LearnerEmail = "learner@example.com",
					TutorSubjectId = 1,
					TotalSessions = 1
				},
				FakeDataFactory.CreateFakeUser("learner@example.com"),
				FakeDataFactory.CreateFakeTutorSubject(1, hourlyRate: 200000),
				FakeDataFactory.CreateFakeSystemFee(),
				true
			).SetName("CreateAsync_ValidRequest_ReturnsCreatedDto");

			// LearnerEmail not found
			yield return new TestCaseData(
				new BookingCreateRequest
				{
					LearnerEmail = "learner@example.com",
					TutorSubjectId = 1,
					TotalSessions = 1
				},
				null,
				FakeDataFactory.CreateFakeTutorSubject(1, hourlyRate: 200000),
				FakeDataFactory.CreateFakeSystemFee(),
				false
			).SetName("CreateAsync_LearnerEmailNotFound_ThrowsException");

			// TutorSubject not found
			yield return new TestCaseData(
				new BookingCreateRequest
				{
					LearnerEmail = "learner@example.com",
					TutorSubjectId = 999,
					TotalSessions = 1
				},
				FakeDataFactory.CreateFakeUser("learner@example.com"),
				null,
				FakeDataFactory.CreateFakeSystemFee(),
				false
			).SetName("CreateAsync_TutorSubjectNotFound_ThrowsException");

			// TutorSubject no hourly rate
			yield return new TestCaseData(
				new BookingCreateRequest
				{
					LearnerEmail = "learner@example.com",
					TutorSubjectId = 1,
					TotalSessions = 1
				},
				FakeDataFactory.CreateFakeUser("learner@example.com"),
				FakeDataFactory.CreateFakeTutorSubject(1, hourlyRate: null),
				FakeDataFactory.CreateFakeSystemFee(),
				false
			).SetName("CreateAsync_TutorSubjectNoHourlyRate_ThrowsException");

			// SystemFee not found
			yield return new TestCaseData(
				new BookingCreateRequest
				{
					LearnerEmail = "learner@example.com",
					TutorSubjectId = 1,
					TotalSessions = 1
				},
				FakeDataFactory.CreateFakeUser("learner@example.com"),
				FakeDataFactory.CreateFakeTutorSubject(1, hourlyRate: 200000),
				null,
				false
			).SetName("CreateAsync_SystemFeeNotFound_ThrowsException");
		}

		/// <summary>
		/// Test CreateAsync với các scenarios khác nhau
		/// </summary>
		[Test]
		[TestCaseSource(nameof(CreateAsyncTestCases))]
		public async Task CreateAsync_WithVariousScenarios_HandlesCorrectly(
			BookingCreateRequest request,
			User? learner,
			TutorSubject? tutorSubject,
			SystemFee? systemFee,
			bool shouldSucceed)
		{
			// Arrange
			_userRepositoryMock
				.Setup(r => r.GetUserByEmailAsync(request.LearnerEmail))
				.ReturnsAsync(learner);

			_tutorSubjectRepositoryMock
				.Setup(r => r.GetByIdFullAsync(request.TutorSubjectId))
				.ReturnsAsync(tutorSubject);

			_systemFeeRepositoryMock
				.Setup(r => r.GetActiveSystemFeeAsync())
				.ReturnsAsync(systemFee);

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
		/// Tạo các test cases cho UpdateAsync - bao phủ các kịch bản: valid, booking not found, learner not found, tutorSubject not found, systemFee not found
		/// </summary>
		private static IEnumerable<TestCaseData> UpdateAsyncTestCases()
		{
			// Valid case
			yield return new TestCaseData(
				new BookingUpdateRequest
				{
					Id = 1,
					LearnerEmail = "newlearner@example.com",
					TutorSubjectId = 2,
					TotalSessions = 2
				},
				FakeDataFactory.CreateFakeBooking(1),
				FakeDataFactory.CreateFakeUser("newlearner@example.com"),
				FakeDataFactory.CreateFakeTutorSubject(2),
				FakeDataFactory.CreateFakeSystemFee(),
				true
			).SetName("UpdateAsync_ValidRequest_ReturnsUpdatedDto");

			// Booking not found
			yield return new TestCaseData(
				new BookingUpdateRequest
				{
					Id = 999,
					LearnerEmail = "newlearner@example.com",
					TutorSubjectId = 2,
					TotalSessions = 2
				},
				null,
				FakeDataFactory.CreateFakeUser("newlearner@example.com"),
				FakeDataFactory.CreateFakeTutorSubject(2),
				FakeDataFactory.CreateFakeSystemFee(),
				false
			).SetName("UpdateAsync_BookingNotFound_ThrowsException");

			// LearnerEmail not found (when updating)
			yield return new TestCaseData(
				new BookingUpdateRequest
				{
					Id = 1,
					LearnerEmail = "newlearner@example.com",
					TutorSubjectId = 2,
					TotalSessions = 2
				},
				FakeDataFactory.CreateFakeBooking(1),
				null,
				FakeDataFactory.CreateFakeTutorSubject(2),
				FakeDataFactory.CreateFakeSystemFee(),
				false
			).SetName("UpdateAsync_LearnerEmailNotFound_ThrowsException");

			// TutorSubject not found (when updating)
			yield return new TestCaseData(
				new BookingUpdateRequest
				{
					Id = 1,
					LearnerEmail = "newlearner@example.com",
					TutorSubjectId = 999,
					TotalSessions = 2
				},
				FakeDataFactory.CreateFakeBooking(1),
				FakeDataFactory.CreateFakeUser("newlearner@example.com"),
				null,
				FakeDataFactory.CreateFakeSystemFee(),
				false
			).SetName("UpdateAsync_TutorSubjectNotFound_ThrowsException");

			// SystemFee not found (when recalculating)
			yield return new TestCaseData(
				new BookingUpdateRequest
				{
					Id = 1,
					LearnerEmail = "newlearner@example.com",
					TutorSubjectId = 2,
					TotalSessions = 2
				},
				FakeDataFactory.CreateFakeBooking(1),
				FakeDataFactory.CreateFakeUser("newlearner@example.com"),
				FakeDataFactory.CreateFakeTutorSubject(2),
				null,
				false
			).SetName("UpdateAsync_SystemFeeNotFound_ThrowsException");
		}

		/// <summary>
		/// Test UpdateAsync với các scenarios khác nhau
		/// </summary>
		[Test]
		[TestCaseSource(nameof(UpdateAsyncTestCases))]
		public async Task UpdateAsync_WithVariousScenarios_HandlesCorrectly(
			BookingUpdateRequest request,
			BookingEntity? existingBooking,
			User? learner,
			TutorSubject? tutorSubject,
			SystemFee? systemFee,
			bool shouldSucceed)
		{
			// Arrange
			_bookingRepositoryMock
				.Setup(r => r.GetByIdAsync(request.Id))
				.ReturnsAsync(existingBooking);

			if (existingBooking != null)
			{
				if (request.LearnerEmail != null)
				{
					_userRepositoryMock
						.Setup(r => r.GetUserByEmailAsync(request.LearnerEmail))
						.ReturnsAsync(learner);
				}

				if (request.TutorSubjectId.HasValue)
				{
					_tutorSubjectRepositoryMock
						.Setup(r => r.GetByIdFullAsync(request.TutorSubjectId.Value))
						.ReturnsAsync(tutorSubject);
				}

				if (request.TotalSessions.HasValue)
				{
					_systemFeeRepositoryMock
						.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
						.ReturnsAsync(systemFee);
				}

				_bookingRepositoryMock
					.Setup(r => r.UpdateAsync(It.IsAny<BookingEntity>()))
					.Returns(Task.CompletedTask);
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

