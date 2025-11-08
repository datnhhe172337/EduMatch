using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.TutorEducation;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.Tests.Common;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.Tests;


public class TutorEducationServiceTests
{
	private Mock<ITutorEducationRepository> _tutorEducationRepositoryMock;
	private Mock<IEducationInstitutionRepository> _educationInstitutionRepositoryMock;
	private Mock<ITutorProfileRepository> _tutorProfileRepositoryMock;
	private Mock<ICloudMediaService> _cloudMediaServiceMock;
	private IMapper _mapper;
	private CurrentUserService _currentUserService;
	private TutorEducationService _service;

	/// <summary>
	/// Khởi tạo các mock objects và service trước mỗi test
	/// </summary>
	[SetUp]
	public void Setup()
	{
		_tutorEducationRepositoryMock = new Mock<ITutorEducationRepository>();
		_educationInstitutionRepositoryMock = new Mock<IEducationInstitutionRepository>();
		_tutorProfileRepositoryMock = new Mock<ITutorProfileRepository>();
		_cloudMediaServiceMock = new Mock<ICloudMediaService>();

		var config = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<MappingProfile>();
		});
		_mapper = config.CreateMapper();

		_currentUserService = new CurrentUserServiceFake("test@gmail.com");

		_service = new TutorEducationService(
			_tutorEducationRepositoryMock.Object,
			_mapper,
			_cloudMediaServiceMock.Object,
			_currentUserService,
			_tutorProfileRepositoryMock.Object,
			_educationInstitutionRepositoryMock.Object
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
		TutorEducation? entity = shouldExist ? FakeDataFactory.CreateFakeTutorEducation(id) : null;

		_tutorEducationRepositoryMock
			.Setup(r => r.GetByIdFullAsync(id))
			.ReturnsAsync(entity);

		// Act
		var result = await _service.GetByIdFullAsync(id);

		// Assert
		if (shouldExist)
		{
			Assert.That(result, Is.Not.Null);
			Assert.That(result!.Id, Is.EqualTo(id));
			Assert.That(result.Verified, Is.EqualTo(VerifyStatus.Verified));
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
	/// Test GetByTutorIdFullAsync với các TutorId khác nhau - trả về DTO khi tồn tại, null khi không tồn tại
	/// </summary>
	[Test]
	[TestCase(1, true)]
	[TestCase(999, false)]
	public async Task GetByTutorIdFullAsync_WithDifferentTutorIds_ReturnsExpectedResult(int tutorId, bool shouldExist)
	{
		// Arrange
		TutorEducation? entity = shouldExist ? FakeDataFactory.CreateFakeTutorEducation(1, tutorId) : null;

		_tutorEducationRepositoryMock
			.Setup(r => r.GetByTutorIdFullAsync(tutorId))
			.ReturnsAsync(entity);

		// Act
		var result = await _service.GetByTutorIdFullAsync(tutorId);

		// Assert
		if (shouldExist)
		{
			Assert.That(result, Is.Not.Null);
			Assert.That(result!.TutorId, Is.EqualTo(tutorId));
		}
		else
		{
			Assert.That(result, Is.Null);
		}
	}

	/// <summary>
	/// Test GetByTutorIdFullAsync với TutorId không hợp lệ - ném ArgumentException
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(-1)]
	public void GetByTutorIdFullAsync_WithInvalidTutorId_ThrowsArgumentException(int tutorId)
	{
		// Act & Assert
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetByTutorIdFullAsync(tutorId));
		Assert.That(exception.Message, Does.Contain("TutorId must be greater than 0"));
	}

	/// <summary>
	/// Test GetByTutorIdAsync với số lượng khác nhau - trả về danh sách DTOs đúng số lượng
	/// </summary>
	[Test]
	[TestCase(1, 0)]
	[TestCase(1, 1)]
	[TestCase(1, 3)]
	public async Task GetByTutorIdAsync_WithDifferentCounts_ReturnsExpectedList(int tutorId, int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorEducation(i, tutorId))
			.ToList();

		_tutorEducationRepositoryMock
			.Setup(r => r.GetByTutorIdAsync(tutorId))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetByTutorIdAsync(tutorId);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
		if (count > 0)
		{
			Assert.That(result.All(r => r.TutorId == tutorId), Is.True);
		}
	}

	/// <summary>
	/// Test GetByTutorIdAsync với TutorId không hợp lệ - ném ArgumentException
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(-1)]
	public void GetByTutorIdAsync_WithInvalidTutorId_ThrowsArgumentException(int tutorId)
	{
		// Act & Assert
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetByTutorIdAsync(tutorId));
		Assert.That(exception.Message, Does.Contain("TutorId must be greater than 0"));
	}

	/// <summary>
	/// Test GetByInstitutionIdAsync với số lượng khác nhau - trả về danh sách DTOs đúng số lượng
	/// </summary>
	[Test]
	[TestCase(1, 0)]
	[TestCase(1, 1)]
	[TestCase(1, 5)]
	public async Task GetByInstitutionIdAsync_WithDifferentCounts_ReturnsExpectedList(int institutionId, int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorEducation(i, institutionId: institutionId))
			.ToList();

		_tutorEducationRepositoryMock
			.Setup(r => r.GetByInstitutionIdAsync(institutionId))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetByInstitutionIdAsync(institutionId);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
	}

	/// <summary>
	/// Test GetByInstitutionIdAsync với InstitutionId không hợp lệ - ném ArgumentException
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(-1)]
	public void GetByInstitutionIdAsync_WithInvalidInstitutionId_ThrowsArgumentException(int institutionId)
	{
		// Act & Assert
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetByInstitutionIdAsync(institutionId));
		Assert.That(exception.Message, Does.Contain("InstitutionId must be greater than 0"));
	}

	/// <summary>
	/// Test GetByVerifiedStatusAsync với các trạng thái xác thực khác nhau - trả về danh sách đúng trạng thái
	/// </summary>
	[Test]
	[TestCase(VerifyStatus.Pending, 2)]
	[TestCase(VerifyStatus.Verified, 3)]
	[TestCase(VerifyStatus.Rejected, 1)]
	public async Task GetByVerifiedStatusAsync_WithDifferentStatuses_ReturnsExpectedList(VerifyStatus status, int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorEducation(i, verified: status))
			.ToList();

		_tutorEducationRepositoryMock
			.Setup(r => r.GetByVerifiedStatusAsync(status))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetByVerifiedStatusAsync(status);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
		Assert.That(result.All(r => r.Verified == status), Is.True);
	}



	/// <summary>
	/// Test GetPendingVerificationsAsync với số lượng khác nhau - trả về danh sách đang chờ xác thực
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(5)]
	public async Task GetPendingVerificationsAsync_WithDifferentCounts_ReturnsExpectedList(int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorEducation(i, verified: VerifyStatus.Pending))
			.ToList();

		_tutorEducationRepositoryMock
			.Setup(r => r.GetPendingVerificationsAsync())
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetPendingVerificationsAsync();

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
		Assert.That(result.All(r => r.Verified == VerifyStatus.Pending), Is.True);
	}



	/// <summary>
	/// Test GetAllFullAsync với số lượng khác nhau - trả về tất cả tutor education với đầy đủ thông tin
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(10)]
	public async Task GetAllFullAsync_WithDifferentCounts_ReturnsExpectedList(int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorEducation(i))
			.ToList();

		_tutorEducationRepositoryMock
			.Setup(r => r.GetAllFullAsync())
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetAllFullAsync();

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
	}


	/// <summary>
	/// Tạo các test cases cho CreateAsync - bao phủ các kịch bản: valid, tutor not found, institution not found, certificate URL invalid, repository exception
	/// </summary>
	private static IEnumerable<TestCaseData> CreateAsyncTestCases()
	{
		// Valid case
		yield return new TestCaseData(
			new TutorEducationCreateRequest
			{
				TutorId = 1,
				InstitutionId = 1,
				IssueDate = new DateTime(2020, 1, 1),
				CertificateEducationUrl = "https://example.com/certificate.jpg"
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			FakeDataFactory.CreateFakeEducationInstitution(1),
			true,
			null
		).SetName("CreateAsync_ValidRequest_ReturnsCreatedDto");

		// Tutor not found
		yield return new TestCaseData(
			new TutorEducationCreateRequest
			{
				TutorId = 999,
				InstitutionId = 1,
				IssueDate = new DateTime(2020, 1, 1),
				CertificateEducationUrl = "https://example.com/certificate.jpg"
			},
			null,
			FakeDataFactory.CreateFakeEducationInstitution(1),
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_TutorNotFound_ThrowsArgumentException");

		// Institution not found
		yield return new TestCaseData(
			new TutorEducationCreateRequest
			{
				TutorId = 1,
				InstitutionId = 999,
				IssueDate = new DateTime(2020, 1, 1),
				CertificateEducationUrl = "https://example.com/certificate.jpg"
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			null,
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_InstitutionNotFound_ThrowsArgumentException");

		// Certificate URL is empty
		yield return new TestCaseData(
			new TutorEducationCreateRequest
			{
				TutorId = 1,
				InstitutionId = 1,
				IssueDate = new DateTime(2020, 1, 1),
				CertificateEducationUrl = ""
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			FakeDataFactory.CreateFakeEducationInstitution(1),
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_CertificateUrlEmpty_ThrowsArgumentException");

		// Certificate URL is null
		yield return new TestCaseData(
			new TutorEducationCreateRequest
			{
				TutorId = 1,
				InstitutionId = 1,
				IssueDate = new DateTime(2020, 1, 1),
				CertificateEducationUrl = null
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			FakeDataFactory.CreateFakeEducationInstitution(1),
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_CertificateUrlNull_ThrowsArgumentException");

		// Certificate URL is whitespace
		yield return new TestCaseData(
			new TutorEducationCreateRequest
			{
				TutorId = 1,
				InstitutionId = 1,
				IssueDate = new DateTime(2020, 1, 1),
				CertificateEducationUrl = "   "
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			FakeDataFactory.CreateFakeEducationInstitution(1),
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_CertificateUrlWhitespace_ThrowsArgumentException");
	}

	/// <summary>
	/// Test CreateAsync khi repository throw exception - ném InvalidOperationException
	/// </summary>
	[Test]
	public void CreateAsync_WhenRepositoryThrowsException_ThrowsInvalidOperationException()
	{
		// Arrange
		var request = new TutorEducationCreateRequest
		{
			TutorId = 1,
			InstitutionId = 1,
			CertificateEducationUrl = "https://example.com/certificate.jpg"
		};

		_tutorProfileRepositoryMock
			.Setup(r => r.GetByIdFullAsync(request.TutorId))
			.ThrowsAsync(new Exception("Database error"));

		// Act & Assert
		var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.CreateAsync(request));
		Assert.That(exception.Message, Does.Contain("Failed to create tutor education"));
	}

	/// <summary>
	/// Test CreateAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
	/// </summary>
	[Test]
	[TestCaseSource(nameof(CreateAsyncTestCases))]
	public async Task CreateAsync_WithVariousScenarios_HandlesCorrectly(
		TutorEducationCreateRequest request,
		TutorProfile? tutor,
		EducationInstitution? institution,
		bool shouldSucceed,
		Type? expectedExceptionType)
	{
		// Arrange
		_tutorProfileRepositoryMock
			.Setup(r => r.GetByIdFullAsync(request.TutorId))
			.ReturnsAsync(tutor);

		_educationInstitutionRepositoryMock
			.Setup(r => r.GetByIdAsync(request.InstitutionId))
			.ReturnsAsync(institution);

		TutorEducation? capturedEntity = null;
		_tutorEducationRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<TutorEducation>()))
			.Returns(Task.CompletedTask)
			.Callback<TutorEducation>(te =>
			{
				te.Id = 1;
				capturedEntity = te;
			});

		// Act & Assert
		if (shouldSucceed)
		{
			var result = await _service.CreateAsync(request);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.TutorId, Is.EqualTo(request.TutorId));
			Assert.That(result.InstitutionId, Is.EqualTo(request.InstitutionId));
			Assert.That(result.CertificateUrl, Is.EqualTo(request.CertificateEducationUrl));
			Assert.That(result.Verified, Is.EqualTo(VerifyStatus.Pending));
			Assert.That(result.IssueDate, Is.EqualTo(request.IssueDate));

			_tutorEducationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TutorEducation>()), Times.Once);
		}
		else
		{
			var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.CreateAsync(request));
			Assert.That(exception, Is.InstanceOf(expectedExceptionType!));
		}
	}


	/// <summary>
	/// Tạo các test cases cho UpdateAsync - bao phủ các kịch bản: valid update, entity not found, institution not found, rejected without reason, verified clears reason, repository exception
	/// </summary>
	private static IEnumerable<TestCaseData> UpdateAsyncTestCases()
	{
		// Valid update
		yield return new TestCaseData(
			new TutorEducationUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				InstitutionId = 1,
				IssueDate = new DateTime(2021, 1, 1),
				CertificateEducationUrl = "https://example.com/new-certificate.jpg"
			},
			FakeDataFactory.CreateFakeTutorEducation(1),
			FakeDataFactory.CreateFakeEducationInstitution(1),
			true,
			null
		).SetName("UpdateAsync_ValidRequest_ReturnsUpdatedDto");

		// Entity not found
		yield return new TestCaseData(
			new TutorEducationUpdateRequest
			{
				Id = 999,
				TutorId = 1,
				InstitutionId = 1,
				IssueDate = new DateTime(2021, 1, 1),
				CertificateEducationUrl = "https://example.com/certificate.jpg"
			},
			null,
			FakeDataFactory.CreateFakeEducationInstitution(1),
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_EntityNotFound_ThrowsInvalidOperationException");

		// Institution not found
		yield return new TestCaseData(
			new TutorEducationUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				InstitutionId = 999,
				IssueDate = new DateTime(2021, 1, 1),
				CertificateEducationUrl = "https://example.com/certificate.jpg"
			},
			FakeDataFactory.CreateFakeTutorEducation(1),
			null,
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_InstitutionNotFound_ThrowsInvalidOperationException");

		// Update with Verified = Rejected but no RejectReason and no existing reason
		yield return new TestCaseData(
			new TutorEducationUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				InstitutionId = 1,
				Verified = VerifyStatus.Rejected,
				RejectReason = null
			},
			FakeDataFactory.CreateFakeTutorEducation(1, verified: VerifyStatus.Pending, rejectReason: null),
			FakeDataFactory.CreateFakeEducationInstitution(1),
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_RejectedWithoutReason_ThrowsInvalidOperationException");

		// Update with Verified = Rejected with empty RejectReason and no existing reason
		yield return new TestCaseData(
			new TutorEducationUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				InstitutionId = 1,
				Verified = VerifyStatus.Rejected,
				RejectReason = ""
			},
			FakeDataFactory.CreateFakeTutorEducation(1, verified: VerifyStatus.Pending, rejectReason: null),
			FakeDataFactory.CreateFakeEducationInstitution(1),
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_RejectedWithEmptyReason_ThrowsInvalidOperationException");

		// Update with Verified = Rejected with valid RejectReason
		yield return new TestCaseData(
			new TutorEducationUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				InstitutionId = 1,
				Verified = VerifyStatus.Rejected,
				RejectReason = "Invalid certificate"
			},
			FakeDataFactory.CreateFakeTutorEducation(1, verified: VerifyStatus.Pending),
			FakeDataFactory.CreateFakeEducationInstitution(1),
			true,
			null
		).SetName("UpdateAsync_RejectedWithValidReason_UpdatesCorrectly");

		// Update with Verified = Verified (clears RejectReason)
		yield return new TestCaseData(
			new TutorEducationUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				InstitutionId = 1,
				Verified = VerifyStatus.Verified
			},
			FakeDataFactory.CreateFakeTutorEducation(1, verified: VerifyStatus.Pending, rejectReason: "Old reason"),
			FakeDataFactory.CreateFakeEducationInstitution(1),
			true,
			null
		).SetName("UpdateAsync_Verified_ClearsRejectReason");

		// Update without changing certificate URL
		yield return new TestCaseData(
			new TutorEducationUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				InstitutionId = 1,
				IssueDate = new DateTime(2021, 1, 1)
			},
			FakeDataFactory.CreateFakeTutorEducation(1, certificateUrl: "https://example.com/old-cert.jpg"),
			FakeDataFactory.CreateFakeEducationInstitution(1),
			true,
			null
		).SetName("UpdateAsync_WithoutCertificateUrl_PreservesOldUrl");

		// Update with Rejected status but existing entity has rejectReason (should use existing)
		yield return new TestCaseData(
			new TutorEducationUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				InstitutionId = 1,
				Verified = VerifyStatus.Rejected,
				RejectReason = null // Will use existing rejectReason
			},
			FakeDataFactory.CreateFakeTutorEducation(1, verified: VerifyStatus.Pending, rejectReason: "Existing reason"),
			FakeDataFactory.CreateFakeEducationInstitution(1),
			true,
			null
		).SetName("UpdateAsync_RejectedWithExistingReason_UsesExistingReason");
	}

	/// <summary>
	/// Test UpdateAsync khi repository throw exception - ném InvalidOperationException
	/// </summary>
	[Test]
	public void UpdateAsync_WhenRepositoryThrowsException_ThrowsInvalidOperationException()
	{
		// Arrange
		var request = new TutorEducationUpdateRequest
		{
			Id = 1,
			TutorId = 1,
			InstitutionId = 1,
			CertificateEducationUrl = "https://example.com/certificate.jpg"
		};

		_tutorEducationRepositoryMock
			.Setup(r => r.GetByIdFullAsync(request.Id))
			.ThrowsAsync(new Exception("Database error"));

		// Act & Assert
		var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateAsync(request));
		Assert.That(exception.Message, Does.Contain("Failed to update tutor education"));
	}

	/// <summary>
	/// Test UpdateAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
	/// </summary>
	[Test]
	[TestCaseSource(nameof(UpdateAsyncTestCases))]
	public async Task UpdateAsync_WithVariousScenarios_HandlesCorrectly(
		TutorEducationUpdateRequest request,
		TutorEducation? existingEntity,
		EducationInstitution? institution,
		bool shouldSucceed,
		Type? expectedExceptionType)
	{
		// Arrange
		_tutorEducationRepositoryMock
			.Setup(r => r.GetByIdFullAsync(request.Id))
			.ReturnsAsync(existingEntity);

		if (institution != null)
		{
			_educationInstitutionRepositoryMock
				.Setup(r => r.GetByIdAsync(request.InstitutionId))
				.ReturnsAsync(institution);
		}
		else
		{
			_educationInstitutionRepositoryMock
				.Setup(r => r.GetByIdAsync(request.InstitutionId))
				.ReturnsAsync((EducationInstitution?)null);
		}

		_tutorEducationRepositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<TutorEducation>()))
			.Returns(Task.CompletedTask);

		// Act & Assert
		if (shouldSucceed)
		{
			var result = await _service.UpdateAsync(request);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.Id, Is.EqualTo(request.Id));
			Assert.That(result.TutorId, Is.EqualTo(request.TutorId));
			Assert.That(result.InstitutionId, Is.EqualTo(request.InstitutionId));

			if (request.IssueDate.HasValue)
			{
				Assert.That(result.IssueDate, Is.EqualTo(request.IssueDate));
			}

			if (!string.IsNullOrWhiteSpace(request.CertificateEducationUrl))
			{
				Assert.That(result.CertificateUrl, Is.EqualTo(request.CertificateEducationUrl));
			}

			if (request.Verified.HasValue)
			{
				Assert.That(result.Verified, Is.EqualTo(request.Verified.Value));
				if (request.Verified.Value == VerifyStatus.Rejected)
				{
					Assert.That(result.RejectReason, Is.Not.Null.And.Not.Empty);
				}
				else
				{
					Assert.That(result.RejectReason, Is.Null);
				}
			}

			_tutorEducationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TutorEducation>()), Times.Once);
		}
		else
		{
			var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateAsync(request));
			Assert.That(exception, Is.InstanceOf(expectedExceptionType!));
		}
	}



	/// <summary>
	/// Test CreateBulkAsync với số lượng khác nhau - tạo thành công tất cả các items
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(3)]
	[TestCase(5)]
	public async Task CreateBulkAsync_WithDifferentCounts_CreatesAllItems(int count)
	{
		// Arrange
		var requests = Enumerable.Range(1, count)
			.Select(i => new TutorEducationCreateRequest
			{
				TutorId = i,
				InstitutionId = 1,
				IssueDate = new DateTime(2020, 1, 1),
				CertificateEducationUrl = $"https://example.com/certificate{i}.jpg"
			})
			.ToList();

		foreach (var request in requests)
		{
			_tutorProfileRepositoryMock
				.Setup(r => r.GetByIdFullAsync(request.TutorId))
				.ReturnsAsync(FakeDataFactory.CreateFakeTutorProfile(request.TutorId));

			_educationInstitutionRepositoryMock
				.Setup(r => r.GetByIdAsync(request.InstitutionId))
				.ReturnsAsync(FakeDataFactory.CreateFakeEducationInstitution(request.InstitutionId));
		}

		int entityId = 1;
		_tutorEducationRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<TutorEducation>()))
			.Returns(Task.CompletedTask)
			.Callback<TutorEducation>(te => te.Id = entityId++);

		// Act
		var result = await _service.CreateBulkAsync(requests);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
		_tutorEducationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TutorEducation>()), Times.Exactly(count));
	}

	/// <summary>
	/// Test CreateBulkAsync với request không hợp lệ - ném InvalidOperationException khi tutor không tồn tại
	/// </summary>
	[Test]
	public void CreateBulkAsync_WithInvalidRequest_ThrowsInvalidOperationException()
	{
		// Arrange
		var requests = new List<TutorEducationCreateRequest>
		{
			new TutorEducationCreateRequest
			{
				TutorId = 999, // Non-existent tutor
				InstitutionId = 1,
				CertificateEducationUrl = "https://example.com/certificate.jpg"
			}
		};

		_tutorProfileRepositoryMock
			.Setup(r => r.GetByIdFullAsync(999))
			.ReturnsAsync((TutorProfile?)null);

		// Act & Assert
		Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.CreateBulkAsync(requests));
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
		_tutorEducationRepositoryMock
			.Setup(r => r.RemoveByIdAsync(id))
			.Returns(Task.CompletedTask);

		// Act
		await _service.DeleteAsync(id);

		// Assert
		_tutorEducationRepositoryMock.Verify(r => r.RemoveByIdAsync(id), Times.Once);
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


	/// <summary>
	/// Test DeleteByTutorIdAsync với các TutorId khác nhau - gọi repository để xóa tất cả tutor education của tutor
	/// </summary>
	[Test]
	[TestCase(1)]
	[TestCase(10)]
	[TestCase(100)]
	public async Task DeleteByTutorIdAsync_WithDifferentTutorIds_CallsRepository(int tutorId)
	{
		// Arrange
		_tutorEducationRepositoryMock
			.Setup(r => r.RemoveByTutorIdAsync(tutorId))
			.Returns(Task.CompletedTask);

		// Act
		await _service.DeleteByTutorIdAsync(tutorId);

		// Assert
		_tutorEducationRepositoryMock.Verify(r => r.RemoveByTutorIdAsync(tutorId), Times.Once);
	}

	/// <summary>
	/// Test DeleteByTutorIdAsync với TutorId không hợp lệ - ném ArgumentException
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(-1)]
	public void DeleteByTutorIdAsync_WithInvalidTutorId_ThrowsArgumentException(int tutorId)
	{
		// Act & Assert
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.DeleteByTutorIdAsync(tutorId));
		Assert.That(exception.Message, Does.Contain("TutorId must be greater than 0"));
	}

}

