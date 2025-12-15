using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.TutorAvailability;
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
/// Test class cho TutorAvailabilityService - bao phủ tất cả methods và exception handling
/// </summary>
public class TutorAvailabilityServiceTests
{
	private Mock<ITutorAvailabilityRepository> _tutorAvailabilityRepositoryMock;
	private Mock<ITutorProfileRepository> _tutorProfileRepositoryMock;
	private Mock<ITimeSlotRepository> _timeSlotRepositoryMock;
	private Mock<IScheduleRepository> _scheduleRepositoryMock;
	private IMapper _mapper;
	private TutorAvailabilityService _service;

	/// <summary>
	/// Khởi tạo các mock objects và service trước mỗi test
	/// </summary>
	[SetUp]
	public void Setup()
	{
		_tutorAvailabilityRepositoryMock = new Mock<ITutorAvailabilityRepository>();
		_tutorProfileRepositoryMock = new Mock<ITutorProfileRepository>();
		_timeSlotRepositoryMock = new Mock<ITimeSlotRepository>();
		_scheduleRepositoryMock = new Mock<IScheduleRepository>();

		var config = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<MappingProfile>();
		});
		_mapper = config.CreateMapper();

		_service = new TutorAvailabilityService(
			_tutorAvailabilityRepositoryMock.Object,
			_mapper,
			_timeSlotRepositoryMock.Object,
			_tutorProfileRepositoryMock.Object,
			_scheduleRepositoryMock.Object
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
		TutorAvailability? entity = shouldExist ? FakeDataFactory.CreateFakeTutorAvailability(id) : null;

		_tutorAvailabilityRepositoryMock
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
			.Select(i => FakeDataFactory.CreateFakeTutorAvailability(i, tutorId))
			.ToList();

		_tutorAvailabilityRepositoryMock
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
	/// Test GetByTutorIdFullAsync với số lượng khác nhau - trả về danh sách DTOs đúng số lượng
	/// </summary>
	[Test]
	[TestCase(1, 0)]
	[TestCase(1, 1)]
	[TestCase(1, 5)]
	public async Task GetByTutorIdFullAsync_WithDifferentCounts_ReturnsExpectedList(int tutorId, int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorAvailability(i, tutorId))
			.ToList();

		_tutorAvailabilityRepositoryMock
			.Setup(r => r.GetByTutorIdFullAsync(tutorId))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetByTutorIdFullAsync(tutorId);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
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
	/// Test GetAllFullAsync với số lượng khác nhau - trả về tất cả tutor availability với đầy đủ thông tin
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(10)]
	public async Task GetAllFullAsync_WithDifferentCounts_ReturnsExpectedList(int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTutorAvailability(i))
			.ToList();

		_tutorAvailabilityRepositoryMock
			.Setup(r => r.GetAllFullAsync())
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetAllFullAsync();

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
	}

	/// <summary>
	/// Tạo các test cases cho CreateAsync - bao phủ các kịch bản: valid, tutor not found, timeSlot not found, duplicate, schedule conflict, repository exception
	/// </summary>
	private static IEnumerable<TestCaseData> CreateAsyncTestCases()
	{
		var testDate = DateTime.UtcNow.Date;

		// Valid case
		yield return new TestCaseData(
			new TutorAvailabilityCreateRequest
			{
				TutorId = 1,
				SlotId = 1,
				StartDate = testDate
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			FakeDataFactory.CreateFakeTimeSlot(1),
			new List<TutorAvailability>(),
			false,
			true,
			null
		).SetName("CreateAsync_ValidRequest_ReturnsCreatedDto");

		// Tutor not found
		yield return new TestCaseData(
			new TutorAvailabilityCreateRequest
			{
				TutorId = 999,
				SlotId = 1,
				StartDate = testDate
			},
			null,
			FakeDataFactory.CreateFakeTimeSlot(1),
			new List<TutorAvailability>(),
			false,
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_TutorNotFound_ThrowsInvalidOperationException");

		// TimeSlot not found
		yield return new TestCaseData(
			new TutorAvailabilityCreateRequest
			{
				TutorId = 1,
				SlotId = 999,
				StartDate = testDate
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			null,
			new List<TutorAvailability>(),
			false,
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_TimeSlotNotFound_ThrowsInvalidOperationException");

		// Duplicate availability
		yield return new TestCaseData(
			new TutorAvailabilityCreateRequest
			{
				TutorId = 1,
				SlotId = 1,
				StartDate = testDate
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			FakeDataFactory.CreateFakeTimeSlot(1),
			new List<TutorAvailability> { FakeDataFactory.CreateFakeTutorAvailability(1, 1, 1, testDate) },
			false,
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_DuplicateAvailability_ThrowsInvalidOperationException");

		// Schedule conflict
		yield return new TestCaseData(
			new TutorAvailabilityCreateRequest
			{
				TutorId = 1,
				SlotId = 1,
				StartDate = testDate
			},
			FakeDataFactory.CreateFakeTutorProfile(1),
			FakeDataFactory.CreateFakeTimeSlot(1),
			new List<TutorAvailability>(),
			true,
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_ScheduleConflict_ThrowsInvalidOperationException");
	}

	/// <summary>
	/// Test CreateAsync khi repository throw exception - ném InvalidOperationException
	/// </summary>
	[Test]
	public void CreateAsync_WhenRepositoryThrowsException_ThrowsInvalidOperationException()
	{
		// Arrange
		var request = new TutorAvailabilityCreateRequest
		{
			TutorId = 1,
			SlotId = 1,
			StartDate = DateTime.UtcNow.Date
		};

		_tutorProfileRepositoryMock
			.Setup(r => r.GetByIdFullAsync(request.TutorId))
			.ThrowsAsync(new Exception("Database error"));

		// Act & Assert
		var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.CreateAsync(request));
		Assert.That(exception.Message, Does.Contain("Failed to create tutor availability"));
	}

	/// <summary>
	/// Test CreateAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
	/// </summary>
	[Test]
	[TestCaseSource(nameof(CreateAsyncTestCases))]
	public async Task CreateAsync_WithVariousScenarios_HandlesCorrectly(
		TutorAvailabilityCreateRequest request,
		TutorProfile? tutor,
		TimeSlot? timeSlot,
		List<TutorAvailability> existingAvailabilities,
		bool hasScheduleConflict,
		bool shouldSucceed,
		Type? expectedExceptionType)
	{
		// Arrange
		_tutorProfileRepositoryMock
			.Setup(r => r.GetByIdFullAsync(request.TutorId))
			.ReturnsAsync(tutor);

		_timeSlotRepositoryMock
			.Setup(r => r.GetByIdAsync(request.SlotId))
			.ReturnsAsync(timeSlot);

		_tutorAvailabilityRepositoryMock
			.Setup(r => r.GetByTutorIdAsync(request.TutorId))
			.ReturnsAsync(existingAvailabilities);

		_scheduleRepositoryMock
			.Setup(r => r.HasTutorScheduleOnSlotDateAsync(request.TutorId, request.SlotId, request.StartDate.Date))
			.ReturnsAsync(hasScheduleConflict);

		TutorAvailability? capturedEntity = null;
		_tutorAvailabilityRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<TutorAvailability>()))
			.Returns(Task.CompletedTask)
			.Callback<TutorAvailability>(ta =>
			{
				ta.Id = 1;
				capturedEntity = ta;
			});

		// Act & Assert
		if (shouldSucceed)
		{
			var result = await _service.CreateAsync(request);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.TutorId, Is.EqualTo(request.TutorId));
			Assert.That(result.SlotId, Is.EqualTo(request.SlotId));
			Assert.That(result.Status, Is.EqualTo(TutorAvailabilityStatus.Available));

			_tutorAvailabilityRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TutorAvailability>()), Times.Once);
		}
		else
		{
			var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.CreateAsync(request));
			Assert.That(exception, Is.InstanceOf(expectedExceptionType!));
		}
	}

	/// <summary>
	/// Tạo các test cases cho UpdateAsync - bao phủ các kịch bản: valid update, entity not found, timeSlot not found, duplicate, schedule conflict, repository exception
	/// </summary>
	private static IEnumerable<TestCaseData> UpdateAsyncTestCases()
	{
		var testDate = DateTime.UtcNow.Date;

		// Valid update
		yield return new TestCaseData(
			new TutorAvailabilityUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				SlotId = 1,
				StartDate = testDate
			},
			FakeDataFactory.CreateFakeTutorAvailability(1),
			FakeDataFactory.CreateFakeTimeSlot(1),
			new List<TutorAvailability>(),
			false,
			true,
			null
		).SetName("UpdateAsync_ValidRequest_ReturnsUpdatedDto");

		// Entity not found
		yield return new TestCaseData(
			new TutorAvailabilityUpdateRequest
			{
				Id = 999,
				TutorId = 1,
				SlotId = 1,
				StartDate = testDate
			},
			null,
			FakeDataFactory.CreateFakeTimeSlot(1),
			new List<TutorAvailability>(),
			false,
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_EntityNotFound_ThrowsInvalidOperationException");

		// TimeSlot not found
		yield return new TestCaseData(
			new TutorAvailabilityUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				SlotId = 999,
				StartDate = testDate
			},
			FakeDataFactory.CreateFakeTutorAvailability(1),
			null,
			new List<TutorAvailability>(),
			false,
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_TimeSlotNotFound_ThrowsInvalidOperationException");

		// Duplicate availability
		yield return new TestCaseData(
			new TutorAvailabilityUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				SlotId = 1,
				StartDate = testDate
			},
			FakeDataFactory.CreateFakeTutorAvailability(1),
			FakeDataFactory.CreateFakeTimeSlot(1),
			new List<TutorAvailability> { FakeDataFactory.CreateFakeTutorAvailability(2, 1, 1, testDate) },
			false,
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_DuplicateAvailability_ThrowsInvalidOperationException");

		// Schedule conflict
		yield return new TestCaseData(
			new TutorAvailabilityUpdateRequest
			{
				Id = 1,
				TutorId = 1,
				SlotId = 1,
				StartDate = testDate
			},
			FakeDataFactory.CreateFakeTutorAvailability(1),
			FakeDataFactory.CreateFakeTimeSlot(1),
			new List<TutorAvailability>(),
			true,
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_ScheduleConflict_ThrowsInvalidOperationException");
	}

	/// <summary>
	/// Test UpdateAsync khi repository throw exception - ném InvalidOperationException
	/// </summary>
	[Test]
	public void UpdateAsync_WhenRepositoryThrowsException_ThrowsInvalidOperationException()
	{
		// Arrange
		var request = new TutorAvailabilityUpdateRequest
		{
			Id = 1,
			TutorId = 1,
			SlotId = 1,
			StartDate = DateTime.UtcNow.Date
		};

		_tutorAvailabilityRepositoryMock
			.Setup(r => r.GetByIdFullAsync(request.Id))
			.ThrowsAsync(new Exception("Database error"));

		// Act & Assert
		var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateAsync(request));
		Assert.That(exception.Message, Does.Contain("Failed to update tutor availability"));
	}

	/// <summary>
	/// Test UpdateAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
	/// </summary>
	[Test]
	[TestCaseSource(nameof(UpdateAsyncTestCases))]
	public async Task UpdateAsync_WithVariousScenarios_HandlesCorrectly(
		TutorAvailabilityUpdateRequest request,
		TutorAvailability? existingEntity,
		TimeSlot? timeSlot,
		List<TutorAvailability> existingAvailabilities,
		bool hasScheduleConflict,
		bool shouldSucceed,
		Type? expectedExceptionType)
	{
		// Arrange
		_tutorAvailabilityRepositoryMock
			.Setup(r => r.GetByIdFullAsync(request.Id))
			.ReturnsAsync(existingEntity);

		if (timeSlot != null)
		{
			_timeSlotRepositoryMock
				.Setup(r => r.GetByIdAsync(request.SlotId))
				.ReturnsAsync(timeSlot);
		}

		_tutorAvailabilityRepositoryMock
			.Setup(r => r.GetByTutorIdAsync(request.TutorId))
			.ReturnsAsync(existingAvailabilities);

		_scheduleRepositoryMock
			.Setup(r => r.HasTutorScheduleOnSlotDateAsync(request.TutorId, request.SlotId, request.StartDate.Date))
			.ReturnsAsync(hasScheduleConflict);

		_tutorAvailabilityRepositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<TutorAvailability>()))
			.Returns(Task.CompletedTask);

		// Act & Assert
		if (shouldSucceed)
		{
			var result = await _service.UpdateAsync(request);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.Id, Is.EqualTo(request.Id));
			Assert.That(result.TutorId, Is.EqualTo(request.TutorId));
			Assert.That(result.SlotId, Is.EqualTo(request.SlotId));

			

			_tutorAvailabilityRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TutorAvailability>()), Times.Once);
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
		var testDate = DateTime.UtcNow.Date;
		var requests = Enumerable.Range(1, count)
			.Select(i => new TutorAvailabilityCreateRequest
			{
				TutorId = 1,
				SlotId = i,
				StartDate = testDate.AddDays(i)
			})
			.ToList();

		foreach (var request in requests)
		{
			_tutorProfileRepositoryMock
				.Setup(r => r.GetByIdFullAsync(request.TutorId))
				.ReturnsAsync(FakeDataFactory.CreateFakeTutorProfile(request.TutorId));

			_timeSlotRepositoryMock
				.Setup(r => r.GetByIdAsync(request.SlotId))
				.ReturnsAsync(FakeDataFactory.CreateFakeTimeSlot(request.SlotId));

			_tutorAvailabilityRepositoryMock
				.Setup(r => r.GetByTutorIdAsync(request.TutorId))
				.ReturnsAsync(new List<TutorAvailability>());

			_scheduleRepositoryMock
				.Setup(r => r.HasTutorScheduleOnSlotDateAsync(request.TutorId, request.SlotId, request.StartDate.Date))
				.ReturnsAsync(false);
		}

		int entityId = 1;
		_tutorAvailabilityRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<TutorAvailability>()))
			.Returns(Task.CompletedTask)
			.Callback<TutorAvailability>(ta => ta.Id = entityId++);

		// Act
		var result = await _service.CreateBulkAsync(requests);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
		_tutorAvailabilityRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TutorAvailability>()), Times.Exactly(count));
	}

	/// <summary>
	/// Test CreateBulkAsync với request không hợp lệ - ném InvalidOperationException khi tutor không tồn tại
	/// </summary>
	[Test]
	public void CreateBulkAsync_WithInvalidRequest_ThrowsInvalidOperationException()
	{
		// Arrange
		var requests = new List<TutorAvailabilityCreateRequest>
		{
			new TutorAvailabilityCreateRequest
			{
				TutorId = 999, // Non-existent tutor
				SlotId = 1,
				StartDate = DateTime.UtcNow.Date
			}
		};

		_tutorProfileRepositoryMock
			.Setup(r => r.GetByIdFullAsync(999))
			.ReturnsAsync((TutorProfile?)null);

		// Act & Assert
		Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.CreateBulkAsync(requests));
	}

	/// <summary>
	/// Test UpdateStatusAsync với các trạng thái khác nhau - cập nhật thành công trạng thái
	/// </summary>
	[Test]
	[TestCase(TutorAvailabilityStatus.Available)]
	[TestCase(TutorAvailabilityStatus.Booked)]
	[TestCase(TutorAvailabilityStatus.InProgress)]
	[TestCase(TutorAvailabilityStatus.Cancelled)]
	public async Task UpdateStatusAsync_WithDifferentStatuses_UpdatesSuccessfully(TutorAvailabilityStatus status)
	{
		// Arrange
		var entity = FakeDataFactory.CreateFakeTutorAvailability(1);
		_tutorAvailabilityRepositoryMock
			.Setup(r => r.GetByIdFullAsync(1))
			.ReturnsAsync(entity);

		_tutorAvailabilityRepositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<TutorAvailability>()))
			.Returns(Task.CompletedTask);

		// Act
		var result = await _service.UpdateStatusAsync(1, status);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Status, Is.EqualTo(status));
		_tutorAvailabilityRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TutorAvailability>()), Times.Once);
	}

	/// <summary>
	/// Test UpdateStatusAsync khi entity không tồn tại - ném ArgumentException
	/// </summary>
	[Test]
	[TestCase(999)]
	public void UpdateStatusAsync_WhenEntityNotFound_ThrowsArgumentException(int id)
	{
		// Arrange
		_tutorAvailabilityRepositoryMock
			.Setup(r => r.GetByIdFullAsync(id))
			.ReturnsAsync((TutorAvailability?)null);

		// Act & Assert
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateStatusAsync(id, TutorAvailabilityStatus.Available));
		Assert.That(exception.Message, Does.Contain($"Tutor availability with ID {id} not found"));
	}

	/// <summary>
	/// Test UpdateStatusAsync với ID không hợp lệ - ném ArgumentException
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(-1)]
	public void UpdateStatusAsync_WithInvalidId_ThrowsArgumentException(int id)
	{
		// Act & Assert
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.UpdateStatusAsync(id, TutorAvailabilityStatus.Available));
		Assert.That(exception.Message, Does.Contain("ID must be greater than 0"));
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
		_tutorAvailabilityRepositoryMock
			.Setup(r => r.RemoveByIdAsync(id))
			.Returns(Task.CompletedTask);

		// Act
		await _service.DeleteAsync(id);

		// Assert
		_tutorAvailabilityRepositoryMock.Verify(r => r.RemoveByIdAsync(id), Times.Once);
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

