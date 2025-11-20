//using AutoMapper;
//using EduMatch.BusinessLogicLayer.DTOs;
//using EduMatch.BusinessLogicLayer.Interfaces;
//using EduMatch.BusinessLogicLayer.Mappings;
//using EduMatch.BusinessLogicLayer.Requests.TutorProfile;
//using EduMatch.BusinessLogicLayer.Requests.User;
//using EduMatch.BusinessLogicLayer.Services;
//using EduMatch.DataAccessLayer.Entities;
//using EduMatch.DataAccessLayer.Enum;
//using EduMatch.DataAccessLayer.Interfaces;
//using EduMatch.DataAccessLayer.Repositories;
//using EduMatch.Tests.Common;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace EduMatch.Tests;

///// <summary>
///// Test class cho TutorProfileService
///// </summary>
//public class TutorProfileServiceTests
//{
//	private Mock<ITutorProfileRepository> _tutorProfileRepositoryMock;
//	private Mock<ICloudMediaService> _cloudMediaServiceMock;
//	private Mock<IUserService> _userServiceMock;
//	private Mock<IUserProfileService> _userProfileServiceMock;
//	private IMapper _mapper;
//	private CurrentUserService _currentUserService;
//	private Mock<ITutorVerificationRequestService> _tutorVertificationRequestServiceMock;
//	private TutorProfileService _service;

//	/// <summary>
//	/// Khởi tạo các mock objects và service trước mỗi test
//	/// </summary>
//	[SetUp]
//	public void Setup()
//	{
//		_tutorProfileRepositoryMock = new Mock<ITutorProfileRepository>();
//		_cloudMediaServiceMock = new Mock<ICloudMediaService>();
//		_userServiceMock = new Mock<IUserService>();
//		_userProfileServiceMock = new Mock<IUserProfileService>();
//		_tutorVertificationRequestRepositoryMock = new Mock<ITutorVerificationRequestRepository>();

//		var config = new MapperConfiguration(cfg =>
//		{
//			cfg.AddProfile<MappingProfile>();
//		});
//		_mapper = config.CreateMapper();

//		_currentUserService = new CurrentUserServiceFake("abc@gmail.com");

//		_service = new TutorProfileService(

//			_tutorProfileRepositoryMock.Object,
//			_mapper,
//			_cloudMediaServiceMock.Object,
//			_currentUserService,
//			_userServiceMock.Object,
//			_userProfileServiceMock.Object,
//			_tutorVertificationRequestRepositoryMock.Object

//		);
//	}

//	/// <summary>
//	/// Test GetByIdFullAsync với các ID khác nhau - trả về DTO khi tồn tại, null khi không tồn tại
//	/// </summary>
//	[Test]
//	[TestCase(1, true)]
//	[TestCase(999, false)]
//	public async Task GetByIdFullAsync_WithDifferentIds_ReturnsExpectedResult(int id, bool shouldExist)
//	{
//		// Arrange
//		TutorProfile? entity = shouldExist ? FakeDataFactory.CreateFakeTutorProfile("abc@gmail.com") : null;
//		if (entity != null) entity.Id = id;

//		_tutorProfileRepositoryMock
//			.Setup(r => r.GetByIdFullAsync(id))
//			.ReturnsAsync(entity);

//		// Act
//		var result = await _service.GetByIdFullAsync(id);

//		// Assert
//		if (shouldExist)
//		{
//			Assert.That(result, Is.Not.Null);
//			Assert.That(result!.Id, Is.EqualTo(id));
//		}
//		else
//		{
//			Assert.That(result, Is.Null);
//		}
//	}

//	/// <summary>
//	/// Test GetByIdFullAsync với ID không hợp lệ - ném ArgumentException
//	/// </summary>
//	[Test]
//	[TestCase(0)]
//	[TestCase(-1)]
//	public void GetByIdFullAsync_WithInvalidId_ThrowsArgumentException(int id)
//	{
//		// Act & Assert
//		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetByIdFullAsync(id));
//		Assert.That(exception.Message, Does.Contain("ID must be greater than 0"));
//	}

//	/// <summary>
//	/// Test GetByEmailFullAsync với các email khác nhau - trả về DTO khi tồn tại, null khi không tồn tại
//	/// </summary>
//	[Test]
//	[TestCase("abc@gmail.com", true)]
//	[TestCase("nonexistent@gmail.com", false)]
//	public async Task GetByEmailFullAsync_WithDifferentEmails_ReturnsExpectedResult(string email, bool shouldExist)
//	{
//		// Arrange
//		TutorProfile? entity = shouldExist ? FakeDataFactory.CreateFakeTutorProfile(email) : null;

