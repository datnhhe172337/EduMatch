using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.TutorCertificate;
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

/// <summary>
/// Test class cho TutorCertificateService
/// </summary>
public class TutorCertificateServiceTests
{
	private Mock<ITutorCertificateRepository> _tutorCertificateRepositoryMock;
	private Mock<ITutorProfileRepository> _tutorProfileRepositoryMock;
	private Mock<ICertificateTypeRepository> _certificateTypeRepositoryMock;
	private Mock<ICloudMediaService> _cloudMediaServiceMock;
	private IMapper _mapper;
	private CurrentUserService _currentUserService;
	private TutorCertificateService _service;

	/// <summary>
	/// Khởi tạo các mock objects và service trước mỗi test
	/// </summary>
	[SetUp]
	public void Setup()
	{
		_tutorCertificateRepositoryMock = new Mock<ITutorCertificateRepository>();
		_tutorProfileRepositoryMock = new Mock<ITutorProfileRepository>();
		_certificateTypeRepositoryMock = new Mock<ICertificateTypeRepository>();
		_cloudMediaServiceMock = new Mock<ICloudMediaService>();

		var config = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<MappingProfile>();
		});
		_mapper = config.CreateMapper();

		_currentUserService = new CurrentUserServiceFake("test@gmail.com");

		_service = new TutorCertificateService(
			_tutorCertificateRepositoryMock.Object,
			_mapper,
			_cloudMediaServiceMock.Object,
			_currentUserService,
			_tutorProfileRepositoryMock.Object,
			_certificateTypeRepositoryMock.Object
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
		TutorCertificate? entity = shouldExist ? FakeDataFactory.CreateFakeTutorCertificate(id) : null;

		_tutorCertificateRepositoryMock
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
	/// Test GetByTutorIdFullAsync với các TutorId khác nhau - trả về DTO khi tồn tại, null khi không tồn tại
	/// </summary>
	[Test]
	[TestCase(1, true)]
	[TestCase(999, false)]
	public async Task GetByTutorIdFullAsync_WithDifferentTutorIds_ReturnsExpectedResult(int tutorId, bool shouldExist)
	{
		// Arrange
		TutorCertificate? entity = shouldExist ? FakeDataFactory.CreateFakeTutorCertificate(1, tutorId) : null;

		_tutorCertificateRepositoryMock
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
			.Select(i => FakeDataFactory.CreateFakeTutorCertificate(i, tutorId))
			.ToList();

		_tutorCertificateRepositoryMock
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
	/// Test GetByCertificateTypeAsync với số lượng khác nhau - trả về danh sách DTOs đúng số lượng
	/// </summary>
	[Test]
	[TestCase(1, 0)]
	[TestCase(1, 1)]
	[TestCase(1, 5)]
	public async Task GetByCertificateTypeAsync_WithDifferentCounts_ReturnsExpectedList(int certificateTypeId, int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorCertificate(i, certificateTypeId: certificateTypeId))
			.ToList();

		_tutorCertificateRepositoryMock
			.Setup(r => r.GetByCertificateTypeAsync(certificateTypeId))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetByCertificateTypeAsync(certificateTypeId);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
	}

	/// <summary>
	/// Test GetByCertificateTypeAsync với CertificateTypeId không hợp lệ - ném ArgumentException
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(-1)]
	public void GetByCertificateTypeAsync_WithInvalidCertificateTypeId_ThrowsArgumentException(int certificateTypeId)
	{
		// Act & Assert
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetByCertificateTypeAsync(certificateTypeId));
		Assert.That(exception.Message, Does.Contain("CertificateTypeId must be greater than 0"));
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
			.Select(i => FakeDataFactory.CreateFakeTutorCertificate(i, verified: status))
			.ToList();

		_tutorCertificateRepositoryMock
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
	/// Test GetExpiredCertificatesAsync với số lượng khác nhau - trả về danh sách certificate đã hết hạn
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(5)]
	public async Task GetExpiredCertificatesAsync_WithDifferentCounts_ReturnsExpectedList(int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorCertificate(i, expiryDate: DateTime.UtcNow.AddDays(-1)))
			.ToList();

		_tutorCertificateRepositoryMock
			.Setup(r => r.GetExpiredCertificatesAsync())
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetExpiredCertificatesAsync();

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
	}

	/// <summary>
	/// Test GetExpiringCertificatesAsync với số lượng khác nhau - trả về danh sách certificate sắp hết hạn
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(5)]
	public async Task GetExpiringCertificatesAsync_WithDifferentCounts_ReturnsExpectedList(int count)
	{
		// Arrange
		var beforeDate = DateTime.UtcNow.AddDays(30);
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorCertificate(i, expiryDate: beforeDate.AddDays(-1)))
			.ToList();

		_tutorCertificateRepositoryMock
			.Setup(r => r.GetExpiringCertificatesAsync(beforeDate))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetExpiringCertificatesAsync(beforeDate);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
	}

	/// <summary>
	/// Test GetAllFullAsync với số lượng khác nhau - trả về tất cả tutor certificate với đầy đủ thông tin
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(10)]
	public async Task GetAllFullAsync_WithDifferentCounts_ReturnsExpectedList(int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorCertificate(i))
			.ToList();

		_tutorCertificateRepositoryMock
			.Setup(r => r.GetAllFullAsync())
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetAllFullAsync();

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
	}

	/// <summary>
	/// Tạo các test cases cho CreateAsync - bao phủ các kịch bản: valid, tutor not found, certificate type not found, certificate URL invalid
	/// </summary>
	private static IEnumerable<TestCaseData> CreateAsyncTestCases()
	{
		// Valid case
		yield return new TestCaseData(
			new TutorCertificateCreateRequest
			{
				TutorId = 1,
				CertificateTypeId = 1,
				IssueDate = new DateTime(2020, 1, 1),
				ExpiryDate = new DateTime(2025, 12, 31),
				CertificateUrl = "https://example.com/certificate.jpg"
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			FakeDataFactory.CreateFakeCertificateType(1),
			true,
			null
		).SetName("CreateAsync_ValidRequest_ReturnsCreatedDto");

		// Tutor not found
		yield return new TestCaseData(
			new TutorCertificateCreateRequest
			{
				TutorId = 999,
				CertificateTypeId = 1,
				IssueDate = new DateTime(2020, 1, 1),
				ExpiryDate = new DateTime(2025, 12, 31),
				CertificateUrl = "https://example.com/certificate.jpg"
			},
			null,
			FakeDataFactory.CreateFakeCertificateType(1),
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_TutorNotFound_ThrowsInvalidOperationException");

		// Certificate type not found
		yield return new TestCaseData(
			new TutorCertificateCreateRequest
			{
				TutorId = 1,
				CertificateTypeId = 999,
				IssueDate = new DateTime(2020, 1, 1),
				ExpiryDate = new DateTime(2025, 12, 31),
				CertificateUrl = "https://example.com/certificate.jpg"
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			null,
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_CertificateTypeNotFound_ThrowsInvalidOperationException");

		// Certificate URL is empty
		yield return new TestCaseData(
			new TutorCertificateCreateRequest
			{
				TutorId = 1,
				CertificateTypeId = 1,
				IssueDate = new DateTime(2020, 1, 1),
				ExpiryDate = new DateTime(2025, 12, 31),
				CertificateUrl = ""
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			FakeDataFactory.CreateFakeCertificateType(1),
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_CertificateUrlEmpty_ThrowsInvalidOperationException");

		// Certificate URL is null
		yield return new TestCaseData(
			new TutorCertificateCreateRequest
			{
				TutorId = 1,
				CertificateTypeId = 1,
				IssueDate = new DateTime(2020, 1, 1),
				ExpiryDate = new DateTime(2025, 12, 31),
				CertificateUrl = null
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			FakeDataFactory.CreateFakeCertificateType(1),
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_CertificateUrlNull_ThrowsInvalidOperationException");

		// Certificate URL is whitespace
		yield return new TestCaseData(
			new TutorCertificateCreateRequest
			{
				TutorId = 1,
				CertificateTypeId = 1,
				IssueDate = new DateTime(2020, 1, 1),
				ExpiryDate = new DateTime(2025, 12, 31),
				CertificateUrl = "   "
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			FakeDataFactory.CreateFakeCertificateType(1),
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_CertificateUrlWhitespace_ThrowsInvalidOperationException");
	}

	/// <summary>
	/// Test CreateAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
	/// </summary>
	[Test]
	[TestCaseSource(nameof(CreateAsyncTestCases))]
	public async Task CreateAsync_WithVariousScenarios_HandlesCorrectly(
		TutorCertificateCreateRequest request,
		TutorProfile? tutor,
		CertificateType? certificateType,
		bool shouldSucceed,
		Type? expectedExceptionType)
	{
		// Arrange
		_tutorProfileRepositoryMock
			.Setup(r => r.GetByIdFullAsync(request.TutorId))
			.ReturnsAsync(tutor);

		_certificateTypeRepositoryMock
			.Setup(r => r.GetByIdAsync(request.CertificateTypeId))
			.ReturnsAsync(certificateType);

		TutorCertificate? capturedEntity = null;
		_tutorCertificateRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<TutorCertificate>()))
			.Returns(Task.CompletedTask)
			.Callback<TutorCertificate>(tc =>
			{
				tc.Id = 1;
				capturedEntity = tc;
			});

		// Act & Assert
		if (shouldSucceed)
		{
			var result = await _service.CreateAsync(request);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.TutorId, Is.EqualTo(request.TutorId));
			Assert.That(result.CertificateTypeId, Is.EqualTo(request.CertificateTypeId));
			Assert.That(result.CertificateUrl, Is.EqualTo(request.CertificateUrl));
			Assert.That(result.Verified, Is.EqualTo(VerifyStatus.Pending));
			if (request.IssueDate.HasValue)
				Assert.That(result.IssueDate, Is.EqualTo(request.IssueDate));
			if (request.ExpiryDate.HasValue)
				Assert.That(result.ExpiryDate, Is.EqualTo(request.ExpiryDate));

			_tutorCertificateRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TutorCertificate>()), Times.Once);
		}
		else
		{
			var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.CreateAsync(request));
			Assert.That(exception, Is.InstanceOf(expectedExceptionType!));
		}
	}

	/// <summary>
	/// Tạo các test cases cho UpdateAsync - bao phủ các kịch bản: valid update, entity not found, certificate type not found, rejected without reason, verified clears reason
	/// </summary>
	private static IEnumerable<TestCaseData> UpdateAsyncTestCases()
	{
		// Valid update
		yield return new TestCaseData(
			new TutorCertificateUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				CertificateTypeId = 1,
				IssueDate = new DateTime(2021, 1, 1),
				ExpiryDate = new DateTime(2026, 12, 31),
				CertificateUrl = "https://example.com/new-certificate.jpg"
			},
			FakeDataFactory.CreateFakeTutorCertificate(1),
			FakeDataFactory.CreateFakeCertificateType(1),
			true,
			null
		).SetName("UpdateAsync_ValidRequest_ReturnsUpdatedDto");

		// Entity not found
		yield return new TestCaseData(
			new TutorCertificateUpdateRequest
			{
				Id = 999,
				TutorId = 1,
				CertificateTypeId = 1,
				IssueDate = new DateTime(2021, 1, 1),
				ExpiryDate = new DateTime(2026, 12, 31),
				CertificateUrl = "https://example.com/certificate.jpg"
			},
			null,
			FakeDataFactory.CreateFakeCertificateType(1),
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_EntityNotFound_ThrowsInvalidOperationException");

		// Certificate type not found
		yield return new TestCaseData(
			new TutorCertificateUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				CertificateTypeId = 999,
				IssueDate = new DateTime(2021, 1, 1),
				ExpiryDate = new DateTime(2026, 12, 31),
				CertificateUrl = "https://example.com/certificate.jpg"
			},
			FakeDataFactory.CreateFakeTutorCertificate(1),
			null,
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_CertificateTypeNotFound_ThrowsInvalidOperationException");

		// Update with Verified = Rejected but no RejectReason
		yield return new TestCaseData(
			new TutorCertificateUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				CertificateTypeId = 1,
				Verified = VerifyStatus.Rejected,
				RejectReason = null
			},
			FakeDataFactory.CreateFakeTutorCertificate(1, verified: VerifyStatus.Pending, rejectReason: null),
			FakeDataFactory.CreateFakeCertificateType(1),
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_RejectedWithoutReason_ThrowsInvalidOperationException");

		// Update with Verified = Rejected with empty RejectReason
		yield return new TestCaseData(
			new TutorCertificateUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				CertificateTypeId = 1,
				Verified = VerifyStatus.Rejected,
				RejectReason = ""
			},
			FakeDataFactory.CreateFakeTutorCertificate(1, verified: VerifyStatus.Pending, rejectReason: null),
			FakeDataFactory.CreateFakeCertificateType(1),
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_RejectedWithEmptyReason_ThrowsInvalidOperationException");

		// Update with Verified = Rejected with valid RejectReason
		yield return new TestCaseData(
			new TutorCertificateUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				CertificateTypeId = 1,
				Verified = VerifyStatus.Rejected,
				RejectReason = "Invalid certificate"
			},
			FakeDataFactory.CreateFakeTutorCertificate(1, verified: VerifyStatus.Pending),
			FakeDataFactory.CreateFakeCertificateType(1),
			true,
			null
		).SetName("UpdateAsync_RejectedWithValidReason_UpdatesCorrectly");

		// Update with Verified = Verified (clears RejectReason)
		yield return new TestCaseData(
			new TutorCertificateUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				CertificateTypeId = 1,
				Verified = VerifyStatus.Verified
			},
			FakeDataFactory.CreateFakeTutorCertificate(1, verified: VerifyStatus.Pending, rejectReason: "Old reason"),
			FakeDataFactory.CreateFakeCertificateType(1),
			true,
			null
		).SetName("UpdateAsync_Verified_ClearsRejectReason");

		// Update without changing certificate URL
		yield return new TestCaseData(
			new TutorCertificateUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				CertificateTypeId = 1,
				IssueDate = new DateTime(2021, 1, 1)
			},
			FakeDataFactory.CreateFakeTutorCertificate(1, certificateUrl: "https://example.com/old-cert.jpg"),
			FakeDataFactory.CreateFakeCertificateType(1),
			true,
			null
		).SetName("UpdateAsync_WithoutCertificateUrl_PreservesOldUrl");
	}

	/// <summary>
	/// Test UpdateAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
	/// </summary>
	[Test]
	[TestCaseSource(nameof(UpdateAsyncTestCases))]
	public async Task UpdateAsync_WithVariousScenarios_HandlesCorrectly(
		TutorCertificateUpdateRequest request,
		TutorCertificate? existingEntity,
		CertificateType? certificateType,
		bool shouldSucceed,
		Type? expectedExceptionType)
	{
		// Arrange
		_tutorCertificateRepositoryMock
			.Setup(r => r.GetByIdFullAsync(request.Id))
			.ReturnsAsync(existingEntity);

		if (certificateType != null)
		{
			_certificateTypeRepositoryMock
				.Setup(r => r.GetByIdAsync(request.CertificateTypeId))
				.ReturnsAsync(certificateType);
		}
		else
		{
			_certificateTypeRepositoryMock
				.Setup(r => r.GetByIdAsync(request.CertificateTypeId))
				.ReturnsAsync((CertificateType?)null);
		}

		_tutorCertificateRepositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<TutorCertificate>()))
			.Returns(Task.CompletedTask);

		// Act & Assert
		if (shouldSucceed)
		{
			var result = await _service.UpdateAsync(request);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.Id, Is.EqualTo(request.Id));
			Assert.That(result.TutorId, Is.EqualTo(request.TutorId));
			Assert.That(result.CertificateTypeId, Is.EqualTo(request.CertificateTypeId));

			if (request.IssueDate.HasValue)
			{
				Assert.That(result.IssueDate, Is.EqualTo(request.IssueDate));
			}

			if (request.ExpiryDate.HasValue)
			{
				Assert.That(result.ExpiryDate, Is.EqualTo(request.ExpiryDate));
			}

			if (!string.IsNullOrWhiteSpace(request.CertificateUrl))
			{
				Assert.That(result.CertificateUrl, Is.EqualTo(request.CertificateUrl));
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

			_tutorCertificateRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TutorCertificate>()), Times.Once);
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
			.Select(i => new TutorCertificateCreateRequest
			{
				TutorId = i,
				CertificateTypeId = 1,
				IssueDate = new DateTime(2020, 1, 1),
				ExpiryDate = new DateTime(2025, 12, 31),
				CertificateUrl = $"https://example.com/certificate{i}.jpg"
			})
			.ToList();

		foreach (var request in requests)
		{
			_tutorProfileRepositoryMock
				.Setup(r => r.GetByIdFullAsync(request.TutorId))
				.ReturnsAsync(FakeDataFactory.CreateFakeTutorProfile(request.TutorId));

			_certificateTypeRepositoryMock
				.Setup(r => r.GetByIdAsync(request.CertificateTypeId))
				.ReturnsAsync(FakeDataFactory.CreateFakeCertificateType(request.CertificateTypeId));
		}

		int entityId = 1;
		_tutorCertificateRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<TutorCertificate>()))
			.Returns(Task.CompletedTask)
			.Callback<TutorCertificate>(tc => tc.Id = entityId++);

		// Act
		var result = await _service.CreateBulkAsync(requests);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
		_tutorCertificateRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TutorCertificate>()), Times.Exactly(count));
	}

	/// <summary>
	/// Test CreateBulkAsync với request không hợp lệ - ném InvalidOperationException khi tutor không tồn tại
	/// </summary>
	[Test]
	public void CreateBulkAsync_WithInvalidRequest_ThrowsInvalidOperationException()
	{
		// Arrange
		var requests = new List<TutorCertificateCreateRequest>
		{
			new TutorCertificateCreateRequest
			{
				TutorId = 999, // Non-existent tutor
				CertificateTypeId = 1,
				CertificateUrl = "https://example.com/certificate.jpg"
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
		_tutorCertificateRepositoryMock
			.Setup(r => r.RemoveByIdAsync(id))
			.Returns(Task.CompletedTask);

		// Act
		await _service.DeleteAsync(id);

		// Assert
		_tutorCertificateRepositoryMock.Verify(r => r.RemoveByIdAsync(id), Times.Once);
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
	/// Test DeleteByTutorIdAsync với các TutorId khác nhau - gọi repository để xóa tất cả tutor certificate của tutor
	/// </summary>
	[Test]
	[TestCase(1)]
	[TestCase(10)]
	[TestCase(100)]
	public async Task DeleteByTutorIdAsync_WithDifferentTutorIds_CallsRepository(int tutorId)
	{
		// Arrange
		_tutorCertificateRepositoryMock
			.Setup(r => r.RemoveByTutorIdAsync(tutorId))
			.Returns(Task.CompletedTask);

		// Act
		await _service.DeleteByTutorIdAsync(tutorId);

		// Assert
		_tutorCertificateRepositoryMock.Verify(r => r.RemoveByTutorIdAsync(tutorId), Times.Once);
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

