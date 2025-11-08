using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.Level;
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
/// Test class cho LevelService
/// </summary>
public class LevelServiceTests
{
	private Mock<ILevelRepository> _levelRepositoryMock;
	private IMapper _mapper;
	private LevelService _service;

	/// <summary>
	/// Khởi tạo các mock objects và service trước mỗi test
	/// </summary>
	[SetUp]
	public void Setup()
	{
		_levelRepositoryMock = new Mock<ILevelRepository>();

		var config = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<MappingProfile>();
		});
		_mapper = config.CreateMapper();

		_service = new LevelService(
			_levelRepositoryMock.Object,
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
		Level? entity = shouldExist ? FakeDataFactory.CreateFakeLevel(id) : null;

		_levelRepositoryMock
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
	/// Test GetAllAsync với số lượng khác nhau - trả về tất cả level
	/// </summary>
	[Test]
	[TestCase(0)]
	[TestCase(1)]
	[TestCase(5)]
	public async Task GetAllAsync_WithDifferentCounts_ReturnsExpectedList(int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeLevel(i))
			.ToList();

		_levelRepositoryMock
			.Setup(r => r.GetAllAsync())
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetAllAsync();

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
	}

	/// <summary>
	/// Test GetByNameAsync với các tên khác nhau - trả về danh sách đúng tên
	/// </summary>
	[Test]
	[TestCase("Elementary", 1)]
	[TestCase("High School", 2)]
	[TestCase("Nonexistent", 0)]
	public async Task GetByNameAsync_WithDifferentNames_ReturnsExpectedList(string name, int count)
	{
		// Arrange
		var entities = Enumerable.Range(1, count)
			.Select(i => FakeDataFactory.CreateFakeLevel(i))
			.ToList();
		foreach (var entity in entities)
		{
			entity.Name = name;
		}

		_levelRepositoryMock
			.Setup(r => r.GetByNameAsync(name))
			.ReturnsAsync(entities);

		// Act
		var result = await _service.GetByNameAsync(name);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result.Count, Is.EqualTo(count));
		if (count > 0)
		{
			Assert.That(result.All(r => r.Name == name), Is.True);
		}
	}

	/// <summary>
	/// Test GetByNameAsync với tên không hợp lệ - ném ArgumentException
	/// </summary>
	[Test]
	[TestCase("")]
	[TestCase("   ")]
	[TestCase(null)]
	public void GetByNameAsync_WithInvalidName_ThrowsArgumentException(string? name)
	{
		// Act & Assert
		var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.GetByNameAsync(name!));
		Assert.That(exception.Message, Does.Contain("Name is required"));
	}

	/// <summary>
	/// Tạo các test cases cho CreateAsync - bao phủ các kịch bản: valid, validation failed
	/// </summary>
	private static IEnumerable<TestCaseData> CreateAsyncTestCases()
	{
		// Valid case
		yield return new TestCaseData(
			new LevelCreateRequest
			{
				Name = "Elementary"
			},
			true,
			null
		).SetName("CreateAsync_ValidRequest_ReturnsCreatedDto");

		// Empty name (validation will fail)
		yield return new TestCaseData(
			new LevelCreateRequest
			{
				Name = ""
			},
			false,
			typeof(InvalidOperationException)
		).SetName("CreateAsync_EmptyName_ThrowsInvalidOperationException");
	}

	/// <summary>
	/// Test CreateAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
	/// </summary>
	[Test]
	[TestCaseSource(nameof(CreateAsyncTestCases))]
	public async Task CreateAsync_WithVariousScenarios_HandlesCorrectly(
		LevelCreateRequest request,
		bool shouldSucceed,
		Type? expectedExceptionType)
	{
		// Arrange
		Level? capturedEntity = null;
		_levelRepositoryMock
			.Setup(r => r.AddAsync(It.IsAny<Level>()))
			.Returns(Task.CompletedTask)
			.Callback<Level>(l =>
			{
				l.Id = 1;
				capturedEntity = l;
			});

		// Act & Assert
		if (shouldSucceed)
		{
			var result = await _service.CreateAsync(request);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.Name, Is.EqualTo(request.Name));
			_levelRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Level>()), Times.Once);
		}
		else
		{
			var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.CreateAsync(request));
			Assert.That(exception, Is.InstanceOf(expectedExceptionType!));
		}
	}

	/// <summary>
	/// Tạo các test cases cho UpdateAsync - bao phủ các kịch bản: valid update, entity not found, validation failed
	/// </summary>
	private static IEnumerable<TestCaseData> UpdateAsyncTestCases()
	{
		// Valid update
		yield return new TestCaseData(
			new LevelUpdateRequest
			{
				Id = 1,
				Name = "Updated Level"
			},
			FakeDataFactory.CreateFakeLevel(1),
			true,
			null
		).SetName("UpdateAsync_ValidRequest_ReturnsUpdatedDto");

		// Entity not found
		yield return new TestCaseData(
			new LevelUpdateRequest
			{
				Id = 999,
				Name = "Updated Level"
			},
			null,
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_EntityNotFound_ThrowsInvalidOperationException");

		// Empty name (validation will fail)
		yield return new TestCaseData(
			new LevelUpdateRequest
			{
				Id = 1,
				Name = ""
			},
			FakeDataFactory.CreateFakeLevel(1),
			false,
			typeof(InvalidOperationException)
		).SetName("UpdateAsync_EmptyName_ThrowsInvalidOperationException");
	}

	/// <summary>
	/// Test UpdateAsync với nhiều kịch bản khác nhau - bao phủ success và các trường hợp lỗi
	/// </summary>
	[Test]
	[TestCaseSource(nameof(UpdateAsyncTestCases))]
	public async Task UpdateAsync_WithVariousScenarios_HandlesCorrectly(
		LevelUpdateRequest request,
		Level? existingEntity,
		bool shouldSucceed,
		Type? expectedExceptionType)
	{
		// Arrange
		if (existingEntity != null)
		{
			_levelRepositoryMock
				.Setup(r => r.GetByIdAsync(request.Id))
				.ReturnsAsync(existingEntity);
		}
		else
		{
			_levelRepositoryMock
				.Setup(r => r.GetByIdAsync(request.Id))
				.ReturnsAsync((Level?)null);
		}

		_levelRepositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<Level>()))
			.Returns(Task.CompletedTask);

		// Act & Assert
		if (shouldSucceed)
		{
			var result = await _service.UpdateAsync(request);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.Id, Is.EqualTo(request.Id));
			Assert.That(result.Name, Is.EqualTo(request.Name));
			_levelRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Level>()), Times.Once);
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
		_levelRepositoryMock
			.Setup(r => r.RemoveByIdAsync(id))
			.Returns(Task.CompletedTask);

		// Act
		await _service.DeleteAsync(id);

		// Assert
		_levelRepositoryMock.Verify(r => r.RemoveByIdAsync(id), Times.Once);
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

