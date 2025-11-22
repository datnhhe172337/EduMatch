using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.TutorProfile;
using EduMatch.BusinessLogicLayer.Requests.TutorVerificationRequest;
using EduMatch.BusinessLogicLayer.Requests.User;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Repositories;
using EduMatch.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.Tests;

/// <summary>
/// Test class cho TutorProfileService
/// </summary>
public class TutorProfileServiceTests
{
	private Mock<ITutorProfileRepository> _tutorProfileRepositoryMock;
	private Mock<ICloudMediaService> _cloudMediaServiceMock;
	private Mock<IUserService> _userServiceMock;
	private Mock<IUserProfileService> _userProfileServiceMock;
	private Mock<IQdrantService> _iqdrantServiceMock;
	private IMapper _mapper;
	private CurrentUserService _currentUserService;
	private Mock<ITutorVerificationRequestService> _tutorVerificationRequestServiceMock;
	private Mock<EduMatchContext> _contextMock;
	private Mock<IDbContextTransaction> _transactionMock;
	private TutorProfileService _service;

	/// <summary>
	/// Khởi tạo các mock objects và service trước mỗi test
	/// </summary>
	[SetUp]
	public void Setup()
	{
		_tutorProfileRepositoryMock = new Mock<ITutorProfileRepository>();
		_cloudMediaServiceMock = new Mock<ICloudMediaService>();
		_userServiceMock = new Mock<IUserService>();
		_userProfileServiceMock = new Mock<IUserProfileService>();
		_iqdrantServiceMock = new Mock<IQdrantService>();
		_tutorVerificationRequestServiceMock = new Mock<ITutorVerificationRequestService>();
		_contextMock = new Mock<EduMatchContext>();
		_transactionMock = new Mock<IDbContextTransaction>();

		var config = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<MappingProfile>();
		});
		_mapper = config.CreateMapper();

		_currentUserService = new CurrentUserServiceFake("abc@gmail.com");

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

		_service = new TutorProfileService(
			_tutorProfileRepositoryMock.Object,
			_mapper,
			_cloudMediaServiceMock.Object,
			_currentUserService,
			_userServiceMock.Object,
			_userProfileServiceMock.Object,
			_iqdrantServiceMock.Object,
			
			_tutorVerificationRequestServiceMock.Object,
			_contextMock.Object
		);
	}

	/// <summary>
	/// Test GetByIdFullAsync với các ID khác nhau - trả về DTO khi tồn tại, null khi không tồn tại
	/// </summary>
	[Test]
	[TestCase(1, true)]
	[TestCase(999, false)]
	public async Task GetByIdFullAsync_WithDifferentIds_ReturnsExpectedResult(int id, bool shouldExist)
	{
		// Arrange
		TutorProfile? entity = shouldExist ? FakeDataFactory.CreateFakeTutorProfile("abc@gmail.com") : null;
		if (entity != null) entity.Id = id;

		_tutorProfileRepositoryMock
			.Setup(r => r.GetByIdFullAsync(id))
			.ReturnsAsync(entity);

		// Act
		var result = await _service.GetByIdFullAsync(id);

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
	/// Test GetByIdFullAsync với ID không hợp lệ - ném ArgumentException
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(-1)]
	public void GetByIdFullAsync_WithInvalidId_ThrowsArgumentException(int id)
	{
		// Act & Assert
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetByIdFullAsync(id));
		Assert.That(exception.Message, Does.Contain("ID must be greater than 0"));
	}

	/// <summary>
	/// Test GetByEmailFullAsync với các email khác nhau - trả về DTO khi tồn tại, null khi không tồn tại
	/// </summary>
	[Test]
	[TestCase("abc@gmail.com", true)]
	[TestCase("nonexistent@gmail.com", false)]
	public async Task GetByEmailFullAsync_WithDifferentEmails_ReturnsExpectedResult(string email, bool shouldExist)
	{
		// Arrange
		TutorProfile? entity = shouldExist ? FakeDataFactory.CreateFakeTutorProfile(email) : null;

		_tutorProfileRepositoryMock
			.Setup(r => r.GetByEmailFullAsync(email))
			.ReturnsAsync(entity);

		// Act
		var result = await _service.GetByEmailFullAsync(email);

		// Assert
		if (shouldExist)
		{
			Assert.That(result, Is.Not.Null);
			Assert.That(result!.UserEmail, Is.EqualTo(email));
		}
		else
		{
			Assert.That(result, Is.Null);
		}
	}

	/// <summary>
	/// Test GetByEmailFullAsync với email không hợp lệ - ném ArgumentException
	/// </summary>
	[Test]
	[TestCase("")]
	[TestCase("   ")]
	[TestCase(null)]
	public void GetByEmailFullAsync_WithInvalidEmail_ThrowsArgumentException(string? email)
	{
		// Act & Assert
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetByEmailFullAsync(email!));
		Assert.That(exception.Message, Does.Contain("Email is required"));
	}

	/// <summary>
	/// Test GetAllFullAsync với số lượng khác nhau - trả về tất cả tutor profile với đầy đủ thông tin
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(5)]
	public async Task GetAllFullAsync_WithDifferentCounts_ReturnsExpectedList(int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorProfile($"tutor{i}@example.com"))
			.ToList();
		foreach (var entity in entities.Select((e, i) => new { e, i }))
		{
			entity.e.Id = entity.i + 1;
		}

		_tutorProfileRepositoryMock
			.Setup(r => r.GetAllFullAsync())
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetAllFullAsync();

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
	}

	/// <summary>
	/// Tạo các test cases cho CreateAsync - bao phủ các kịch bản: valid, profile exists, missing VideoIntroUrl, missing AvatarUrl, invalid YouTube URL
	/// </summary>
	private static IEnumerable<TestCaseData> CreateAsyncTestCases()
	{
		// Valid case
		yield return new TestCaseData(
			new TutorProfileCreateRequest
			{
				UserEmail = "abc@gmail.com",
				UserName = "Test User",
				Phone = "0123456789",
				Bio = "Test Bio",
				DateOfBirth = new DateTime(1990, 1, 1),
				AvatarUrl = "https://example.com/avatar.jpg",
				ProvinceId = 1,
				SubDistrictId = 1,
				TeachingExp = "5 years",
				VideoIntroUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
				TeachingModes = TeachingMode.Online,
				Latitude = 21.0285m,
				Longitude = 105.8542m
			},
			null,
			true,
			null,
			null // No pending verification requests
		).SetName("CreateAsync_ValidRequest_ReturnsCreatedDto");

		// Profile already exists with Approved status
		var approvedProfile = FakeDataFactory.CreateFakeTutorProfile("abc@gmail.com");
		approvedProfile.Status = (int)TutorStatus.Approved;
		yield return new TestCaseData(
			new TutorProfileCreateRequest
			{
				UserEmail = "abc@gmail.com",
				VideoIntroUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
				AvatarUrl = "https://example.com/avatar.jpg",
				TeachingModes = TeachingMode.Online
			},
			approvedProfile,
			false,
			typeof(Exception),
			null // No pending verification requests
		).SetName("CreateAsync_ProfileAlreadyApproved_ThrowsException");

		// Profile exists with Pending status and no pending verification request - should update
		var pendingProfile = FakeDataFactory.CreateFakeTutorProfile("abc@gmail.com");
		pendingProfile.Id = 1;
		pendingProfile.Status = (int)TutorStatus.Pending;
		yield return new TestCaseData(
			new TutorProfileCreateRequest
			{
				UserEmail = "abc@gmail.com",
				UserName = "Test User",
				Phone = "0123456789",
				Bio = "Updated Bio",
				DateOfBirth = new DateTime(1990, 1, 1),
				AvatarUrl = "https://example.com/avatar.jpg",
				ProvinceId = 1,
				SubDistrictId = 1,
				TeachingExp = "10 years",
				VideoIntroUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
				TeachingModes = TeachingMode.Online,
				Latitude = 21.0285m,
				Longitude = 105.8542m
			},
			pendingProfile,
			true,
			null,
			new List<TutorVerificationRequestDto>() // No pending verification requests
		).SetName("CreateAsync_ProfilePending_NoPendingRequest_UpdatesProfile");

		// Profile exists with Pending status and has pending verification request - should throw
		var pendingProfile2 = FakeDataFactory.CreateFakeTutorProfile("abc@gmail.com");
		pendingProfile2.Id = 1;
		pendingProfile2.Status = (int)TutorStatus.Pending;
		yield return new TestCaseData(
			new TutorProfileCreateRequest
			{
				UserEmail = "abc@gmail.com",
				VideoIntroUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
				AvatarUrl = "https://example.com/avatar.jpg",
				TeachingModes = TeachingMode.Online
			},
			pendingProfile2,
			false,
			typeof(Exception),
			new List<TutorVerificationRequestDto> { new TutorVerificationRequestDto { Id = 1, Status = TutorVerificationRequestStatus.Pending } } // Has pending verification request
		).SetName("CreateAsync_ProfilePending_HasPendingRequest_ThrowsException");

		// Profile exists with Rejected status and no pending verification request - should update
		var rejectedProfile = FakeDataFactory.CreateFakeTutorProfile("abc@gmail.com");
		rejectedProfile.Id = 1;
		rejectedProfile.Status = (int)TutorStatus.Rejected;
		yield return new TestCaseData(
			new TutorProfileCreateRequest
			{
				UserEmail = "abc@gmail.com",
				UserName = "Test User",
				Phone = "0123456789",
				Bio = "Updated Bio",
				DateOfBirth = new DateTime(1990, 1, 1),
				AvatarUrl = "https://example.com/avatar.jpg",
				ProvinceId = 1,
				SubDistrictId = 1,
				TeachingExp = "10 years",
				VideoIntroUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
				TeachingModes = TeachingMode.Online,
				Latitude = 21.0285m,
				Longitude = 105.8542m
			},
			rejectedProfile,
			true,
			null,
			new List<TutorVerificationRequestDto>() // No pending verification requests
		).SetName("CreateAsync_ProfileRejected_NoPendingRequest_UpdatesProfile");

		// Missing VideoIntroUrl
		yield return new TestCaseData(
			new TutorProfileCreateRequest
			{
				UserEmail = "abc@gmail.com",
				AvatarUrl = "https://example.com/avatar.jpg",
				TeachingModes = TeachingMode.Online
			},
			null,
			false,
			typeof(Exception),
			null
		).SetName("CreateAsync_MissingVideoIntroUrl_ThrowsException");

		// Missing AvatarUrl
		yield return new TestCaseData(
			new TutorProfileCreateRequest
			{
				UserEmail = "abc@gmail.com",
				VideoIntroUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
				TeachingModes = TeachingMode.Online
			},
			null,
			false,
			typeof(Exception),
			null
		).SetName("CreateAsync_MissingAvatarUrl_ThrowsException");

		// Invalid YouTube URL (invalid format)
		yield return new TestCaseData(
			new TutorProfileCreateRequest
			{
				UserEmail = "abc@gmail.com",
				VideoIntroUrl = "https://youtube.com/invalid",
				AvatarUrl = "https://example.com/avatar.jpg",
				TeachingModes = TeachingMode.Online
			},
			null,
			false,
			typeof(Exception),
			null
		).SetName("CreateAsync_InvalidYouTubeUrl_ThrowsException");
	}

	/// <summary>
	/// Test CreateAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
	/// </summary>
	[Test]
	[TestCaseSource(nameof(CreateAsyncTestCases))]
	public async Task CreateAsync_WithVariousScenarios_HandlesCorrectly(
		TutorProfileCreateRequest request,
		TutorProfile? existingProfile,
		bool shouldSucceed,
		Type? expectedExceptionType,
		List<TutorVerificationRequestDto>? pendingVerificationRequests)
	{
		// Arrange
		var email = "abc@gmail.com";
		_tutorProfileRepositoryMock
			.Setup(r => r.GetByEmailFullAsync(email))
			.ReturnsAsync(existingProfile);

		TutorProfile? capturedEntity = null;
		_tutorProfileRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<TutorProfile>()))
			.Returns(Task.CompletedTask)
			.Callback<TutorProfile>(tp =>
			{
				tp.Id = 1;
				capturedEntity = tp;
			});

		_tutorProfileRepositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<TutorProfile>()))
			.Returns(Task.CompletedTask);

		_userProfileServiceMock
			.Setup(s => s.UpdateAsync(It.IsAny<UserProfileUpdateRequest>()))
			.ReturnsAsync((UserProfileDto?)null);

		// Mock TutorVerificationRequestService - return pending requests based on test case
		var verificationRequests = pendingVerificationRequests ?? new List<TutorVerificationRequestDto>();
		_tutorVerificationRequestServiceMock
			.Setup(s => s.GetAllByEmailOrTutorIdAsync(
				It.IsAny<string>(), 
				It.IsAny<int?>(), 
				TutorVerificationRequestStatus.Pending))
			.ReturnsAsync(verificationRequests);

		_tutorVerificationRequestServiceMock
			.Setup(s => s.CreateAsync(It.IsAny<TutorVerificationRequestCreateRequest>()))
			.ReturnsAsync(new TutorVerificationRequestDto { Id = 1 });

		// Act & Assert
		if (shouldSucceed)
		{
			var result = await _service.CreateAsync(request);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.UserEmail, Is.EqualTo(email));
			Assert.That(result.Status, Is.EqualTo(TutorStatus.Pending));
			
			// Nếu existing profile là null thì tạo mới, ngược lại thì update
			if (existingProfile == null)
			{
				_tutorProfileRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TutorProfile>()), Times.Once);
			}
			else
			{
				_tutorProfileRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TutorProfile>()), Times.Once);
			}
		}
		else
		{
			var exception = Assert.ThrowsAsync<Exception>(async () => await _service.CreateAsync(request));
			Assert.That(exception, Is.InstanceOf(expectedExceptionType!));
		}
	}

	/// <summary>
	/// Tạo các test cases cho UpdateAsync - bao phủ các kịch bản: valid update, entity not found, status change valid/invalid
	/// </summary>
	private static IEnumerable<TestCaseData> UpdateAsyncTestCases()
	{
		// Valid update
		yield return new TestCaseData(
			new TutorProfileUpdateRequest
			{
				Id = 1,
				UserEmail = "abc@gmail.com",
				Bio = "Updated Bio",
				TeachingExp = "10 years",
				TeachingModes = TeachingMode.Hybrid
			},
			FakeDataFactory.CreateFakeTutorProfile("abc@gmail.com"),
			true,
			null
		).SetName("UpdateAsync_ValidRequest_ReturnsUpdatedDto");

		// Entity not found
		yield return new TestCaseData(
			new TutorProfileUpdateRequest
			{
				Id = 999,
				UserEmail = "test@gmail.com",
				TeachingModes = TeachingMode.Online
			},
			null,
			false,
			typeof(Exception)
		).SetName("UpdateAsync_EntityNotFound_ThrowsException");
	}

	/// <summary>
	/// Test UpdateAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
	/// </summary>
	[Test]
	[TestCaseSource(nameof(UpdateAsyncTestCases))]
	public async Task UpdateAsync_WithVariousScenarios_HandlesCorrectly(
		TutorProfileUpdateRequest request,
		TutorProfile? existingEntity,
		bool shouldSucceed,
		Type? expectedExceptionType)
	{
		// Arrange
		if (existingEntity != null)
		{
			_tutorProfileRepositoryMock
				.Setup(r => r.GetByIdFullAsync(request.Id))
				.ReturnsAsync(existingEntity);
		}
		else
		{
			_tutorProfileRepositoryMock
				.Setup(r => r.GetByIdFullAsync(request.Id))
				.ReturnsAsync((TutorProfile?)null);
		}

		_tutorProfileRepositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<TutorProfile>()))
			.Returns(Task.CompletedTask);

		_userProfileServiceMock
			.Setup(s => s.UpdateAsync(It.IsAny<UserProfileUpdateRequest>()))
			.ReturnsAsync((UserProfileDto?)null);

		// Act & Assert
		if (shouldSucceed)
		{
			var result = await _service.UpdateAsync(request);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.Id, Is.EqualTo(request.Id));

			_tutorProfileRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TutorProfile>()), Times.Once);
		}
		else
		{
			var exception = Assert.ThrowsAsync<Exception>(async () => await _service.UpdateAsync(request));
			Assert.That(exception, Is.InstanceOf(expectedExceptionType!));
		}
	}

	/// <summary>
	/// Test DeleteAsync với các ID khác nhau - gọi repository để xóa
	/// </summary>
	[Test]
	[TestCase(1)]
	[TestCase(10)]
	[TestCase(100)]
	public async Task DeleteAsync_WithDifferentIds_CallsRepository(int id)
	{
		// Arrange
		_tutorProfileRepositoryMock
			.Setup(r => r.RemoveByIdAsync(id))
			.Returns(Task.CompletedTask);

		// Act
		await _service.DeleteAsync(id);

		// Assert
		_tutorProfileRepositoryMock.Verify(r => r.RemoveByIdAsync(id), Times.Once);
	}

	/// <summary>
	/// Test DeleteAsync với ID không hợp lệ - ném ArgumentException
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
