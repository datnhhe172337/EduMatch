using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.TimeSlot;
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
/// Test class cho TimeSlotService
/// </summary>
public class TimeSlotServiceTests
{
	private Mock<ITimeSlotRepository> _timeSlotRepositoryMock;
	private IMapper _mapper;
	private TimeSlotService _service;

	/// <summary>
	/// Khởi tạo các mock objects và service trước mỗi test
	/// </summary>
	[SetUp]
	public void Setup()
	{
		_timeSlotRepositoryMock = new Mock<ITimeSlotRepository>();

		var config = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<MappingProfile>();
		});
		_mapper = config.CreateMapper();

		_service = new TimeSlotService(
			_timeSlotRepositoryMock.Object,
			_mapper
		);
	}

	/// <summary>
	/// Test GetByIdAsync với các ID khác nhau - trả về DTO khi tồn tại, null khi không tồn tại
	/// </summary>
	[Test]
	[TestCase(1, true)]
	[TestCase(999, false)]
	public async Task GetByIdAsync_WithDifferentIds_ReturnsExpectedResult(int id, bool shouldExist)
	{
		// Arrange
		TimeSlot? entity = shouldExist ? FakeDataFactory.CreateFakeTimeSlot(id) : null;

		_timeSlotRepositoryMock
			.Setup(r => r.GetByIdAsync(id))
			.ReturnsAsync(entity);

		// Act
		var result = await _service.GetByIdAsync(id);

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
	/// Test GetByIdAsync với ID không hợp lệ - ném ArgumentException
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(-1)]
	public void GetByIdAsync_WithInvalidId_ThrowsArgumentException(int id)
	{
		// Act & Assert
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetByIdAsync(id));
		Assert.That(exception.Message, Does.Contain("ID must be greater than 0"));
	}

	/// <summary>
	/// Test GetAllAsync với số lượng khác nhau - trả về tất cả time slot
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(5)]
	public async Task GetAllAsync_WithDifferentCounts_ReturnsExpectedList(int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTimeSlot(i, new TimeOnly(8 + i, 0), new TimeOnly(10 + i, 0)))
			.ToList();

		_timeSlotRepositoryMock
			.Setup(r => r.GetAllAsync())
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetAllAsync();

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
	}

	/// <summary>
	/// Test GetByTimeRangeAsync với các khoảng thời gian khác nhau - trả về danh sách đúng khoảng
	/// </summary>
	[Test]
	[TestCase(8, 10, 2)]
	[TestCase(10, 12, 1)]
	[TestCase(14, 16, 0)]
	public async Task GetByTimeRangeAsync_WithDifferentRanges_ReturnsExpectedList(int startHour, int endHour, int count)
	{
		// Arrange
		var startTime = new TimeOnly(startHour, 0);
		var endTime = new TimeOnly(endHour, 0);
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeTimeSlot(i, startTime, endTime))
			.ToList();

		_timeSlotRepositoryMock
			.Setup(r => r.GetByTimeRangeAsync(startTime, endTime))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetByTimeRangeAsync(startTime, endTime);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
	}

	/// <summary>
	/// Test GetByExactTimeAsync với các thời gian khác nhau - trả về DTO khi tồn tại, null khi không tồn tại
	/// </summary>
	[Test]
	[TestCase(8, 10, true)]
	[TestCase(14, 16, false)]
	public async Task GetByExactTimeAsync_WithDifferentTimes_ReturnsExpectedResult(int startHour, int endHour, bool shouldExist)
	{
		// Arrange
		var startTime = new TimeOnly(startHour, 0);
		var endTime = new TimeOnly(endHour, 0);
		TimeSlot? entity = shouldExist ? FakeDataFactory.CreateFakeTimeSlot(1, startTime, endTime) : null;

		_timeSlotRepositoryMock
			.Setup(r => r.GetByExactTimeAsync(startTime, endTime))
			.ReturnsAsync(entity);

		// Act
		var result = await _service.GetByExactTimeAsync(startTime, endTime);

		// Assert
		if (shouldExist)
		{
			Assert.That(result, Is.Not.Null);
			Assert.That(result!.StartTime, Is.EqualTo(startTime));
			Assert.That(result.EndTime, Is.EqualTo(endTime));
		}
		else
		{
			Assert.That(result, Is.Null);
		}
	}

	/// <summary>
	/// Tạo các test cases cho CreateAsync - bao phủ các kịch bản: valid, duplicate time slot
	/// </summary>
	private static IEnumerable<TestCaseData> CreateAsyncTestCases()
	{
		// Valid case
		yield return new TestCaseData(
			new TimeSlotCreateRequest
			{
				StartTime = new TimeOnly(8, 0),
				EndTime = new TimeOnly(10, 0)
			},
			null,
			true,
			null
		).SetName("CreateAsync_ValidRequest_ReturnsCreatedDto");

		// Duplicate time slot
		yield return new TestCaseData(
			new TimeSlotCreateRequest
			{
				StartTime = new TimeOnly(8, 0),
				EndTime = new TimeOnly(10, 0)
			},
			FakeDataFactory.CreateFakeTimeSlot(1, new TimeOnly(8, 0), new TimeOnly(10, 0)),
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_DuplicateTimeSlot_ThrowsInvalidOperationException");
	}

	/// <summary>
	/// Test CreateAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
	/// </summary>
	[Test]
	[TestCaseSource(nameof(CreateAsyncTestCases))]
	public async Task CreateAsync_WithVariousScenarios_HandlesCorrectly(
		TimeSlotCreateRequest request,
		TimeSlot? existingSlot,
		bool shouldSucceed,
		Type? expectedExceptionType)
	{
		// Arrange
		_timeSlotRepositoryMock
			.Setup(r => r.GetByExactTimeAsync(request.StartTime, request.EndTime))
			.ReturnsAsync(existingSlot);

		TimeSlot? capturedEntity = null;
		_timeSlotRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<TimeSlot>()))
			.Returns(Task.CompletedTask)
			.Callback<TimeSlot>(ts =>
			{
				ts.Id = 1;
				capturedEntity = ts;
			});

		// Act & Assert
		if (shouldSucceed)
		{
			var result = await _service.CreateAsync(request);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.StartTime, Is.EqualTo(request.StartTime));
			Assert.That(result.EndTime, Is.EqualTo(request.EndTime));
			_timeSlotRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TimeSlot>()), Times.Once);
		}
		else
		{
			var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.CreateAsync(request));
			Assert.That(exception, Is.InstanceOf(expectedExceptionType!));
		}
	}

	/// <summary>
	/// Tạo các test cases cho UpdateAsync - bao phủ các kịch bản: valid update, entity not found, duplicate time slot
	/// </summary>
	private static IEnumerable<TestCaseData> UpdateAsyncTestCases()
	{
		// Valid update
		yield return new TestCaseData(
			new TimeSlotUpdateRequest
			{
				Id = 1,
				StartTime = new TimeOnly(9, 0),
				EndTime = new TimeOnly(11, 0)
			},
			FakeDataFactory.CreateFakeTimeSlot(1, new TimeOnly(8, 0), new TimeOnly(10, 0)),
			null,
			true,
			null
		).SetName("UpdateAsync_ValidRequest_ReturnsUpdatedDto");

		// Entity not found
		yield return new TestCaseData(
			new TimeSlotUpdateRequest
			{
				Id = 999,
				StartTime = new TimeOnly(9, 0),
				EndTime = new TimeOnly(11, 0)
			},
			null,
			null,
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_EntityNotFound_ThrowsInvalidOperationException");

		// Duplicate time slot (different ID)
		var existingEntity = FakeDataFactory.CreateFakeTimeSlot(1, new TimeOnly(8, 0), new TimeOnly(10, 0));
		var duplicateSlot = FakeDataFactory.CreateFakeTimeSlot(2, new TimeOnly(9, 0), new TimeOnly(11, 0));
		yield return new TestCaseData(
			new TimeSlotUpdateRequest
			{
				Id = 1,
				StartTime = new TimeOnly(9, 0),
				EndTime = new TimeOnly(11, 0)
			},
			existingEntity,
			duplicateSlot,
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_DuplicateTimeSlot_ThrowsInvalidOperationException");
	}

	/// <summary>
	/// Test UpdateAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
	/// </summary>
	[Test]
	[TestCaseSource(nameof(UpdateAsyncTestCases))]
	public async Task UpdateAsync_WithVariousScenarios_HandlesCorrectly(
		TimeSlotUpdateRequest request,
		TimeSlot? existingEntity,
		TimeSlot? duplicateSlot,
		bool shouldSucceed,
		Type? expectedExceptionType)
	{
		// Arrange
		if (existingEntity != null)
		{
			_timeSlotRepositoryMock
				.Setup(r => r.GetByIdAsync(request.Id))
				.ReturnsAsync(existingEntity);
		}
		else
		{
			_timeSlotRepositoryMock
				.Setup(r => r.GetByIdAsync(request.Id))
				.ReturnsAsync((TimeSlot?)null);
		}

		_timeSlotRepositoryMock
			.Setup(r => r.GetByExactTimeAsync(request.StartTime, request.EndTime))
			.ReturnsAsync(duplicateSlot);

		_timeSlotRepositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<TimeSlot>()))
			.Returns(Task.CompletedTask);

		// Act & Assert
		if (shouldSucceed)
		{
			var result = await _service.UpdateAsync(request);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.Id, Is.EqualTo(request.Id));
			Assert.That(result.StartTime, Is.EqualTo(request.StartTime));
			Assert.That(result.EndTime, Is.EqualTo(request.EndTime));
			_timeSlotRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TimeSlot>()), Times.Once);
		}
		else
		{
			var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.UpdateAsync(request));
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
		_timeSlotRepositoryMock
			.Setup(r => r.RemoveByIdAsync(id))
			.Returns(Task.CompletedTask);

		// Act
		await _service.DeleteAsync(id);

		// Assert
		_timeSlotRepositoryMock.Verify(r => r.RemoveByIdAsync(id), Times.Once);
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