//		_tutorProfileRepositoryMock
//			.Setup(r => r.GetByEmailFullAsync(email))
//			.ReturnsAsync(entity);

//		// Act
//		var result = await _service.GetByEmailFullAsync(email);

//		// Assert
//		if (shouldExist)
//		{
//			Assert.That(result, Is.Not.Null);
//			Assert.That(result!.UserEmail, Is.EqualTo(email));
//		}
//		else
//		{
//			Assert.That(result, Is.Null);
//		}
//	}

//	/// <summary>
//	/// Test GetByEmailFullAsync với email không hợp lệ - ném ArgumentException
//	/// </summary>
//	[Test]
//	[TestCase("")]
//	[TestCase("   ")]
//	[TestCase(null)]
//	public void GetByEmailFullAsync_WithInvalidEmail_ThrowsArgumentException(string? email)
//	{
//		// Act & Assert
//		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetByEmailFullAsync(email!));
//		Assert.That(exception.Message, Does.Contain("Email is required"));
//	}

//	/// <summary>
//	/// Test GetAllFullAsync với số lượng khác nhau - trả về tất cả tutor profile với đầy đủ thông tin
//	/// </summary>
//	[Test]
//	[TestCase(0)]
//	[TestCase(1)]
//	[TestCase(5)]
//	public async Task GetAllFullAsync_WithDifferentCounts_ReturnsExpectedList(int count)
//	{
//		// Arrange
//		var entities = Enumerable.Range(1, count)
//			.Select(i => FakeDataFactory.CreateFakeTutorProfile($"tutor{i}@example.com"))
//			.ToList();
//		foreach (var entity in entities.Select((e, i) => new { e, i }))
//		{
//			entity.e.Id = entity.i + 1;
//		}

//		_tutorProfileRepositoryMock
//			.Setup(r => r.GetAllFullAsync())
//			.ReturnsAsync(entities);

//		// Act
//		var result = await _service.GetAllFullAsync();

//		// Assert
//		Assert.That(result, Is.Not.Null);
//		Assert.That(result.Count, Is.EqualTo(count));
//	}

//	/// <summary>
//	/// Tạo các test cases cho CreateAsync - bao phủ các kịch bản: valid, profile exists, missing VideoIntroUrl, missing AvatarUrl, invalid YouTube URL
//	/// </summary>
//	private static IEnumerable<TestCaseData> CreateAsyncTestCases()
//	{
//		// Valid case
//		yield return new TestCaseData(
//			new TutorProfileCreateRequest
//			{
//				UserEmail = "abc@gmail.com",
//				UserName = "Test User",
//				Phone = "0123456789",
//				Bio = "Test Bio",
//				DateOfBirth = new DateTime(1990, 1, 1),
//				AvatarUrl = "https://example.com/avatar.jpg",
//				ProvinceId = 1,
//				SubDistrictId = 1,
//				TeachingExp = "5 years",
//				VideoIntroUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
//				TeachingModes = TeachingMode.Online,
//				Latitude = 21.0285m,
//				Longitude = 105.8542m
//			},
//			null,
//			true,
//			null
//		).SetName("CreateAsync_ValidRequest_ReturnsCreatedDto");

//		// Profile already exists
//		yield return new TestCaseData(
//			new TutorProfileCreateRequest
//			{
//				UserEmail = "abc@gmail.com",
//				VideoIntroUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
//				AvatarUrl = "https://example.com/avatar.jpg",
//				TeachingModes = TeachingMode.Online
//			},
//			FakeDataFactory.CreateFakeTutorProfile("abc@gmail.com"),
//			false,
//			typeof(InvalidOperationException)
//		).SetName("CreateAsync_ProfileAlreadyExists_ThrowsInvalidOperationException");

//		// Missing VideoIntroUrl
//		yield return new TestCaseData(
//			new TutorProfileCreateRequest
//			{
//				UserEmail = "abc@gmail.com",
//				AvatarUrl = "https://example.com/avatar.jpg",
//				TeachingModes = TeachingMode.Online
//			},
//			null,
//			false,
//			typeof(InvalidOperationException)
//		).SetName("CreateAsync_MissingVideoIntroUrl_ThrowsInvalidOperationException");

