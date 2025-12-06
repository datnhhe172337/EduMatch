using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.RefundRequestEvidence;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.Tests.Common;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.Tests;

/// <summary>
/// Test class cho RefundRequestEvidenceService
/// </summary>
public class RefundRequestEvidenceServiceTests
{
	private Mock<IRefundRequestEvidenceRepository> _refundRequestEvidenceRepositoryMock;
	private Mock<IBookingRefundRequestRepository> _bookingRefundRequestRepositoryMock;
	private IMapper _mapper;
	private RefundRequestEvidenceService _service;

	/// <summary>
	/// Khởi tạo các mock objects và service trước mỗi test
	/// </summary>
	[SetUp]
	public void Setup()
	{
		_refundRequestEvidenceRepositoryMock = new Mock<IRefundRequestEvidenceRepository>();
		_bookingRefundRequestRepositoryMock = new Mock<IBookingRefundRequestRepository>();

		var config = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<MappingProfile>();
		});
		_mapper = config.CreateMapper();

		_service = new RefundRequestEvidenceService(
			_refundRequestEvidenceRepositoryMock.Object,
			_bookingRefundRequestRepositoryMock.Object,
			_mapper
		);
	}

	/// <summary>
	/// Test GetByIdAsync với ID hợp lệ và entity tồn tại - trả về DTO
	/// </summary>
	[Test]
	public async Task GetByIdAsync_ValidIdAndEntityExists_ReturnsDto()
	{
		// Arrange
		var id = 1;
		var entity = FakeDataFactory.CreateFakeRefundRequestEvidence(id, 1, "https://example.com/file1.jpg");

		_refundRequestEvidenceRepositoryMock
			.Setup(r => r.GetByIdAsync(id))
			.ReturnsAsync(entity);

		// Act
		var result = await _service.GetByIdAsync(id);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Id, Is.EqualTo(id));
		Assert.That(result.BookingRefundRequestId, Is.EqualTo(1));
		Assert.That(result.FileUrl, Is.EqualTo("https://example.com/file1.jpg"));
		_refundRequestEvidenceRepositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
	}

	/// <summary>
	/// Test GetByIdAsync với ID hợp lệ nhưng entity không tồn tại - trả về null
	/// </summary>
	[Test]
	public async Task GetByIdAsync_ValidIdButEntityNotFound_ReturnsNull()
	{
		// Arrange
		var id = 999;

		_refundRequestEvidenceRepositoryMock
			.Setup(r => r.GetByIdAsync(id))
			.ReturnsAsync((RefundRequestEvidence?)null);

		// Act
		var result = await _service.GetByIdAsync(id);

		// Assert
		Assert.That(result, Is.Null);
		_refundRequestEvidenceRepositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
	}

	/// <summary>
	/// Test GetByIdAsync với ID không hợp lệ (<= 0) - throw exception
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(-1)]
	[TestCase(-100)]
	public void GetByIdAsync_InvalidId_ThrowsException(int invalidId)
	{
		// Act & Assert
		var exception = Assert.ThrowsAsync<Exception>(async () => await _service.GetByIdAsync(invalidId));
		Assert.That(exception.Message, Does.Contain("Id phải lớn hơn 0"));
		_refundRequestEvidenceRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
	}

	/// <summary>
	/// Test GetByIdAsync khi repository throw exception
	/// </summary>
	[Test]
	public void GetByIdAsync_RepositoryThrowsException_ThrowsException()
	{
		// Arrange
		var id = 1;
		_refundRequestEvidenceRepositoryMock
			.Setup(r => r.GetByIdAsync(id))
			.ThrowsAsync(new InvalidOperationException("Database error"));

		// Act & Assert
		var exception = Assert.ThrowsAsync<Exception>(async () => await _service.GetByIdAsync(id));
		Assert.That(exception.Message, Does.Contain("Lỗi khi lấy bằng chứng hoàn tiền"));
		Assert.That(exception.Message, Does.Contain("Database error"));
	}

	/// <summary>
	/// Test GetByBookingRefundRequestIdAsync với ID hợp lệ - trả về danh sách DTOs
	/// </summary>
	[Test]
	public async Task GetByBookingRefundRequestIdAsync_ValidId_ReturnsList()
	{
		// Arrange
		var bookingRefundRequestId = 1;
		var entities = new List<RefundRequestEvidence>
		{
			FakeDataFactory.CreateFakeRefundRequestEvidence(1, bookingRefundRequestId, "https://example.com/file1.jpg"),
			FakeDataFactory.CreateFakeRefundRequestEvidence(2, bookingRefundRequestId, "https://example.com/file2.jpg"),
			FakeDataFactory.CreateFakeRefundRequestEvidence(3, bookingRefundRequestId, "https://example.com/file3.jpg")
		};

		_refundRequestEvidenceRepositoryMock
			.Setup(r => r.GetByBookingRefundRequestIdAsync(bookingRefundRequestId))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetByBookingRefundRequestIdAsync(bookingRefundRequestId);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(3));
		Assert.That(result.All(e => e.BookingRefundRequestId == bookingRefundRequestId), Is.True);
		_refundRequestEvidenceRepositoryMock.Verify(r => r.GetByBookingRefundRequestIdAsync(bookingRefundRequestId), Times.Once);
	}

	/// <summary>
	/// Test GetByBookingRefundRequestIdAsync khi không có evidence - trả về empty list
	/// </summary>
	[Test]
	public async Task GetByBookingRefundRequestIdAsync_NoEvidence_ReturnsEmptyList()
	{
		// Arrange
		var bookingRefundRequestId = 1;
		var entities = new List<RefundRequestEvidence>();

		_refundRequestEvidenceRepositoryMock
			.Setup(r => r.GetByBookingRefundRequestIdAsync(bookingRefundRequestId))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetByBookingRefundRequestIdAsync(bookingRefundRequestId);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(0));
		_refundRequestEvidenceRepositoryMock.Verify(r => r.GetByBookingRefundRequestIdAsync(bookingRefundRequestId), Times.Once);
	}

	/// <summary>
	/// Test GetByBookingRefundRequestIdAsync với ID không hợp lệ (<= 0) - throw exception
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(-1)]
	[TestCase(-100)]
	public void GetByBookingRefundRequestIdAsync_InvalidId_ThrowsException(int invalidId)
	{
		// Act & Assert
		var exception = Assert.ThrowsAsync<Exception>(async () => await _service.GetByBookingRefundRequestIdAsync(invalidId));
		Assert.That(exception.Message, Does.Contain("BookingRefundRequestId phải lớn hơn 0"));
		_refundRequestEvidenceRepositoryMock.Verify(r => r.GetByBookingRefundRequestIdAsync(It.IsAny<int>()), Times.Never);
	}

	/// <summary>
	/// Test GetByBookingRefundRequestIdAsync khi repository throw exception
	/// </summary>
	[Test]
	public void GetByBookingRefundRequestIdAsync_RepositoryThrowsException_ThrowsException()
	{
		// Arrange
		var bookingRefundRequestId = 1;
		_refundRequestEvidenceRepositoryMock
			.Setup(r => r.GetByBookingRefundRequestIdAsync(bookingRefundRequestId))
			.ThrowsAsync(new InvalidOperationException("Database error"));

		// Act & Assert
		var exception = Assert.ThrowsAsync<Exception>(async () => await _service.GetByBookingRefundRequestIdAsync(bookingRefundRequestId));
		Assert.That(exception.Message, Does.Contain("Lỗi khi lấy danh sách bằng chứng hoàn tiền"));
		Assert.That(exception.Message, Does.Contain("Database error"));
	}

	/// <summary>
	/// Tạo các test cases cho CreateAsync - bao phủ các kịch bản: valid, null request, invalid BookingRefundRequestId, empty FileUrl, BookingRefundRequest not found
	/// </summary>
	private static IEnumerable<TestCaseData> CreateAsyncTestCases()
	{
		// Valid case
		yield return new TestCaseData(
			new RefundRequestEvidenceCreateRequest
			{
				BookingRefundRequestId = 1,
				FileUrl = "https://example.com/evidence.jpg"
			},
			FakeDataFactory.CreateFakeBookingRefundRequest(1),
			true,
			null
		).SetName("CreateAsync_ValidRequest_ReturnsCreatedDto");

		// Null request
		yield return new TestCaseData(
			null!,
			null,
			false,
			typeof(Exception)
		).SetName("CreateAsync_RequestIsNull_ThrowsException");

		// Invalid BookingRefundRequestId - zero
		yield return new TestCaseData(
			new RefundRequestEvidenceCreateRequest
			{
				BookingRefundRequestId = 0,
				FileUrl = "https://example.com/evidence.jpg"
			},
			null,
			false,
			typeof(Exception)
		).SetName("CreateAsync_BookingRefundRequestIdIsZero_ThrowsException");

		// Invalid BookingRefundRequestId - negative
		yield return new TestCaseData(
			new RefundRequestEvidenceCreateRequest
			{
				BookingRefundRequestId = -1,
				FileUrl = "https://example.com/evidence.jpg"
			},
			null,
			false,
			typeof(Exception)
		).SetName("CreateAsync_BookingRefundRequestIdIsNegative_ThrowsException");

		// Empty FileUrl
		yield return new TestCaseData(
			new RefundRequestEvidenceCreateRequest
			{
				BookingRefundRequestId = 1,
				FileUrl = ""
			},
			null,
			false,
			typeof(Exception)
		).SetName("CreateAsync_FileUrlIsEmpty_ThrowsException");

		// Whitespace FileUrl
		yield return new TestCaseData(
			new RefundRequestEvidenceCreateRequest
			{
				BookingRefundRequestId = 1,
				FileUrl = "   "
			},
			null,
			false,
			typeof(Exception)
		).SetName("CreateAsync_FileUrlIsWhitespace_ThrowsException");

		// Null FileUrl
		yield return new TestCaseData(
			new RefundRequestEvidenceCreateRequest
			{
				BookingRefundRequestId = 1,
				FileUrl = null!
			},
			null,
			false,
			typeof(Exception)
		).SetName("CreateAsync_FileUrlIsNull_ThrowsException");

		// BookingRefundRequest not found
		yield return new TestCaseData(
			new RefundRequestEvidenceCreateRequest
			{
				BookingRefundRequestId = 999,
				FileUrl = "https://example.com/evidence.jpg"
			},
			null,
			false,
			typeof(Exception)
		).SetName("CreateAsync_BookingRefundRequestNotFound_ThrowsException");
	}

	/// <summary>
	/// Test CreateAsync với các scenarios khác nhau
	/// </summary>
	[Test]
	[TestCaseSource(nameof(CreateAsyncTestCases))]
	public async Task CreateAsync_WithVariousScenarios_HandlesCorrectly(
		RefundRequestEvidenceCreateRequest? request,
		BookingRefundRequest? bookingRefundRequest,
		bool shouldSucceed,
		Type? expectedExceptionType)
	{
		// Arrange
		if (request != null && request.BookingRefundRequestId > 0)
		{
			_bookingRefundRequestRepositoryMock
				.Setup(r => r.GetByIdAsync(request.BookingRefundRequestId))
				.ReturnsAsync(bookingRefundRequest);
		}

		// Mock mapper để map từ request sang entity
		var mapperMock = new Mock<IMapper>();
		if (request != null && shouldSucceed)
		{
			var entity = FakeDataFactory.CreateFakeRefundRequestEvidence(1, request.BookingRefundRequestId, request.FileUrl);
			var dto = _mapper.Map<RefundRequestEvidenceDto>(entity);

			mapperMock.Setup(m => m.Map<RefundRequestEvidence>(It.IsAny<RefundRequestEvidenceCreateRequest>()))
				.Returns<RefundRequestEvidenceCreateRequest>(req => new RefundRequestEvidence
				{
					BookingRefundRequestId = req.BookingRefundRequestId,
					FileUrl = req.FileUrl,
					CreatedAt = DateTime.UtcNow
				});
			mapperMock.Setup(m => m.Map<RefundRequestEvidenceDto>(It.IsAny<RefundRequestEvidence>()))
				.Returns<RefundRequestEvidence>(e => dto);
		}

		var serviceWithMockedMapper = new RefundRequestEvidenceService(
			_refundRequestEvidenceRepositoryMock.Object,
			_bookingRefundRequestRepositoryMock.Object,
			shouldSucceed ? mapperMock.Object : _mapper
		);

		if (shouldSucceed)
		{
			_refundRequestEvidenceRepositoryMock
				.Setup(r => r.CreateAsync(It.IsAny<RefundRequestEvidence>()))
				.Returns(Task.CompletedTask);

			// Act
			var result = await serviceWithMockedMapper.CreateAsync(request!);

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.BookingRefundRequestId, Is.EqualTo(request!.BookingRefundRequestId));
			Assert.That(result.FileUrl, Is.EqualTo(request.FileUrl));
			_bookingRefundRequestRepositoryMock.Verify(r => r.GetByIdAsync(request.BookingRefundRequestId), Times.Once);
			_refundRequestEvidenceRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<RefundRequestEvidence>()), Times.Once);
		}
		else
		{
			// Act & Assert
			var exception = Assert.ThrowsAsync<Exception>(async () => await serviceWithMockedMapper.CreateAsync(request!));
			Assert.That(exception, Is.InstanceOf(expectedExceptionType!));
		}
	}

	/// <summary>
	/// Test CreateAsync khi repository throw exception
	/// </summary>
	[Test]
	public void CreateAsync_RepositoryThrowsException_ThrowsException()
	{
		// Arrange
		var request = new RefundRequestEvidenceCreateRequest
		{
			BookingRefundRequestId = 1,
			FileUrl = "https://example.com/evidence.jpg"
		};

		var bookingRefundRequest = FakeDataFactory.CreateFakeBookingRefundRequest(1);
		var entity = FakeDataFactory.CreateFakeRefundRequestEvidence(1, request.BookingRefundRequestId, request.FileUrl);
		var dto = _mapper.Map<RefundRequestEvidenceDto>(entity);

		_bookingRefundRequestRepositoryMock
			.Setup(r => r.GetByIdAsync(request.BookingRefundRequestId))
			.ReturnsAsync(bookingRefundRequest);

		// Mock mapper để map từ request sang entity
		var mapperMock = new Mock<IMapper>();
		mapperMock.Setup(m => m.Map<RefundRequestEvidence>(It.IsAny<RefundRequestEvidenceCreateRequest>()))
			.Returns<RefundRequestEvidenceCreateRequest>(req => new RefundRequestEvidence
			{
				BookingRefundRequestId = req.BookingRefundRequestId,
				FileUrl = req.FileUrl,
				CreatedAt = DateTime.UtcNow
			});
		mapperMock.Setup(m => m.Map<RefundRequestEvidenceDto>(It.IsAny<RefundRequestEvidence>()))
			.Returns<RefundRequestEvidence>(e => dto);

		var serviceWithMockedMapper = new RefundRequestEvidenceService(
			_refundRequestEvidenceRepositoryMock.Object,
			_bookingRefundRequestRepositoryMock.Object,
			mapperMock.Object
		);

		_refundRequestEvidenceRepositoryMock
			.Setup(r => r.CreateAsync(It.IsAny<RefundRequestEvidence>()))
			.ThrowsAsync(new InvalidOperationException("Database error"));

		// Act & Assert
		var exception = Assert.ThrowsAsync<Exception>(async () => await serviceWithMockedMapper.CreateAsync(request));
		Assert.That(exception.Message, Does.Contain("Lỗi khi tạo bằng chứng hoàn tiền"));
		Assert.That(exception.Message, Does.Contain("Database error"));
	}

	/// <summary>
	/// Test CreateAsync khi BookingRefundRequestRepository throw exception
	/// </summary>
	[Test]
	public void CreateAsync_BookingRefundRequestRepositoryThrowsException_ThrowsException()
	{
		// Arrange
		var request = new RefundRequestEvidenceCreateRequest
		{
			BookingRefundRequestId = 1,
			FileUrl = "https://example.com/evidence.jpg"
		};

		_bookingRefundRequestRepositoryMock
			.Setup(r => r.GetByIdAsync(request.BookingRefundRequestId))
			.ThrowsAsync(new InvalidOperationException("Database error"));

		// Act & Assert
		var exception = Assert.ThrowsAsync<Exception>(async () => await _service.CreateAsync(request));
		Assert.That(exception.Message, Does.Contain("Lỗi khi tạo bằng chứng hoàn tiền"));
		Assert.That(exception.Message, Does.Contain("Database error"));
	}
}
