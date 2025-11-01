using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.TutorProfile;
using EduMatch.BusinessLogicLayer.Requests.User;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.Tests.Common;
using Moq;

namespace EduMatch.Tests;


public class TutorProfileServiceTests
{

	private Mock<ITutorProfileRepository> _repositoryMock;
	private Mock<ICloudMediaService> _cloudMediaMock;
	private Mock<IUserService> _userServiceMock;
	private Mock<IUserProfileService> _userProfileServiceMock;
	private IMapper _mapper;
	private CurrentUserService _currentUserService;
	private TutorProfileService _service;

	[SetUp]
	public void Setup()
	{
		_repositoryMock = new Mock<ITutorProfileRepository>();
		_cloudMediaMock = new Mock<ICloudMediaService>();
		_userServiceMock = new Mock<IUserService>();
		_userProfileServiceMock = new Mock<IUserProfileService>();

		var config = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<MappingProfile>();
		});
		_mapper = config.CreateMapper();

		_currentUserService = new CurrentUserServiceFake("abc@gmail.com");



		_service = new TutorProfileService(
			_repositoryMock.Object,
			_mapper,
			_cloudMediaMock.Object,
			_currentUserService,
			_userServiceMock.Object,
			_userProfileServiceMock.Object
		);

	}


	/// <summary>
	/// Test GetByEmailFullAsync returns mapped DTO when tutor profile exists.
	/// </summary>
	[Test]
	public async Task GetByEmailFullAsync_WhenExists_ReturnsMappedDto()
	{
		var email = "abc@gmail.com";
		var fakeTutor = FakeDataFactory.CreateFakeTutorProfile(email);

		_repositoryMock
			.Setup(r => r.GetByEmailFullAsync(email))
			.ReturnsAsync(fakeTutor);

		var result = await _service.GetByEmailFullAsync(email);

		Assert.That(result, Is.Not.Null);
		Assert.That(result.UserEmail, Is.EqualTo(email));
		Assert.That(result.Province.Name, Is.EqualTo("Hà Nội"));
		Assert.That(result.TutorSubjects.First().Subject.SubjectName, Is.EqualTo("Math"));
	}

	/// <summary>
	/// Test GetByEmailFullAsync returns null when tutor profile does not exist.
	/// </summary>
	[Test]
	public async Task GetByEmailFullAsync_WhenNotExists_ReturnsNull()
	{
		var email = "nonexistent@gmail.com";

		_repositoryMock
			.Setup(r => r.GetByEmailFullAsync(email))
			.ReturnsAsync((TutorProfile?)null);

		var result = await _service.GetByEmailFullAsync(email);

		Assert.That(result, Is.Null);
	}

	/// <summary>
	/// Test GetByEmailFullAsync throws ArgumentException when email is empty.
	/// </summary>
	[Test]
	public void GetByEmailFullAsync_WhenEmailIsEmpty_ThrowsArgumentException()
	{
		Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetByEmailFullAsync(""));
	}

	/// <summary>
	/// Test GetByIdFullAsync returns mapped DTO when tutor profile exists.
	/// </summary>
	[Test]
	public async Task GetByIdFullAsync_WhenExists_ReturnsMappedDto()
	{
		var id = 1;
		var email = "abc@gmail.com";
		var fakeTutor = FakeDataFactory.CreateFakeTutorProfile(email);
		fakeTutor.Id = id;

		_repositoryMock
			.Setup(r => r.GetByIdFullAsync(id))
			.ReturnsAsync(fakeTutor);

		var result = await _service.GetByIdFullAsync(id);

		Assert.That(result, Is.Not.Null);
		Assert.That(result.Id, Is.EqualTo(id));
		Assert.That(result.UserEmail, Is.EqualTo(email));
	}

	/// <summary>
	/// Test GetByIdFullAsync returns null when tutor profile does not exist.
	/// </summary>
	[Test]
	public async Task GetByIdFullAsync_WhenNotExists_ReturnsNull()
	{
		var id = 999;

		_repositoryMock
			.Setup(r => r.GetByIdFullAsync(id))
			.ReturnsAsync((TutorProfile?)null);

		var result = await _service.GetByIdFullAsync(id);

		Assert.That(result, Is.Null);
	}

	/// <summary>
	/// Test GetAllFullAsync returns list of mapped DTOs.
	/// </summary>
	[Test]
	public async Task GetAllFullAsync_ReturnsListOfDtos()
	{
		var email1 = "abc@gmail.com";
		var email2 = "xyz@gmail.com";
		var tutor1 = FakeDataFactory.CreateFakeTutorProfile(email1);
		tutor1.Id = 1;
		var tutor2 = FakeDataFactory.CreateFakeTutorProfile(email2);
		tutor2.Id = 2;
		var fakeTutors = new List<TutorProfile> { tutor1, tutor2 };

		_repositoryMock
			.Setup(r => r.GetAllFullAsync())
			.ReturnsAsync(fakeTutors);

		var result = await _service.GetAllFullAsync();

		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(2));
		Assert.That(result.Select(r => r.UserEmail), Contains.Item(email1));
		Assert.That(result.Select(r => r.UserEmail), Contains.Item(email2));
	}

	/// <summary>
	/// Test CreateAsync successfully creates tutor profile and returns DTO.
	/// </summary>
	[Test]
	public async Task CreateAsync_WhenValid_ReturnsCreatedDto()
	{
		var email = "abc@gmail.com";
		var request = new TutorProfileCreateRequest
		{
			UserEmail = email,
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
		};

		_repositoryMock
			.Setup(r => r.GetByEmailFullAsync(email))
			.ReturnsAsync((TutorProfile?)null);

		_repositoryMock
			.Setup(r => r.AddAsync(It.IsAny<TutorProfile>()))
			.Returns(Task.CompletedTask)
			.Callback<TutorProfile>(tp => tp.Id = 1);

		_userProfileServiceMock
			.Setup(s => s.UpdateAsync(It.IsAny<UserProfileUpdateRequest>()))
			.ReturnsAsync((UserProfileDto?)null);

		var result = await _service.CreateAsync(request);

		Assert.That(result, Is.Not.Null);
		Assert.That(result.UserEmail, Is.EqualTo(email));
		Assert.That(result.Status, Is.EqualTo(TutorStatus.Pending));
	}

	/// <summary>
	/// Test CreateAsync throws InvalidOperationException when profile already exists.
	/// </summary>
	[Test]
	public void CreateAsync_WhenProfileAlreadyExists_ThrowsInvalidOperationException()
	{
		var email = "abc@gmail.com";
		var request = new TutorProfileCreateRequest
		{
			UserEmail = email,
			VideoIntroUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
			AvatarUrl = "https://example.com/avatar.jpg",
			TeachingModes = TeachingMode.Online
		};

		var existingProfile = FakeDataFactory.CreateFakeTutorProfile(email);

		_repositoryMock
			.Setup(r => r.GetByEmailFullAsync(email))
			.ReturnsAsync(existingProfile);

		Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.CreateAsync(request));
	}

	/// <summary>
	/// Test CreateAsync throws InvalidOperationException when VideoIntroUrl is missing.
	/// </summary>
	[Test]
	public void CreateAsync_WhenVideoIntroUrlMissing_ThrowsInvalidOperationException()
	{
		var email = "abc@gmail.com";
		var request = new TutorProfileCreateRequest
		{
			UserEmail = email,
			AvatarUrl = "https://example.com/avatar.jpg",
			TeachingModes = TeachingMode.Online
		};

		_repositoryMock
			.Setup(r => r.GetByEmailFullAsync(email))
			.ReturnsAsync((TutorProfile?)null);

		Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.CreateAsync(request));
	}

	/// <summary>
	/// Test UpdateAsync successfully updates tutor profile and returns DTO.
	/// </summary>
	[Test]
	public async Task UpdateAsync_WhenValid_ReturnsUpdatedDto()
	{
		var id = 1;
		var email = "abc@gmail.com";
		var existingProfile = FakeDataFactory.CreateFakeTutorProfile(email);
		existingProfile.Id = id;
		existingProfile.Status = (int)TutorStatus.Pending;

		var request = new TutorProfileUpdateRequest
		{
			Id = id,
			UserEmail = email,
			Bio = "Updated Bio",
			TeachingExp = "10 years",
			TeachingModes = TeachingMode.Hybrid
		};

		_repositoryMock
			.Setup(r => r.GetByIdFullAsync(id))
			.ReturnsAsync(existingProfile);

		_repositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<TutorProfile>()))
			.Returns(Task.CompletedTask);

		_userProfileServiceMock
			.Setup(s => s.UpdateAsync(It.IsAny<UserProfileUpdateRequest>()))
			.ReturnsAsync((UserProfileDto?)null);

		var result = await _service.UpdateAsync(request);

		Assert.That(result, Is.Not.Null);
		Assert.That(result.Bio, Is.EqualTo("Updated Bio"));
		Assert.That(result.Id, Is.EqualTo(id));
	}

	/// <summary>
	/// Test UpdateAsync throws InvalidOperationException when tutor profile not found.
	/// </summary>
	[Test]
	public void UpdateAsync_WhenNotFound_ThrowsInvalidOperationException()
	{
		var id = 999;
		var request = new TutorProfileUpdateRequest
		{
			Id = id,
			UserEmail = "test@gmail.com",
			TeachingModes = TeachingMode.Online
		};

		_repositoryMock
			.Setup(r => r.GetByIdFullAsync(id))
			.ReturnsAsync((TutorProfile?)null);

		Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateAsync(request));
	}

	/// <summary>
	/// Test UpdateAsync allows status change from Pending to Approved.
	/// </summary>
	[Test]
	public async Task UpdateAsync_WhenStatusPendingToApproved_UpdatesStatus()
	{
		var id = 1;
		var email = "abc@gmail.com";
		var existingProfile = FakeDataFactory.CreateFakeTutorProfile(email);
		existingProfile.Id = id;
		existingProfile.Status = (int)TutorStatus.Pending;

		var request = new TutorProfileUpdateRequest
		{
			Id = id,
			UserEmail = email,
			TeachingModes = TeachingMode.Online,
			Status = TutorStatus.Approved
		};

		_repositoryMock
			.Setup(r => r.GetByIdFullAsync(id))
			.ReturnsAsync(existingProfile);

		_repositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<TutorProfile>()))
			.Returns(Task.CompletedTask);

		_userProfileServiceMock
			.Setup(s => s.UpdateAsync(It.IsAny<UserProfileUpdateRequest>()))
			.ReturnsAsync((UserProfileDto?)null);

		var result = await _service.UpdateAsync(request);

		Assert.That(result, Is.Not.Null);
		Assert.That((TutorStatus)existingProfile.Status, Is.EqualTo(TutorStatus.Approved));
	}

	/// <summary>
	/// Test UpdateAsync throws InvalidOperationException when status is not Pending.
	/// </summary>
	[Test]
	public void UpdateAsync_WhenStatusNotPending_ThrowsInvalidOperationException()
	{
		var id = 1;
		var email = "abc@gmail.com";
		var existingProfile = FakeDataFactory.CreateFakeTutorProfile(email);
		existingProfile.Id = id;
		existingProfile.Status = (int)TutorStatus.Approved;

		var request = new TutorProfileUpdateRequest
		{
			Id = id,
			UserEmail = email,
			TeachingModes = TeachingMode.Online,
			Status = TutorStatus.Rejected
		};

		_repositoryMock
			.Setup(r => r.GetByIdFullAsync(id))
			.ReturnsAsync(existingProfile);

		Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateAsync(request));
	}

	/// <summary>
	/// Test DeleteAsync calls repository to remove tutor profile.
	/// </summary>
	[Test]
	public async Task DeleteAsync_WhenValid_CallsRepository()
	{
		var id = 1;

		_repositoryMock
			.Setup(r => r.RemoveByIdAsync(id))
			.Returns(Task.CompletedTask);

		await _service.DeleteAsync(id);

		_repositoryMock.Verify(r => r.RemoveByIdAsync(id), Times.Once);
	}

	/// <summary>
	/// Test VerifyAsync updates status to Approved and sets verification info.
	/// </summary>
	[Test]
	public async Task VerifyAsync_WhenValid_UpdatesStatusToApproved()
	{
		var id = 1;
		var email = "abc@gmail.com";
		var verifiedBy = "admin@example.com";
		var existingProfile = FakeDataFactory.CreateFakeTutorProfile(email);
		existingProfile.Id = id;
		existingProfile.Status = (int)TutorStatus.Pending;

		_repositoryMock
			.Setup(r => r.GetByIdFullAsync(id))
			.ReturnsAsync(existingProfile);

		_repositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<TutorProfile>()))
			.Returns(Task.CompletedTask);

		var result = await _service.VerifyAsync(id, verifiedBy);

		Assert.That(result, Is.Not.Null);
		Assert.That((TutorStatus)existingProfile.Status, Is.EqualTo(TutorStatus.Approved));
		Assert.That(existingProfile.VerifiedBy, Is.EqualTo(verifiedBy));
		Assert.That(existingProfile.VerifiedAt, Is.Not.Null);
	}

	/// <summary>
	/// Test VerifyAsync throws InvalidOperationException when tutor profile not found.
	/// </summary>
	[Test]
	public void VerifyAsync_WhenNotFound_ThrowsInvalidOperationException()
	{
		var id = 999;
		var verifiedBy = "admin@example.com";

		_repositoryMock
			.Setup(r => r.GetByIdFullAsync(id))
			.ReturnsAsync((TutorProfile?)null);

		Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.VerifyAsync(id, verifiedBy));
	}

	/// <summary>
	/// Test VerifyAsync throws InvalidOperationException when status is not Pending.
	/// </summary>
	[Test]
	public void VerifyAsync_WhenNotPending_ThrowsInvalidOperationException()
	{
		var id = 1;
		var email = "abc@gmail.com";
		var verifiedBy = "admin@example.com";
		var existingProfile = FakeDataFactory.CreateFakeTutorProfile(email);
		existingProfile.Id = id;
		existingProfile.Status = (int)TutorStatus.Approved;

		_repositoryMock
			.Setup(r => r.GetByIdFullAsync(id))
			.ReturnsAsync(existingProfile);

		Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.VerifyAsync(id, verifiedBy));
	}

	/// <summary>
	/// Test VerifyAsync throws InvalidOperationException when verifiedBy is empty.
	/// </summary>
	[Test]
	public void VerifyAsync_WhenVerifiedByIsEmpty_ThrowsInvalidOperationException()
	{
		var id = 1;
		var email = "abc@gmail.com";
		var existingProfile = FakeDataFactory.CreateFakeTutorProfile(email);
		existingProfile.Id = id;
		existingProfile.Status = (int)TutorStatus.Pending;

		_repositoryMock
			.Setup(r => r.GetByIdFullAsync(id))
			.ReturnsAsync(existingProfile);

		Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.VerifyAsync(id, ""));
	}

}