//		// Missing AvatarUrl
//		yield return new TestCaseData(
//			new TutorProfileCreateRequest
//			{
//				UserEmail = "abc@gmail.com",
//				VideoIntroUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
//				TeachingModes = TeachingMode.Online
//			},
//			null,
//			false,
//			typeof(InvalidOperationException)
//		).SetName("CreateAsync_MissingAvatarUrl_ThrowsInvalidOperationException");

//		// Invalid YouTube URL (invalid format)
//		yield return new TestCaseData(
//			new TutorProfileCreateRequest
//			{
//				UserEmail = "abc@gmail.com",
//				VideoIntroUrl = "https://youtube.com/invalid",
//				AvatarUrl = "https://example.com/avatar.jpg",
//				TeachingModes = TeachingMode.Online
//			},
//			null,
//			false,
//			typeof(InvalidOperationException)
//		).SetName("CreateAsync_InvalidYouTubeUrl_ThrowsInvalidOperationException");
//	}

//	/// <summary>
//	/// Test CreateAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
//	/// </summary>
//	[Test]
//	[TestCaseSource(nameof(CreateAsyncTestCases))]
//	public async Task CreateAsync_WithVariousScenarios_HandlesCorrectly(
//		TutorProfileCreateRequest request,
//		TutorProfile? existingProfile,
//		bool shouldSucceed,
//		Type? expectedExceptionType)
//	{
//		// Arrange
//		var email = "abc@gmail.com";
//		_tutorProfileRepositoryMock
//			.Setup(r => r.GetByEmailFullAsync(email))
//			.ReturnsAsync(existingProfile);

//		TutorProfile? capturedEntity = null;
//		_tutorProfileRepositoryMock
//			.Setup(r => r.AddAsync(It.IsAny<TutorProfile>()))
//			.Returns(Task.CompletedTask)
//			.Callback<TutorProfile>(tp =>
//			{
//				tp.Id = 1;
//				capturedEntity = tp;
//			});

//		_userProfileServiceMock
//			.Setup(s => s.UpdateAsync(It.IsAny<UserProfileUpdateRequest>()))
//			.ReturnsAsync((UserProfileDto?)null);

//		// Act & Assert
//		if (shouldSucceed)
//		{
//			var result = await _service.CreateAsync(request);

//			Assert.That(result, Is.Not.Null);
//			Assert.That(result.UserEmail, Is.EqualTo(email));
//			Assert.That(result.Status, Is.EqualTo(TutorStatus.Pending));
//			_tutorProfileRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TutorProfile>()), Times.Once);
//		}
//		else
//		{
//			var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.CreateAsync(request));
//			Assert.That(exception, Is.InstanceOf(expectedExceptionType!));
//		}
//	}

//	/// <summary>
//	/// Tạo các test cases cho UpdateAsync - bao phủ các kịch bản: valid update, entity not found, status change valid/invalid
//	/// </summary>
//	private static IEnumerable<TestCaseData> UpdateAsyncTestCases()
//	{
//		// Valid update
//		yield return new TestCaseData(
//			new TutorProfileUpdateRequest
//			{
//				Id = 1,
//				UserEmail = "abc@gmail.com",
//				Bio = "Updated Bio",
//				TeachingExp = "10 years",
//				TeachingModes = TeachingMode.Hybrid
//			},
//			FakeDataFactory.CreateFakeTutorProfile("abc@gmail.com"),
//			true,
//			null
//		).SetName("UpdateAsync_ValidRequest_ReturnsUpdatedDto");

//		// Entity not found
//		yield return new TestCaseData(
//			new TutorProfileUpdateRequest
//			{
//				Id = 999,
//				UserEmail = "test@gmail.com",
//				TeachingModes = TeachingMode.Online
//			},
//			null,
//			false,
//			typeof(InvalidOperationException)
//		).SetName("UpdateAsync_EntityNotFound_ThrowsInvalidOperationException");

//		// Status change from Pending to Approved
//		var pendingProfile = FakeDataFactory.CreateFakeTutorProfile("abc@gmail.com");
//		pendingProfile.Id = 1;
//		pendingProfile.Status = (int)TutorStatus.Pending;
//		yield return new TestCaseData(
//			new TutorProfileUpdateRequest
//			{
//				Id = 1,
//				UserEmail = "abc@gmail.com",
//				TeachingModes = TeachingMode.Online,
//				Status = TutorStatus.Approved
//			},
//			pendingProfile,
//			true,
//			null
//		).SetName("UpdateAsync_PendingToApproved_UpdatesStatus");

