using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.TutorSubject;
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
/// Test class cho TutorSubjectService - bao phủ tất cả methods và exception handling
/// </summary>
public class TutorSubjectServiceTests
{
	private Mock<ITutorSubjectRepository> _tutorSubjectRepositoryMock;
	private Mock<ITutorProfileRepository> _tutorProfileRepositoryMock;
	private Mock<ISubjectRepository> _subjectRepositoryMock;
	private Mock<ILevelRepository> _levelRepositoryMock;
	private IMapper _mapper;
	private TutorSubjectService _service;

	/// <summary>
	/// Khởi tạo các mock objects và service trước mỗi test
	/// </summary>
	[SetUp]
	public void Setup()
	{
		_tutorSubjectRepositoryMock = new Mock<ITutorSubjectRepository>();
		_tutorProfileRepositoryMock = new Mock<ITutorProfileRepository>();
		_subjectRepositoryMock = new Mock<ISubjectRepository>();
		_levelRepositoryMock = new Mock<ILevelRepository>();

		var config = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<MappingProfile>();
		});
		_mapper = config.CreateMapper();

		_service = new TutorSubjectService(
			_tutorSubjectRepositoryMock.Object,
			_mapper,
			_tutorProfileRepositoryMock.Object,
			_subjectRepositoryMock.Object,
			_levelRepositoryMock.Object
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
		TutorSubject? entity = shouldExist ? FakeDataFactory.CreateFakeTutorSubject(id) : null;

		_tutorSubjectRepositoryMock
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
		TutorSubject? entity = shouldExist ? FakeDataFactory.CreateFakeTutorSubject(1, tutorId) : null;

		_tutorSubjectRepositoryMock
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
			.Select(i => FakeDataFactory.CreateFakeTutorSubject(i, tutorId))
			.ToList();

		_tutorSubjectRepositoryMock
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
	/// Test GetBySubjectIdAsync với số lượng khác nhau - trả về danh sách DTOs đúng số lượng
	/// </summary>
	[Test]
	[TestCase(1, 0)]
	[TestCase(1, 1)]
	[TestCase(1, 5)]
	public async Task GetBySubjectIdAsync_WithDifferentCounts_ReturnsExpectedList(int subjectId, int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorSubject(i, subjectId: subjectId))
			.ToList();

		_tutorSubjectRepositoryMock
			.Setup(r => r.GetBySubjectIdAsync(subjectId))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetBySubjectIdAsync(subjectId);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
	}

	/// <summary>
	/// Test GetBySubjectIdAsync với SubjectId không hợp lệ - ném ArgumentException
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(-1)]
	public void GetBySubjectIdAsync_WithInvalidSubjectId_ThrowsArgumentException(int subjectId)
	{
		// Act & Assert
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetBySubjectIdAsync(subjectId));
		Assert.That(exception.Message, Does.Contain("SubjectId must be greater than 0"));
	}

	/// <summary>
	/// Test GetByLevelIdAsync với số lượng khác nhau - trả về danh sách DTOs đúng số lượng
	/// </summary>
	[Test]
	[TestCase(1, 0)]
	[TestCase(1, 1)]
	[TestCase(1, 5)]
	public async Task GetByLevelIdAsync_WithDifferentCounts_ReturnsExpectedList(int levelId, int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorSubject(i, levelId: levelId))
			.ToList();

		_tutorSubjectRepositoryMock
			.Setup(r => r.GetByLevelIdAsync(levelId))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetByLevelIdAsync(levelId);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
	}

	/// <summary>
	/// Test GetByLevelIdAsync với LevelId không hợp lệ - ném ArgumentException
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(-1)]
	public void GetByLevelIdAsync_WithInvalidLevelId_ThrowsArgumentException(int levelId)
	{
		// Act & Assert
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetByLevelIdAsync(levelId));
		Assert.That(exception.Message, Does.Contain("LevelId must be greater than 0"));
	}

	/// <summary>
	/// Test GetByHourlyRateRangeAsync với các khoảng giá khác nhau - trả về danh sách đúng khoảng giá
	/// </summary>
	[Test]
	[TestCase(100000, 200000, 2)]
	[TestCase(200000, 300000, 1)]
	[TestCase(500000, 600000, 0)]
	public async Task GetByHourlyRateRangeAsync_WithDifferentRanges_ReturnsExpectedList(decimal minRate, decimal maxRate, int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorSubject(i, hourlyRate: minRate + (i * 10000)))
			.ToList();

		_tutorSubjectRepositoryMock
			.Setup(r => r.GetByHourlyRateRangeAsync(minRate, maxRate))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetByHourlyRateRangeAsync(minRate, maxRate);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
	}

	/// <summary>
	/// Test GetTutorsBySubjectAndLevelAsync với các tham số khác nhau - trả về danh sách đúng
	/// </summary>
	[Test]
	[TestCase(1, 1, 0)]
	[TestCase(1, 1, 1)]
	[TestCase(1, 1, 3)]
	public async Task GetTutorsBySubjectAndLevelAsync_WithDifferentParams_ReturnsExpectedList(int subjectId, int levelId, int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorSubject(i, subjectId: subjectId, levelId: levelId))
			.ToList();

		_tutorSubjectRepositoryMock
			.Setup(r => r.GetTutorsBySubjectAndLevelAsync(subjectId, levelId))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetTutorsBySubjectAndLevelAsync(subjectId, levelId);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
	}

	/// <summary>
	/// Test GetAllFullAsync với số lượng khác nhau - trả về tất cả tutor subject với đầy đủ thông tin
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(10)]
	public async Task GetAllFullAsync_WithDifferentCounts_ReturnsExpectedList(int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorSubject(i))
			.ToList();

		_tutorSubjectRepositoryMock
			.Setup(r => r.GetAllFullAsync())
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetAllFullAsync();

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
	}

	/// <summary>
	/// Tạo các test cases cho CreateAsync - bao phủ các kịch bản: valid, tutor not found, subject not found, level not found, repository exception
	/// </summary>
	private static IEnumerable<TestCaseData> CreateAsyncTestCases()
	{
		// Valid case
		yield return new TestCaseData(
			new TutorSubjectCreateRequest
			{
				TutorId = 1,
				SubjectId = 1,
				LevelId = 1,
				HourlyRate = 200000
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			FakeDataFactory.CreateFakeSubject(1),
			FakeDataFactory.CreateFakeLevel(1),
			true,
			null
		).SetName("CreateAsync_ValidRequest_ReturnsCreatedDto");

		// Tutor not found
		yield return new TestCaseData(
			new TutorSubjectCreateRequest
			{
				TutorId = 999,
				SubjectId = 1,
				LevelId = 1,
				HourlyRate = 200000
			},
			null,
			FakeDataFactory.CreateFakeSubject(1),
			FakeDataFactory.CreateFakeLevel(1),
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_TutorNotFound_ThrowsInvalidOperationException");

		// Subject not found
		yield return new TestCaseData(
			new TutorSubjectCreateRequest
			{
				TutorId = 1,
				SubjectId = 999,
				LevelId = 1,
				HourlyRate = 200000
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			null,
			FakeDataFactory.CreateFakeLevel(1),
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_SubjectNotFound_ThrowsInvalidOperationException");

		// Level not found
		yield return new TestCaseData(
			new TutorSubjectCreateRequest
			{
				TutorId = 1,
				SubjectId = 1,
				LevelId = 999,
				HourlyRate = 200000
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			FakeDataFactory.CreateFakeSubject(1),
			null,
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_LevelNotFound_ThrowsInvalidOperationException");
	}

	/// <summary>
	/// Test CreateAsync khi repository throw exception - ném InvalidOperationException
	/// </summary>
	[Test]
	public void CreateAsync_WhenRepositoryThrowsException_ThrowsInvalidOperationException()
	{
		// Arrange
		var request = new TutorSubjectCreateRequest
		{
			TutorId = 1,
			SubjectId = 1,
			LevelId = 1,
			HourlyRate = 200000
		};

		_tutorProfileRepositoryMock
			.Setup(r => r.GetByIdFullAsync(request.TutorId))
			.ThrowsAsync(new Exception("Database error"));

		// Act & Assert
		var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.CreateAsync(request));
		Assert.That(exception.Message, Does.Contain("Failed to create tutor subject"));
	}

	/// <summary>
	/// Test CreateAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
	/// </summary>
	[Test]
	[TestCaseSource(nameof(CreateAsyncTestCases))]
	public async Task CreateAsync_WithVariousScenarios_HandlesCorrectly(
		TutorSubjectCreateRequest request,
		TutorProfile? tutor,
		Subject? subject,
		Level? level,
		bool shouldSucceed,
		Type? expectedExceptionType)
	{
		// Arrange
		_tutorProfileRepositoryMock
			.Setup(r => r.GetByIdFullAsync(request.TutorId))
			.ReturnsAsync(tutor);

		_subjectRepositoryMock
			.Setup(r => r.GetByIdAsync(request.SubjectId))
			.ReturnsAsync(subject);

		_levelRepositoryMock
			.Setup(r => r.GetByIdAsync(request.LevelId))
			.ReturnsAsync(level);

		TutorSubject? capturedEntity = null;
		_tutorSubjectRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<TutorSubject>()))
			.Returns(Task.CompletedTask)
			.Callback<TutorSubject>(ts =>
			{
				ts.Id = 1;
				capturedEntity = ts;
			});

		// Act & Assert
		if (shouldSucceed)
		{
			var result = await _service.CreateAsync(request);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.TutorId, Is.EqualTo(request.TutorId));
			Assert.That(result.SubjectId, Is.EqualTo(request.SubjectId));
			Assert.That(result.LevelId, Is.EqualTo(request.LevelId));
			Assert.That(result.HourlyRate, Is.EqualTo(request.HourlyRate));

			_tutorSubjectRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TutorSubject>()), Times.Once);
		}
		else
		{
			var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.CreateAsync(request));
			Assert.That(exception, Is.InstanceOf(expectedExceptionType!));
		}
	}

	/// <summary>
	/// Tạo các test cases cho UpdateAsync - bao phủ các kịch bản: valid update, entity not found, repository exception
	/// </summary>
	private static IEnumerable<TestCaseData> UpdateAsyncTestCases()
	{
		// Valid update
		yield return new TestCaseData(
			new TutorSubjectUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				SubjectId = 1,
				LevelId = 2,
				HourlyRate = 250000
			},
			FakeDataFactory.CreateFakeTutorSubject(1),
			true,
			null
		).SetName("UpdateAsync_ValidRequest_ReturnsUpdatedDto");

		// Update with null LevelId and HourlyRate (should preserve existing)
		yield return new TestCaseData(
			new TutorSubjectUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				SubjectId = 1
			},
			FakeDataFactory.CreateFakeTutorSubject(1, levelId: 1, hourlyRate: 200000),
			true,
			null
		).SetName("UpdateAsync_WithNullOptionalFields_PreservesExistingValues");

		// Entity not found
		yield return new TestCaseData(
			new TutorSubjectUpdateRequest
			{
				Id = 999,
				TutorId = 1,
				SubjectId = 1,
				LevelId = 1,
				HourlyRate = 200000
			},
			null,
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_EntityNotFound_ThrowsInvalidOperationException");
	}

	/// <summary>
	/// Test UpdateAsync khi repository throw exception - ném InvalidOperationException
	/// </summary>
	[Test]
	public void UpdateAsync_WhenRepositoryThrowsException_ThrowsInvalidOperationException()
	{
		// Arrange
		var request = new TutorSubjectUpdateRequest
		{
			Id = 1,
			TutorId = 1,
			SubjectId = 1,
			LevelId = 1,
			HourlyRate = 200000
		};

		_tutorSubjectRepositoryMock
			.Setup(r => r.GetByIdFullAsync(request.Id))
			.ThrowsAsync(new Exception("Database error"));

		// Act & Assert
		var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateAsync(request));
		Assert.That(exception.Message, Does.Contain("Failed to update tutor subject"));
	}

	/// <summary>
	/// Test UpdateAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
	/// </summary>
	[Test]
	[TestCaseSource(nameof(UpdateAsyncTestCases))]
	public async Task UpdateAsync_WithVariousScenarios_HandlesCorrectly(
		TutorSubjectUpdateRequest request,
		TutorSubject? existingEntity,
		bool shouldSucceed,
		Type? expectedExceptionType)
	{
		// Arrange
		_tutorSubjectRepositoryMock
			.Setup(r => r.GetByIdFullAsync(request.Id))
			.ReturnsAsync(existingEntity);

		_tutorSubjectRepositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<TutorSubject>()))
			.Returns(Task.CompletedTask);

		// Act & Assert
		if (shouldSucceed)
		{
			var result = await _service.UpdateAsync(request);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.Id, Is.EqualTo(request.Id));
			Assert.That(result.TutorId, Is.EqualTo(request.TutorId));
			Assert.That(result.SubjectId, Is.EqualTo(request.SubjectId));

			if (request.LevelId.HasValue)
			{
				Assert.That(result.LevelId, Is.EqualTo(request.LevelId.Value));
			}

			if (request.HourlyRate.HasValue)
			{
				Assert.That(result.HourlyRate, Is.EqualTo(request.HourlyRate.Value));
			}

			_tutorSubjectRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TutorSubject>()), Times.Once);
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
			.Select(i => new TutorSubjectCreateRequest
			{
				TutorId = i,
				SubjectId = 1,
				LevelId = 1,
				HourlyRate = 200000 + (i * 10000)
			})
			.ToList();

		foreach (var request in requests)
		{
			_tutorProfileRepositoryMock
				.Setup(r => r.GetByIdFullAsync(request.TutorId))
				.ReturnsAsync(FakeDataFactory.CreateFakeTutorProfile(request.TutorId));

			_subjectRepositoryMock
				.Setup(r => r.GetByIdAsync(request.SubjectId))
				.ReturnsAsync(FakeDataFactory.CreateFakeSubject(request.SubjectId));

			_levelRepositoryMock
				.Setup(r => r.GetByIdAsync(request.LevelId))
				.ReturnsAsync(FakeDataFactory.CreateFakeLevel(request.LevelId));
		}

		int entityId = 1;
		_tutorSubjectRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<TutorSubject>()))
			.Returns(Task.CompletedTask)
			.Callback<TutorSubject>(ts => ts.Id = entityId++);

		// Act
		var result = await _service.CreateBulkAsync(requests);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
		_tutorSubjectRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TutorSubject>()), Times.Exactly(count));
	}

	/// <summary>
	/// Test CreateBulkAsync với request không hợp lệ - ném InvalidOperationException khi tutor không tồn tại
	/// </summary>
	[Test]
	public void CreateBulkAsync_WithInvalidRequest_ThrowsInvalidOperationException()
	{
		// Arrange
		var requests = new List<TutorSubjectCreateRequest>
		{
			new TutorSubjectCreateRequest
			{
				TutorId = 999, // Non-existent tutor
				SubjectId = 1,
				LevelId = 1,
				HourlyRate = 200000
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
		_tutorSubjectRepositoryMock
			.Setup(r => r.RemoveByIdAsync(id))
			.Returns(Task.CompletedTask);

		// Act
		await _service.DeleteAsync(id);

		// Assert
		_tutorSubjectRepositoryMock.Verify(r => r.RemoveByIdAsync(id), Times.Once);
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
	/// Test DeleteByTutorIdAsync với các TutorId khác nhau - gọi repository để xóa tất cả tutor subject của tutor
	/// </summary>
	[Test]
	[TestCase(1)]
	[TestCase(10)]
	[TestCase(100)]
	public async Task DeleteByTutorIdAsync_WithDifferentTutorIds_CallsRepository(int tutorId)
	{
		// Arrange
		_tutorSubjectRepositoryMock
			.Setup(r => r.RemoveByTutorIdAsync(tutorId))
			.Returns(Task.CompletedTask);

		// Act
		await _service.DeleteByTutorIdAsync(tutorId);

		// Assert
		_tutorSubjectRepositoryMock.Verify(r => r.RemoveByTutorIdAsync(tutorId), Times.Once);
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

