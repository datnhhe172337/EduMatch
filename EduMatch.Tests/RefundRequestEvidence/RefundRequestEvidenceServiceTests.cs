using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.RefundRequestEvidence;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
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

	#region GetByIdAsync Tests

	/// <summary>
	/// Test GetByIdAsync với ID hợp lệ và entity tồn tại - trả về DTO
	/// </summary>
	[Test]
	public async Task GetByIdAsync_ValidIdAndEntityExists_ReturnsDto()
	{
		// Arrange
		var id = 1;
		var entity = CreateFakeRefundRequestEvidence(id, 1, "https://example.com/file1.jpg");

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

	#endregion

	#region GetByBookingRefundRequestIdAsync Tests

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
			CreateFakeRefundRequestEvidence(1, bookingRefundRequestId, "https://example.com/file1.jpg"),
			CreateFakeRefundRequestEvidence(2, bookingRefundRequestId, "https://example.com/file2.jpg"),
			CreateFakeRefundRequestEvidence(3, bookingRefundRequestId, "https://example.com/file3.jpg")
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

	#endregion

	#region CreateAsync Tests

	/// <summary>
	/// Test CreateAsync với request hợp lệ - tạo thành công và trả về DTO
	/// </summary>
	[Test]
	public async Task CreateAsync_ValidRequest_CreatesAndReturnsDto()
	{
		// Arrange
		var request = new RefundRequestEvidenceCreateRequest
		{
			BookingRefundRequestId = 1,
			FileUrl = "https://example.com/evidence.jpg"
		};

		var bookingRefundRequest = CreateFakeBookingRefundRequest(1);
		var entity = CreateFakeRefundRequestEvidence(1, request.BookingRefundRequestId, request.FileUrl);
		var dto = _mapper.Map<RefundRequestEvidenceDto>(entity);
		RefundRequestEvidence? capturedEntity = null;

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
			.Returns(Task.CompletedTask)
			.Callback<RefundRequestEvidence>(e => capturedEntity = e);

		// Act
		var result = await serviceWithMockedMapper.CreateAsync(request);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.BookingRefundRequestId, Is.EqualTo(request.BookingRefundRequestId));
		Assert.That(result.FileUrl, Is.EqualTo(request.FileUrl));

		Assert.That(capturedEntity, Is.Not.Null);
		Assert.That(capturedEntity!.BookingRefundRequestId, Is.EqualTo(request.BookingRefundRequestId));
		Assert.That(capturedEntity.FileUrl, Is.EqualTo(request.FileUrl));

		_bookingRefundRequestRepositoryMock.Verify(r => r.GetByIdAsync(request.BookingRefundRequestId), Times.Once);
		_refundRequestEvidenceRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<RefundRequestEvidence>()), Times.Once);
	}

	/// <summary>
	/// Test CreateAsync với request null - throw exception
	/// </summary>
	[Test]
	public void CreateAsync_RequestIsNull_ThrowsException()
	{
		// Act & Assert
		var exception = Assert.ThrowsAsync<Exception>(async () => await _service.CreateAsync(null!));
		Assert.That(exception.Message, Does.Contain("Yêu cầu không được để trống"));
		_bookingRefundRequestRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
		_refundRequestEvidenceRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<RefundRequestEvidence>()), Times.Never);
	}

	/// <summary>
	/// Test CreateAsync với BookingRefundRequestId không hợp lệ (<= 0) - throw exception
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(-1)]
	[TestCase(-100)]
	public void CreateAsync_InvalidBookingRefundRequestId_ThrowsException(int invalidId)
	{
		// Arrange
		var request = new RefundRequestEvidenceCreateRequest
		{
			BookingRefundRequestId = invalidId,
			FileUrl = "https://example.com/evidence.jpg"
		};

		// Act & Assert
		var exception = Assert.ThrowsAsync<Exception>(async () => await _service.CreateAsync(request));
		Assert.That(exception.Message, Does.Contain("BookingRefundRequestId phải lớn hơn 0"));
		_bookingRefundRequestRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
		_refundRequestEvidenceRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<RefundRequestEvidence>()), Times.Never);
	}

	/// <summary>
	/// Test CreateAsync với FileUrl rỗng hoặc whitespace - throw exception
	/// </summary>
	[Test]
	[TestCase("")]
	[TestCase(" ")]
	[TestCase("   ")]
	[TestCase(null)]
	public void CreateAsync_FileUrlIsEmptyOrWhitespace_ThrowsException(string? invalidFileUrl)
	{
		// Arrange
		var request = new RefundRequestEvidenceCreateRequest
		{
			BookingRefundRequestId = 1,
			FileUrl = invalidFileUrl!
		};

		// Act & Assert
		var exception = Assert.ThrowsAsync<Exception>(async () => await _service.CreateAsync(request));
		Assert.That(exception.Message, Does.Contain("FileUrl không được để trống"));
		_bookingRefundRequestRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
		_refundRequestEvidenceRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<RefundRequestEvidence>()), Times.Never);
	}

	/// <summary>
	/// Test CreateAsync khi BookingRefundRequest không tồn tại - throw exception
	/// </summary>
	[Test]
	public void CreateAsync_BookingRefundRequestNotFound_ThrowsException()
	{
		// Arrange
		var request = new RefundRequestEvidenceCreateRequest
		{
			BookingRefundRequestId = 999,
			FileUrl = "https://example.com/evidence.jpg"
		};

		_bookingRefundRequestRepositoryMock
			.Setup(r => r.GetByIdAsync(request.BookingRefundRequestId))
			.ReturnsAsync((BookingRefundRequest?)null);

		// Act & Assert
		var exception = Assert.ThrowsAsync<Exception>(async () => await _service.CreateAsync(request));
		Assert.That(exception.Message, Does.Contain("BookingRefundRequest không tồn tại"));
		_bookingRefundRequestRepositoryMock.Verify(r => r.GetByIdAsync(request.BookingRefundRequestId), Times.Once);
		_refundRequestEvidenceRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<RefundRequestEvidence>()), Times.Never);
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

		var bookingRefundRequest = CreateFakeBookingRefundRequest(1);
		var entity = CreateFakeRefundRequestEvidence(1, request.BookingRefundRequestId, request.FileUrl);
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

	#endregion

	#region Helper Methods

	/// <summary>
	/// Tạo fake RefundRequestEvidence entity cho testing
	/// </summary>
	private RefundRequestEvidence CreateFakeRefundRequestEvidence(int id, int bookingRefundRequestId, string fileUrl)
	{
		return new RefundRequestEvidence
		{
			Id = id,
			BookingRefundRequestId = bookingRefundRequestId,
			FileUrl = fileUrl,
			CreatedAt = DateTime.UtcNow,
			BookingRefundRequest = CreateFakeBookingRefundRequest(bookingRefundRequestId)
		};
	}

	/// <summary>
	/// Tạo fake BookingRefundRequest entity cho testing
	/// </summary>
	private BookingRefundRequest CreateFakeBookingRefundRequest(int id)
	{
		return new BookingRefundRequest
		{
			Id = id,
			BookingId = 1,
			LearnerEmail = "learner@example.com",
			RefundPolicyId = 1,
			Reason = "Test reason",
			Status = 0,
			CreatedAt = DateTime.UtcNow
		};
	}

	#endregion
}

