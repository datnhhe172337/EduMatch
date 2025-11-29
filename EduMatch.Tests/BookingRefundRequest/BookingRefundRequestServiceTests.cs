using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.BookingRefundRequest;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.Tests
{
	/// <summary>
	/// Test class cho BookingRefundRequestService
	/// </summary>
	public class BookingRefundRequestServiceTests
	{
		private Mock<IBookingRefundRequestRepository> _bookingRefundRequestRepositoryMock;
		private Mock<IBookingRepository> _bookingRepositoryMock;
		private Mock<IRefundPolicyRepository> _refundPolicyRepositoryMock;
		private Mock<IUserRepository> _userRepositoryMock;
		private Mock<IRefundRequestEvidenceRepository> _refundRequestEvidenceRepositoryMock;
		private CurrentUserService _currentUserService;
		private Mock<EduMatchContext> _contextMock;
		private Mock<IDbContextTransaction> _transactionMock;
		private Mock<IBookingService> _bookingServiceMock;
		private Mock<INotificationService> _notificationServiceMock;
		private IMapper _mapper;
		private BookingRefundRequestService _service;

		/// <summary>
		/// Khởi tạo các mock objects và service trước mỗi test
		/// </summary>
		[SetUp]
		public void Setup()
		{
			_bookingRefundRequestRepositoryMock = new Mock<IBookingRefundRequestRepository>();
			_bookingRepositoryMock = new Mock<IBookingRepository>();
			_refundPolicyRepositoryMock = new Mock<IRefundPolicyRepository>();
			_userRepositoryMock = new Mock<IUserRepository>();
			_refundRequestEvidenceRepositoryMock = new Mock<IRefundRequestEvidenceRepository>();
			_currentUserService = new CurrentUserServiceFake("admin@example.com");
			_bookingServiceMock = new Mock<IBookingService>();
			_notificationServiceMock = new Mock<INotificationService>();
			_contextMock = new Mock<EduMatchContext>();
			_transactionMock = new Mock<IDbContextTransaction>();

			var config = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			_mapper = config.CreateMapper();

			// Mock Database transaction
			var databaseMock = new Mock<DatabaseFacade>(_contextMock.Object);
			_contextMock.Setup(c => c.Database).Returns(databaseMock.Object);
			databaseMock.Setup(d => d.BeginTransactionAsync(It.IsAny<System.Threading.CancellationToken>()))
				.ReturnsAsync(_transactionMock.Object);
			_transactionMock.Setup(t => t.CommitAsync(It.IsAny<System.Threading.CancellationToken>()))
				.Returns(Task.CompletedTask);
			_transactionMock.Setup(t => t.RollbackAsync(It.IsAny<System.Threading.CancellationToken>()))
				.Returns(Task.CompletedTask);
			_transactionMock.Setup(t => t.Dispose()).Verifiable();

			_service = new BookingRefundRequestService(
				_bookingRefundRequestRepositoryMock.Object,
				_bookingRepositoryMock.Object,
				_refundPolicyRepositoryMock.Object,
				_userRepositoryMock.Object,
				_refundRequestEvidenceRepositoryMock.Object,
				_mapper,
				_currentUserService,
				_contextMock.Object,
				_bookingServiceMock.Object,
				_notificationServiceMock.Object
			);
		}

		/// <summary>
		/// Test GetAllAsync với status khác nhau
		/// </summary>
		[Test]
		[TestCase(null, 2)]
		[TestCase(BookingRefundRequestStatus.Pending, 1)]
		[TestCase(BookingRefundRequestStatus.Approved, 1)]
		[TestCase(BookingRefundRequestStatus.Rejected, 0)]
		public async Task GetAllAsync_WithDifferentStatus_ReturnsExpectedList(BookingRefundRequestStatus? status, int expectedCount)
		{
			// Arrange
			var requests = new List<BookingRefundRequest>
			{
				FakeDataFactory.CreateFakeBookingRefundRequest(1, status: (int)BookingRefundRequestStatus.Pending),
				FakeDataFactory.CreateFakeBookingRefundRequest(2, status: (int)BookingRefundRequestStatus.Approved)
			};

			int? statusInt = status.HasValue ? (int?)status.Value : null;
			var filteredRequests = statusInt.HasValue 
				? requests.Where(r => r.Status == statusInt.Value).ToList()
				: requests;

			_bookingRefundRequestRepositoryMock
				.Setup(r => r.GetAllAsync(statusInt))
				.ReturnsAsync(filteredRequests);

			// Act
			var result = await _service.GetAllAsync(status);

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Count, Is.EqualTo(expectedCount));
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
			BookingRefundRequest? entity = shouldExist ? FakeDataFactory.CreateFakeBookingRefundRequest(id) : null;

			_bookingRefundRequestRepositoryMock
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
		public void GetByIdAsync_WithInvalidId_ThrowsException(int id)
		{
			// Act & Assert
			var exception = Assert.ThrowsAsync<Exception>(async () => await _service.GetByIdAsync(id));
			Assert.That(exception.Message, Does.Contain("Id phải lớn hơn 0"));
		}

		/// <summary>
		/// Test GetAllByEmailAsync với email và status khác nhau
		/// </summary>
		[Test]
		[TestCase("learner@example.com", null, 2)]
		[TestCase("learner@example.com", BookingRefundRequestStatus.Pending, 1)]
		[TestCase("learner2@example.com", null, 0)]
		public async Task GetAllByEmailAsync_WithDifferentParams_ReturnsExpectedList(string learnerEmail, BookingRefundRequestStatus? status, int expectedCount)
		{
			// Arrange
			var requests = expectedCount > 0
				? new List<BookingRefundRequest>
				{
					FakeDataFactory.CreateFakeBookingRefundRequest(1, learnerEmail: learnerEmail, status: (int)BookingRefundRequestStatus.Pending),
					FakeDataFactory.CreateFakeBookingRefundRequest(2, learnerEmail: learnerEmail, status: (int)BookingRefundRequestStatus.Approved)
				}
				: new List<BookingRefundRequest>();

			int? statusInt = status.HasValue ? (int?)status.Value : null;
			var filteredRequests = statusInt.HasValue
				? requests.Where(r => r.Status == statusInt.Value).ToList()
				: requests;

			_bookingRefundRequestRepositoryMock
				.Setup(r => r.GetAllByEmailAsync(learnerEmail, statusInt))
				.ReturnsAsync(filteredRequests);

			// Act
			var result = await _service.GetAllByEmailAsync(learnerEmail, status);

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Count, Is.EqualTo(expectedCount));
		}

		/// <summary>
		/// Test GetAllByEmailAsync với empty email
		/// </summary>
		[Test]
		[TestCase("")]
		[TestCase("   ")]
		[TestCase(null)]
		public void GetAllByEmailAsync_WithEmptyEmail_ThrowsException(string? learnerEmail)
		{
			// Act & Assert
			var exception = Assert.ThrowsAsync<Exception>(async () => await _service.GetAllByEmailAsync(learnerEmail!));
			Assert.That(exception.Message, Does.Contain("Email không được để trống"));
		}

		/// <summary>
		/// Test data cho CreateAsync
		/// </summary>
		private static IEnumerable<TestCaseData> CreateAsyncTestCases()
		{
			// Valid case
			var validBooking = FakeDataFactory.CreateFakeBooking(
				1,
				learnerEmail: "learner@example.com",
				paymentStatus: (int)PaymentStatus.Paid,
				status: (int)BookingStatus.Confirmed);
			var validUser = FakeDataFactory.CreateFakeUser("learner@example.com");
			var validRefundPolicy = FakeDataFactory.CreateFakeRefundPolicy(1, refundPercentage: 50, isActive: true);

			yield return new TestCaseData(
				new BookingRefundRequestCreateRequest
				{
					BookingId = 1,
					LearnerEmail = "learner@example.com",
					RefundPolicyId = 1,
					Reason = "Test reason",
					FileUrls = null
				},
				validBooking,
				validUser,
				validRefundPolicy,
				new List<BookingRefundRequest>(), // existingRequests
				true,
				null
			).SetName("CreateAsync_ValidRequest_ReturnsCreatedDto");

			// Booking không tồn tại
			yield return new TestCaseData(
				new BookingRefundRequestCreateRequest
				{
					BookingId = 999,
					LearnerEmail = "learner@example.com",
					RefundPolicyId = 1
				},
				null, // booking
				null,
				null,
				null,
				false,
				typeof(Exception)
			).SetName("CreateAsync_WhenBookingNotExists_ThrowsException");

			// Booking status không phải Confirmed
			var bookingNotConfirmed = FakeDataFactory.CreateFakeBooking(
				1,
				learnerEmail: "learner@example.com",
				paymentStatus: (int)PaymentStatus.Paid,
				status: (int)BookingStatus.Pending);
			yield return new TestCaseData(
				new BookingRefundRequestCreateRequest
				{
					BookingId = 1,
					LearnerEmail = "learner@example.com",
					RefundPolicyId = 1
				},
				bookingNotConfirmed,
				validUser,
				validRefundPolicy,
				new List<BookingRefundRequest>(),
				false,
				typeof(Exception)
			).SetName("CreateAsync_WhenBookingNotConfirmed_ThrowsException");

			// Booking payment status không phải Paid
			var bookingNotPaid = FakeDataFactory.CreateFakeBooking(
				1,
				learnerEmail: "learner@example.com",
				paymentStatus: (int)PaymentStatus.Pending,
				status: (int)BookingStatus.Confirmed);
			yield return new TestCaseData(
				new BookingRefundRequestCreateRequest
				{
					BookingId = 1,
					LearnerEmail = "learner@example.com",
					RefundPolicyId = 1
				},
				bookingNotPaid,
				validUser,
				validRefundPolicy,
				new List<BookingRefundRequest>(),
				false,
				typeof(Exception)
			).SetName("CreateAsync_WhenBookingNotPaid_ThrowsException");

			// Đã có request Pending
			var existingPendingRequest = FakeDataFactory.CreateFakeBookingRefundRequest(
				1,
				bookingId: 1,
				status: (int)BookingRefundRequestStatus.Pending);
			yield return new TestCaseData(
				new BookingRefundRequestCreateRequest
				{
					BookingId = 1,
					LearnerEmail = "learner@example.com",
					RefundPolicyId = 1
				},
				validBooking,
				validUser,
				validRefundPolicy,
				new List<BookingRefundRequest> { existingPendingRequest },
				false,
				typeof(Exception)
			).SetName("CreateAsync_WhenPendingRequestExists_ThrowsException");

			// LearnerEmail không tồn tại
			yield return new TestCaseData(
				new BookingRefundRequestCreateRequest
				{
					BookingId = 1,
					LearnerEmail = "nonexistent@example.com",
					RefundPolicyId = 1
				},
				validBooking,
				null, // user
				validRefundPolicy,
				new List<BookingRefundRequest>(),
				false,
				typeof(Exception)
			).SetName("CreateAsync_WhenLearnerEmailNotExists_ThrowsException");

			// RefundPolicy không tồn tại
			yield return new TestCaseData(
				new BookingRefundRequestCreateRequest
				{
					BookingId = 1,
					LearnerEmail = "learner@example.com",
					RefundPolicyId = 999
				},
				validBooking,
				validUser,
				null, // refundPolicy
				new List<BookingRefundRequest>(),
				false,
				typeof(Exception)
			).SetName("CreateAsync_WhenRefundPolicyNotExists_ThrowsException");

			// RefundPolicy không active
			var inactiveRefundPolicy = FakeDataFactory.CreateFakeRefundPolicy(1, isActive: false);
			yield return new TestCaseData(
				new BookingRefundRequestCreateRequest
				{
					BookingId = 1,
					LearnerEmail = "learner@example.com",
					RefundPolicyId = 1
				},
				validBooking,
				validUser,
				inactiveRefundPolicy,
				new List<BookingRefundRequest>(),
				false,
				typeof(Exception)
			).SetName("CreateAsync_WhenRefundPolicyNotActive_ThrowsException");
		}

		/// <summary>
		/// Test CreateAsync với các scenarios khác nhau
		/// </summary>
		[Test]
		[TestCaseSource(nameof(CreateAsyncTestCases))]
		public async Task CreateAsync_WithVariousScenarios_HandlesCorrectly(
			BookingRefundRequestCreateRequest request,
			Booking? booking,
			User? user,
			RefundPolicy? refundPolicy,
			List<BookingRefundRequest>? existingRequests,
			bool shouldSucceed,
			Type? expectedExceptionType)
		{
			// Arrange
			_bookingRepositoryMock
				.Setup(r => r.GetByIdAsync(request.BookingId))
				.ReturnsAsync(booking);

			if (booking != null)
			{
				_bookingRefundRequestRepositoryMock
					.Setup(r => r.GetAllByBookingIdAsync(request.BookingId))
					.ReturnsAsync(existingRequests ?? new List<BookingRefundRequest>());
			}

			_userRepositoryMock
				.Setup(r => r.GetUserByEmailAsync(request.LearnerEmail))
				.ReturnsAsync(user);

			_refundPolicyRepositoryMock
				.Setup(r => r.GetByIdAsync(request.RefundPolicyId))
				.ReturnsAsync(refundPolicy);

			if (shouldSucceed)
			{
				_bookingRefundRequestRepositoryMock
					.Setup(r => r.CreateAsync(It.IsAny<BookingRefundRequest>()))
					.Returns(Task.CompletedTask);

				if (request.FileUrls != null && request.FileUrls.Any())
				{
					_refundRequestEvidenceRepositoryMock
						.Setup(r => r.CreateAsync(It.IsAny<RefundRequestEvidence>()))
						.Returns(Task.CompletedTask);
				}
			}

			// Act & Assert
			if (shouldSucceed)
			{
				var result = await _service.CreateAsync(request);
				Assert.That(result, Is.Not.Null);
				Assert.That(result.BookingId, Is.EqualTo(request.BookingId));
				Assert.That(result.LearnerEmail, Is.EqualTo(request.LearnerEmail));
				Assert.That(result.RefundPolicyId, Is.EqualTo(request.RefundPolicyId));
			}
			else
			{
				var exception = Assert.ThrowsAsync<Exception>(async () => await _service.CreateAsync(request));
				Assert.That(exception, Is.Not.Null);
			}
		}

		/// <summary>
		/// Test CreateAsync với FileUrls
		/// </summary>
		[Test]
		public async Task CreateAsync_WithFileUrls_CreatesEvidence()
		{
			// Arrange
			var request = new BookingRefundRequestCreateRequest
			{
				BookingId = 1,
				LearnerEmail = "learner@example.com",
				RefundPolicyId = 1,
				Reason = "Test reason",
				FileUrls = new List<string> { "https://example.com/file1.jpg", "https://example.com/file2.jpg" }
			};

			var booking = FakeDataFactory.CreateFakeBooking(
				1,
				learnerEmail: "learner@example.com",
				paymentStatus: (int)PaymentStatus.Paid,
				status: (int)BookingStatus.Confirmed);
			var user = FakeDataFactory.CreateFakeUser("learner@example.com");
			var refundPolicy = FakeDataFactory.CreateFakeRefundPolicy(1, refundPercentage: 50, isActive: true);

			_bookingRepositoryMock
				.Setup(r => r.GetByIdAsync(request.BookingId))
				.ReturnsAsync(booking);

			_bookingRefundRequestRepositoryMock
				.Setup(r => r.GetAllByBookingIdAsync(request.BookingId))
				.ReturnsAsync(new List<BookingRefundRequest>());

			_userRepositoryMock
				.Setup(r => r.GetUserByEmailAsync(request.LearnerEmail))
				.ReturnsAsync(user);

			_refundPolicyRepositoryMock
				.Setup(r => r.GetByIdAsync(request.RefundPolicyId))
				.ReturnsAsync(refundPolicy);

			_bookingRefundRequestRepositoryMock
				.Setup(r => r.CreateAsync(It.IsAny<BookingRefundRequest>()))
				.Returns(Task.CompletedTask);

			_refundRequestEvidenceRepositoryMock
				.Setup(r => r.CreateAsync(It.IsAny<RefundRequestEvidence>()))
				.Returns(Task.CompletedTask);

			// Act
			var result = await _service.CreateAsync(request);

			// Assert
			Assert.That(result, Is.Not.Null);
			_refundRequestEvidenceRepositoryMock.Verify(
				r => r.CreateAsync(It.IsAny<RefundRequestEvidence>()),
				Times.Exactly(2));
		}
	}
}