//		// Status change from Pending to Rejected
//		var pendingProfile2 = FakeDataFactory.CreateFakeTutorProfile("abc@gmail.com");
//		pendingProfile2.Id = 1;
//		pendingProfile2.Status = (int)TutorStatus.Pending;
//		yield return new TestCaseData(
//			new TutorProfileUpdateRequest
//			{
//				Id = 1,
//				UserEmail = "abc@gmail.com",
//				TeachingModes = TeachingMode.Online,
//				Status = TutorStatus.Rejected
//			},
//			pendingProfile2,
//			true,
//			null
//		).SetName("UpdateAsync_PendingToRejected_UpdatesStatus");

//		// Status change from Approved (not allowed)
//		var approvedProfile = FakeDataFactory.CreateFakeTutorProfile("abc@gmail.com");
//		approvedProfile.Id = 1;
//		approvedProfile.Status = (int)TutorStatus.Approved;
//		yield return new TestCaseData(
//			new TutorProfileUpdateRequest
//			{
//				Id = 1,
//				UserEmail = "abc@gmail.com",
//				TeachingModes = TeachingMode.Online,
//				Status = TutorStatus.Rejected
//			},
//			approvedProfile,
//			false,
//			typeof(InvalidOperationException)
//		).SetName("UpdateAsync_StatusNotPending_ThrowsInvalidOperationException");
//	}

//	/// <summary>
//	/// Test UpdateAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
//	/// </summary>
//	[Test]
//	[TestCaseSource(nameof(UpdateAsyncTestCases))]
//	public async Task UpdateAsync_WithVariousScenarios_HandlesCorrectly(
//		TutorProfileUpdateRequest request,
//		TutorProfile? existingEntity,
//		bool shouldSucceed,
//		Type? expectedExceptionType)
//	{
//		// Arrange
//		if (existingEntity != null)
//		{
//			_tutorProfileRepositoryMock
//				.Setup(r => r.GetByIdFullAsync(request.Id))
//				.ReturnsAsync(existingEntity);
//		}
//		else
//		{
//			_tutorProfileRepositoryMock
//				.Setup(r => r.GetByIdFullAsync(request.Id))
//				.ReturnsAsync((TutorProfile?)null);
//		}

//		_tutorProfileRepositoryMock
//			.Setup(r => r.UpdateAsync(It.IsAny<TutorProfile>()))
//			.Returns(Task.CompletedTask);

//		_userProfileServiceMock
//			.Setup(s => s.UpdateAsync(It.IsAny<UserProfileUpdateRequest>()))
//			.ReturnsAsync((UserProfileDto?)null);

//		// Act & Assert
//		if (shouldSucceed)
//		{
//			var result = await _service.UpdateAsync(request);

//			Assert.That(result, Is.Not.Null);
//			Assert.That(result.Id, Is.EqualTo(request.Id));

//			if (request.Status.HasValue && existingEntity != null)
//			{
//				Assert.That((TutorStatus)existingEntity.Status, Is.EqualTo(request.Status.Value));
//			}

//			_tutorProfileRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TutorProfile>()), Times.Once);
//		}
//		else
//		{
//			var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateAsync(request));
//			Assert.That(exception, Is.InstanceOf(expectedExceptionType!));
//		}
//	}

//	/// <summary>
//	/// Test DeleteAsync với các ID khác nhau - gọi repository để xóa
//	/// </summary>
//	[Test]
//	[TestCase(1)]
//	[TestCase(10)]
//	[TestCase(100)]
//	public async Task DeleteAsync_WithDifferentIds_CallsRepository(int id)
//	{
//		// Arrange
//		_tutorProfileRepositoryMock
//			.Setup(r => r.RemoveByIdAsync(id))
//			.Returns(Task.CompletedTask);

//		// Act
//		await _service.DeleteAsync(id);

//		// Assert
//		_tutorProfileRepositoryMock.Verify(r => r.RemoveByIdAsync(id), Times.Once);
//	}

//	/// <summary>
//	/// Test DeleteAsync với ID không hợp lệ - ném ArgumentException
//	/// </summary>
//	[Test]
//	[TestCase(0)]
//	[TestCase(-1)]
//	public void DeleteAsync_WithInvalidId_ThrowsArgumentException(int id)
//	{
//		// Act & Assert
//		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.DeleteAsync(id));
//		Assert.That(exception.Message, Does.Contain("ID must be greater than 0"));
//	}

