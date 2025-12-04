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

	/// <summary>
	/// Test GetAllAsync không có filter - trả về tất cả policies
	/// </summary>
	[Test]
	public async Task GetAllAsync_NoFilter_ReturnsAllPolicies()
	{
		// Arrange
		var entities = new List<RefundPolicy>
		{
			FakeDataFactory.CreateFakeRefundPolicy(1, "Policy 1", 50, true),
			FakeDataFactory.CreateFakeRefundPolicy(2, "Policy 2", 50, false),
			FakeDataFactory.CreateFakeRefundPolicy(3, "Policy 3", 50, true)
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
			FakeDataFactory.CreateFakeRefundPolicy(1, "Policy 1", 50, true),
			FakeDataFactory.CreateFakeRefundPolicy(3, "Policy 3", 50, true)
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
			FakeDataFactory.CreateFakeRefundPolicy(2, "Policy 2", 50, false)
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

	/// <summary>
	/// Test GetByIdAsync với ID hợp lệ và entity tồn tại - trả về DTO
	/// </summary>
	[Test]
	public async Task GetByIdAsync_ValidIdAndEntityExists_ReturnsDto()
	{
		// Arrange
		var id = 1;
		var entity = FakeDataFactory.CreateFakeRefundPolicy(id, "Policy 1", 50, true);

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

	/// <summary>
	/// Tạo các test cases cho CreateAsync - bao phủ các kịch bản: valid, null request, empty name, invalid percentage, empty email
	/// </summary>
	private static IEnumerable<TestCaseData> CreateAsyncTestCases()
	{
		// Valid case
		yield return new TestCaseData(
			new RefundPolicyCreateRequest
			{
				Name = "Test Policy",
				Description = "Test Description",
				RefundPercentage = 50
			},
			"test@example.com",
			true,
			null
		).SetName("CreateAsync_ValidRequest_ReturnsCreatedDto");

		// Valid case with null description
		yield return new TestCaseData(
			new RefundPolicyCreateRequest
			{
				Name = "Test Policy",
				Description = null,
				RefundPercentage = 75
			},
			"test@example.com",
			true,
			null
		).SetName("CreateAsync_DescriptionIsNull_CreatesSuccessfully");

		// Valid percentage at boundary (0)
		yield return new TestCaseData(
			new RefundPolicyCreateRequest
			{
				Name = "Test Policy",
				Description = "Test Description",
				RefundPercentage = 0
			},
			"test@example.com",
			true,
			null
		).SetName("CreateAsync_RefundPercentageIsZero_CreatesSuccessfully");

		// Valid percentage at boundary (100)
		yield return new TestCaseData(
			new RefundPolicyCreateRequest
			{
				Name = "Test Policy",
				Description = "Test Description",
				RefundPercentage = 100
			},
			"test@example.com",
			true,
			null
		).SetName("CreateAsync_RefundPercentageIs100_CreatesSuccessfully");

		// Valid percentage with decimal
		yield return new TestCaseData(
			new RefundPolicyCreateRequest
			{
				Name = "Test Policy",
				Description = "Test Description",
				RefundPercentage = 25.5m
			},
			"test@example.com",
			true,
			null
		).SetName("CreateAsync_RefundPercentageIsDecimal_CreatesSuccessfully");

		// Null request
		yield return new TestCaseData(
			null!,
			"test@example.com",
			false,
			typeof(Exception)
		).SetName("CreateAsync_RequestIsNull_ThrowsException");

		// Empty name
		yield return new TestCaseData(
			new RefundPolicyCreateRequest
			{
				Name = "",
				Description = "Test Description",
				RefundPercentage = 50
			},
			"test@example.com",
			false,
			typeof(Exception)
		).SetName("CreateAsync_NameIsEmpty_ThrowsException");

		// Whitespace name
		yield return new TestCaseData(
			new RefundPolicyCreateRequest
			{
				Name = "   ",
				Description = "Test Description",
				RefundPercentage = 50
			},
			"test@example.com",
			false,
			typeof(Exception)
		).SetName("CreateAsync_NameIsWhitespace_ThrowsException");

		// Null name
		yield return new TestCaseData(
			new RefundPolicyCreateRequest
			{
				Name = null!,
				Description = "Test Description",
				RefundPercentage = 50
			},
			"test@example.com",
			false,
			typeof(Exception)
		).SetName("CreateAsync_NameIsNull_ThrowsException");

		// Invalid percentage - negative
		yield return new TestCaseData(
			new RefundPolicyCreateRequest
			{
				Name = "Test Policy",
				Description = "Test Description",
				RefundPercentage = -1
			},
			"test@example.com",
			false,
			typeof(Exception)
		).SetName("CreateAsync_RefundPercentageIsNegative_ThrowsException");

		// Invalid percentage - over 100
		yield return new TestCaseData(
			new RefundPolicyCreateRequest
			{
				Name = "Test Policy",
				Description = "Test Description",
				RefundPercentage = 101
			},
			"test@example.com",
			false,
			typeof(Exception)
		).SetName("CreateAsync_RefundPercentageOver100_ThrowsException");

		// Empty email
		yield return new TestCaseData(
			new RefundPolicyCreateRequest
			{
				Name = "Test Policy",
				Description = "Test Description",
				RefundPercentage = 50
			},
			"",
			false,
			typeof(Exception)
		).SetName("CreateAsync_CurrentUserEmailIsEmpty_ThrowsException");
	}

	/// <summary>
	/// Test CreateAsync với các scenarios khác nhau
	/// </summary>
	[Test]
	[TestCaseSource(nameof(CreateAsyncTestCases))]
	public async Task CreateAsync_WithVariousScenarios_HandlesCorrectly(
		RefundPolicyCreateRequest? request,
		string currentUserEmail,
		bool shouldSucceed,
		Type? expectedExceptionType)
	{
		// Arrange
		var currentUserService = new CurrentUserServiceFake(currentUserEmail);
		var service = new RefundPolicyService(
			_refundPolicyRepositoryMock.Object,
			_mapper,
			currentUserService
		);

		if (shouldSucceed && request != null)
		{
			_refundPolicyRepositoryMock
				.Setup(r => r.CreateAsync(It.IsAny<RefundPolicy>()))
				.Returns(Task.CompletedTask);

			// Act
			var result = await service.CreateAsync(request);

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.Name, Is.EqualTo(request.Name));
			Assert.That(result.Description, Is.EqualTo(request.Description));
			Assert.That(result.RefundPercentage, Is.EqualTo(request.RefundPercentage));
			Assert.That(result.IsActive, Is.True);
			Assert.That(result.CreatedBy, Is.EqualTo(currentUserEmail));
			_refundPolicyRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<RefundPolicy>()), Times.Once);
		}
		else
		{
			// Act & Assert
			var exception = Assert.ThrowsAsync<Exception>(async () => await service.CreateAsync(request!));
			Assert.That(exception, Is.InstanceOf(expectedExceptionType!));
			_refundPolicyRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<RefundPolicy>()), Times.Never);
		}
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
}
