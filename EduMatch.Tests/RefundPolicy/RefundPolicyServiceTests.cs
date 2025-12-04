using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.RefundPolicy;
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
/// Test class cho RefundPolicyService
/// </summary>
public class RefundPolicyServiceTests
{
	private Mock<IRefundPolicyRepository> _refundPolicyRepositoryMock;
	private IMapper _mapper;
	private CurrentUserServiceFake _currentUserService;
	private RefundPolicyService _service;

	/// <summary>
	/// Khởi tạo các mock objects và service trước mỗi test
	/// </summary>
	[SetUp]
	public void Setup()
	{
		_refundPolicyRepositoryMock = new Mock<IRefundPolicyRepository>();

		var config = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<MappingProfile>();
		});
		_mapper = config.CreateMapper();

		_currentUserService = new CurrentUserServiceFake("test@example.com");

		_service = new RefundPolicyService(
			_refundPolicyRepositoryMock.Object,
			_mapper,
			_currentUserService
		);
	}

	#region GetAllAsync Tests

	/// <summary>
	/// Test GetAllAsync không có filter - trả về tất cả policies
	/// </summary>
	[Test]
	public async Task GetAllAsync_NoFilter_ReturnsAllPolicies()
	{
		// Arrange
		var entities = new List<RefundPolicy>
		{
			CreateFakeRefundPolicy(1, "Policy 1", true),
			CreateFakeRefundPolicy(2, "Policy 2", false),
			CreateFakeRefundPolicy(3, "Policy 3", true)
		};

		_refundPolicyRepositoryMock
			.Setup(r => r.GetAllAsync(null))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetAllAsync();

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(3));
		_refundPolicyRepositoryMock.Verify(r => r.GetAllAsync(null), Times.Once);
	}

	/// <summary>
	/// Test GetAllAsync với filter IsActive = true - chỉ trả về policies active
	/// </summary>
	[Test]
	public async Task GetAllAsync_WithActiveFilter_ReturnsOnlyActivePolicies()
	{
		// Arrange
		var entities = new List<RefundPolicy>
		{
			CreateFakeRefundPolicy(1, "Policy 1", true),
			CreateFakeRefundPolicy(3, "Policy 3", true)
		};

		_refundPolicyRepositoryMock
			.Setup(r => r.GetAllAsync(true))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetAllAsync(true);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(2));
		Assert.That(result.All(p => p.IsActive == true), Is.True);
		_refundPolicyRepositoryMock.Verify(r => r.GetAllAsync(true), Times.Once);
	}

	/// <summary>
	/// Test GetAllAsync với filter IsActive = false - chỉ trả về policies inactive
	/// </summary>
	[Test]
	public async Task GetAllAsync_WithInactiveFilter_ReturnsOnlyInactivePolicies()
	{
		// Arrange
		var entities = new List<RefundPolicy>
		{
			CreateFakeRefundPolicy(2, "Policy 2", false)
		};

		_refundPolicyRepositoryMock
			.Setup(r => r.GetAllAsync(false))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetAllAsync(false);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(1));
		Assert.That(result.All(p => p.IsActive == false), Is.True);
		_refundPolicyRepositoryMock.Verify(r => r.GetAllAsync(false), Times.Once);
	}

	/// <summary>
	/// Test GetAllAsync khi không có data - trả về empty list
	/// </summary>
	[Test]
	public async Task GetAllAsync_NoData_ReturnsEmptyList()
	{
		// Arrange
		var entities = new List<RefundPolicy>();

		_refundPolicyRepositoryMock
			.Setup(r => r.GetAllAsync(null))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetAllAsync();

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(0));
		_refundPolicyRepositoryMock.Verify(r => r.GetAllAsync(null), Times.Once);
	}

	/// <summary>
	/// Test GetAllAsync khi repository throw exception
	/// </summary>
	[Test]
	public void GetAllAsync_RepositoryThrowsException_ThrowsException()
	{
		// Arrange
		_refundPolicyRepositoryMock
			.Setup(r => r.GetAllAsync(null))
			.ThrowsAsync(new InvalidOperationException("Database error"));

		// Act & Assert
		var exception = Assert.ThrowsAsync<Exception>(async () => await _service.GetAllAsync());
		Assert.That(exception.Message, Does.Contain("Lỗi khi lấy danh sách chính sách hoàn tiền"));
		Assert.That(exception.Message, Does.Contain("Database error"));
	}

	#endregion

	#region GetByIdAsync Tests

	/// <summary>
	/// Test GetByIdAsync với ID hợp lệ và entity tồn tại - trả về DTO
	/// </summary>
	[Test]
	public async Task GetByIdAsync_ValidIdAndEntityExists_ReturnsDto()
	{
		// Arrange
		var id = 1;
		var entity = CreateFakeRefundPolicy(id, "Policy 1", true);

		_refundPolicyRepositoryMock
			.Setup(r => r.GetByIdAsync(id))
			.ReturnsAsync(entity);

		// Act
		var result = await _service.GetByIdAsync(id);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Id, Is.EqualTo(id));
		Assert.That(result.Name, Is.EqualTo("Policy 1"));
		_refundPolicyRepositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
	}

	/// <summary>
	/// Test GetByIdAsync với ID hợp lệ nhưng entity không tồn tại - trả về null
	/// </summary>
	[Test]
	public async Task GetByIdAsync_ValidIdButEntityNotFound_ReturnsNull()
	{
		// Arrange
		var id = 999;

		_refundPolicyRepositoryMock
			.Setup(r => r.GetByIdAsync(id))
			.ReturnsAsync((RefundPolicy?)null);

		// Act
		var result = await _service.GetByIdAsync(id);

		// Assert
		Assert.That(result, Is.Null);
		_refundPolicyRepositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
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
		_refundPolicyRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
	}

	/// <summary>
	/// Test GetByIdAsync khi repository throw exception
	/// </summary>
	[Test]
	public void GetByIdAsync_RepositoryThrowsException_ThrowsException()
	{
		// Arrange
		var id = 1;
		_refundPolicyRepositoryMock
			.Setup(r => r.GetByIdAsync(id))
			.ThrowsAsync(new InvalidOperationException("Database error"));

		// Act & Assert
		var exception = Assert.ThrowsAsync<Exception>(async () => await _service.GetByIdAsync(id));
		Assert.That(exception.Message, Does.Contain("Lỗi khi lấy chính sách hoàn tiền"));
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
		var request = new RefundPolicyCreateRequest
		{
			Name = "Test Policy",
			Description = "Test Description",
			RefundPercentage = 50
		};

		RefundPolicy? capturedEntity = null;
		_refundPolicyRepositoryMock
			.Setup(r => r.CreateAsync(It.IsAny<RefundPolicy>()))
			.Returns(Task.CompletedTask)
			.Callback<RefundPolicy>(e => capturedEntity = e);

		// Act
		var result = await _service.CreateAsync(request);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Name, Is.EqualTo(request.Name));
		Assert.That(result.Description, Is.EqualTo(request.Description));
		Assert.That(result.RefundPercentage, Is.EqualTo(request.RefundPercentage));
		Assert.That(result.IsActive, Is.True);
		Assert.That(result.CreatedBy, Is.EqualTo("test@example.com"));

		Assert.That(capturedEntity, Is.Not.Null);
		Assert.That(capturedEntity!.Name, Is.EqualTo(request.Name));
		Assert.That(capturedEntity.Description, Is.EqualTo(request.Description));
		Assert.That(capturedEntity.RefundPercentage, Is.EqualTo(request.RefundPercentage));
		Assert.That(capturedEntity.IsActive, Is.True);
		Assert.That(capturedEntity.CreatedBy, Is.EqualTo("test@example.com"));
		Assert.That(capturedEntity.UpdatedAt, Is.Null);
		Assert.That(capturedEntity.UpdatedBy, Is.Null);

		_refundPolicyRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<RefundPolicy>()), Times.Once);
	}

	/// <summary>
	/// Test CreateAsync với Description null - vẫn tạo thành công
	/// </summary>
	[Test]
	public async Task CreateAsync_DescriptionIsNull_CreatesSuccessfully()
	{
		// Arrange
		var request = new RefundPolicyCreateRequest
		{
			Name = "Test Policy",
			Description = null,
			RefundPercentage = 75
		};

		_refundPolicyRepositoryMock
			.Setup(r => r.CreateAsync(It.IsAny<RefundPolicy>()))
			.Returns(Task.CompletedTask);

		// Act
		var result = await _service.CreateAsync(request);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Description, Is.Null);
		Assert.That(result.RefundPercentage, Is.EqualTo(75));
		_refundPolicyRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<RefundPolicy>()), Times.Once);
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
		_refundPolicyRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<RefundPolicy>()), Times.Never);
	}

	/// <summary>
	/// Test CreateAsync với Name rỗng hoặc whitespace - throw exception
	/// </summary>
	[Test]
	[TestCase("")]
	[TestCase(" ")]
	[TestCase("   ")]
	[TestCase(null)]
	public void CreateAsync_NameIsEmptyOrWhitespace_ThrowsException(string? invalidName)
	{
		// Arrange
		var request = new RefundPolicyCreateRequest
		{
			Name = invalidName!,
			Description = "Test Description",
			RefundPercentage = 50
		};

		// Act & Assert
		var exception = Assert.ThrowsAsync<Exception>(async () => await _service.CreateAsync(request));
		Assert.That(exception.Message, Does.Contain("Tên chính sách hoàn tiền không được để trống"));
		_refundPolicyRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<RefundPolicy>()), Times.Never);
	}

	/// <summary>
	/// Test CreateAsync với RefundPercentage ngoài phạm vi [0, 100] - throw exception
	/// </summary>
	[Test]
	[TestCase(-1)]
	[TestCase(101)]
	[TestCase(150)]
	[TestCase(-50)]
	public void CreateAsync_RefundPercentageOutOfRange_ThrowsException(decimal invalidPercentage)
	{
		// Arrange
		var request = new RefundPolicyCreateRequest
		{
			Name = "Test Policy",
			Description = "Test Description",
			RefundPercentage = invalidPercentage
		};

		// Act & Assert
		var exception = Assert.ThrowsAsync<Exception>(async () => await _service.CreateAsync(request));
		Assert.That(exception.Message, Does.Contain("Tỷ lệ hoàn tiền phải từ 0 đến 100"));
		_refundPolicyRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<RefundPolicy>()), Times.Never);
	}

	/// <summary>
	/// Test CreateAsync với RefundPercentage trong phạm vi hợp lệ [0, 100] - tạo thành công
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(100)]
	[TestCase(50)]
	[TestCase(25.5)]
	public async Task CreateAsync_RefundPercentageInValidRange_CreatesSuccessfully(decimal validPercentage)
	{
		// Arrange
		var request = new RefundPolicyCreateRequest
		{
			Name = "Test Policy",
			Description = "Test Description",
			RefundPercentage = validPercentage
		};

		_refundPolicyRepositoryMock
			.Setup(r => r.CreateAsync(It.IsAny<RefundPolicy>()))
			.Returns(Task.CompletedTask);

		// Act
		var result = await _service.CreateAsync(request);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.RefundPercentage, Is.EqualTo(validPercentage));
		_refundPolicyRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<RefundPolicy>()), Times.Once);
	}

	/// <summary>
	/// Test CreateAsync khi CurrentUserService Email rỗng - throw exception
	/// </summary>
	[Test]
	public void CreateAsync_CurrentUserEmailIsEmpty_ThrowsException()
	{
		// Arrange
		var serviceWithEmptyEmail = new RefundPolicyService(
			_refundPolicyRepositoryMock.Object,
			_mapper,
			new CurrentUserServiceFake("")
		);

		var request = new RefundPolicyCreateRequest
		{
			Name = "Test Policy",
			Description = "Test Description",
			RefundPercentage = 50
		};

		// Act & Assert
		var exception = Assert.ThrowsAsync<Exception>(async () => await serviceWithEmptyEmail.CreateAsync(request));
		Assert.That(exception.Message, Does.Contain("Không thể xác định người dùng hiện tại"));
		_refundPolicyRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<RefundPolicy>()), Times.Never);
	}

	/// <summary>
	/// Test CreateAsync khi repository throw exception
	/// </summary>
	[Test]
	public void CreateAsync_RepositoryThrowsException_ThrowsException()
	{
		// Arrange
		var request = new RefundPolicyCreateRequest
		{
			Name = "Test Policy",
			Description = "Test Description",
			RefundPercentage = 50
		};

		_refundPolicyRepositoryMock
			.Setup(r => r.CreateAsync(It.IsAny<RefundPolicy>()))
			.ThrowsAsync(new InvalidOperationException("Database error"));

		// Act & Assert
		var exception = Assert.ThrowsAsync<Exception>(async () => await _service.CreateAsync(request));
		Assert.That(exception.Message, Does.Contain("Lỗi khi tạo chính sách hoàn tiền"));
		Assert.That(exception.Message, Does.Contain("Database error"));
	}

	#endregion

	#region Helper Methods

	/// <summary>
	/// Tạo fake RefundPolicy entity cho testing
	/// </summary>
	private RefundPolicy CreateFakeRefundPolicy(int id, string name, bool isActive)
	{
		return new RefundPolicy
		{
			Id = id,
			Name = name,
			Description = $"Description for {name}",
			RefundPercentage = 50,
			IsActive = isActive,
			CreatedAt = DateTime.UtcNow,
			CreatedBy = "test@example.com",
			UpdatedAt = null,
			UpdatedBy = null
		};
	}

	#endregion
}