//	/// <summary>
//	/// Tạo các test cases cho VerifyAsync - bao phủ các kịch bản: valid, entity not found, status not pending, invalid ID, empty verifiedBy
//	/// </summary>
//	private static IEnumerable<TestCaseData> VerifyAsyncTestCases()
//	{
//		// Valid verification
//		var pendingProfile = FakeDataFactory.CreateFakeTutorProfile("abc@gmail.com");
//		pendingProfile.Id = 1;
//		pendingProfile.Status = (int)TutorStatus.Pending;
//		yield return new TestCaseData(
//			1,
//			"admin@example.com",
//			pendingProfile,
//			true,
//			null
//		).SetName("VerifyAsync_ValidRequest_UpdatesStatusToApproved");

//		// Entity not found
//		yield return new TestCaseData(
//			999,
//			"admin@example.com",
//			null,
//			false,
//			typeof(InvalidOperationException)
//		).SetName("VerifyAsync_EntityNotFound_ThrowsInvalidOperationException");

//		// Status not pending
//		var approvedProfile = FakeDataFactory.CreateFakeTutorProfile("abc@gmail.com");
//		approvedProfile.Id = 1;
//		approvedProfile.Status = (int)TutorStatus.Approved;
//		yield return new TestCaseData(
//			1,
//			"admin@example.com",
//			approvedProfile,
//			false,
//			typeof(InvalidOperationException)
//		).SetName("VerifyAsync_StatusNotPending_ThrowsInvalidOperationException");

//		// Empty verifiedBy
//		var pendingProfile2 = FakeDataFactory.CreateFakeTutorProfile("abc@gmail.com");
//		pendingProfile2.Id = 1;
//		pendingProfile2.Status = (int)TutorStatus.Pending;
//		yield return new TestCaseData(
//			1,
//			"",
//			pendingProfile2,
//			false,
//			typeof(InvalidOperationException)
//		).SetName("VerifyAsync_EmptyVerifiedBy_ThrowsInvalidOperationException");
//	}

//	/// <summary>
//	/// Test VerifyAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
//	/// </summary>
//	//[Test]
//	//[TestCaseSource(nameof(VerifyAsyncTestCases))]
//	//public async Task VerifyAsync_WithVariousScenarios_HandlesCorrectly(
//	//	int id,
//	//	string verifiedBy,
//	//	TutorProfile? existingEntity,
//	//	bool shouldSucceed,
//	//	Type? expectedExceptionType)
//	//{
//	//	// Arrange
//	//	if (existingEntity != null)
//	//	{
//	//		_tutorProfileRepositoryMock
//	//			.Setup(r => r.GetByIdFullAsync(id))
//	//			.ReturnsAsync(existingEntity);
//	//	}
//	//	else
//	//	{
//	//		_tutorProfileRepositoryMock
//	//			.Setup(r => r.GetByIdFullAsync(id))
//	//			.ReturnsAsync((TutorProfile?)null);
//	//	}

//	//	_tutorProfileRepositoryMock
//	//		.Setup(r => r.UpdateAsync(It.IsAny<TutorProfile>()))
//	//		.Returns(Task.CompletedTask);

//	//	// Act & Assert
//	//	if (shouldSucceed)
//	//	{
//	//		var result = await _service.VerifyAsync(id, verifiedBy);

//	//		Assert.That(result, Is.Not.Null);
//	//		Assert.That((TutorStatus)existingEntity!.Status, Is.EqualTo(TutorStatus.Approved));
//	//		Assert.That(existingEntity.VerifiedBy, Is.EqualTo(verifiedBy));
//	//		Assert.That(existingEntity.VerifiedAt, Is.Not.Null);
//	//		_tutorProfileRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TutorProfile>()), Times.Once);
//	//	}
//	//	else
//	//	{
//	//		var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.VerifyAsync(id, verifiedBy));
//	//		Assert.That(exception, Is.InstanceOf(expectedExceptionType!));
//	//	}
//	//}

//	/// <summary>
//	/// Test VerifyAsync với ID không hợp lệ - ném InvalidOperationException
//	/// </summary>
	
//	//[Test]
//	//[TestCase(0)]
//	//[TestCase(-1)]
//	//public void VerifyAsync_WithInvalidId_ThrowsInvalidOperationException(int id)
//	//{
//	//	// Act & Assert
//	//	var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.VerifyAsync(id, "admin@example.com"));
//	//	Assert.That(exception.Message, Does.Contain("ID must be greater than 0"));
//	//}
//}
