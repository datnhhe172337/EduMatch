using Xunit;
using Moq;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;

namespace EduMatch.Tests.Services
{
    public class FindTutorServiceTests
    {
        private readonly Mock<IFindTutorRepository> _mockRepo;
        private readonly IFindTutorService _service;

        public FindTutorServiceTests()
        {
            _mockRepo = new Mock<IFindTutorRepository>();

            // Create the service instance, injecting the fake (mocked) repository
            _service = new FindTutorService(_mockRepo.Object);
        }

        #region GetAllTutorsAsync Tests

        [Fact]
        public async Task GetAllTutorsAsync_ShouldReturnAllTutors_WhenCalled()
        {
            // Arrange
            var fakeTutors = new List<TutorProfile>
            {
                new TutorProfile { Id = 1, Bio = "Test Tutor 1" },
                new TutorProfile { Id = 2, Bio = "Test Tutor 2" }
            };

            _mockRepo.Setup(r => r.GetAllTutorsAsync())
                .Returns(Task.FromResult(fakeTutors as IEnumerable<TutorProfile>)); 

            // Act
            var result = await _service.GetAllTutorsAsync();

            // Assert
            result.Should().BeEquivalentTo(fakeTutors);
            result.Should().HaveCount(2);
            _mockRepo.Verify(r => r.GetAllTutorsAsync(), Times.Once); 
        }

        [Fact]
        public async Task GetAllTutorsAsync_ShouldReturnEmptyList_WhenRepoReturnsEmpty()
        {
            // Arrange
            var emptyTutors = new List<TutorProfile>();
            _mockRepo.Setup(r => r.GetAllTutorsAsync())
                .Returns(Task.FromResult(emptyTutors as IEnumerable<TutorProfile>));

            // Act
            var result = await _service.GetAllTutorsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllTutorsAsync_ShouldPropagateException_WhenRepoThrowsException()
        {
            // Arrange
            var dbException = new InvalidOperationException("Database connection lost");
            _mockRepo.Setup(r => r.GetAllTutorsAsync()).ThrowsAsync(dbException);

            // Act
            Func<Task> act = async () => await _service.GetAllTutorsAsync();

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database connection lost");
        }

        #endregion

        #region SearchTutorsAsync Tests

        [Fact]
        public async Task SearchTutorsAsync_ShouldCallRepository_WithCorrectParameters()
        {
            // Arrange
            var filter = new TutorFilterDto
            {
                Keyword = "Math",
                Gender = Gender.Male,
                City = 1,
                TeachingMode = TeachingMode.Online,
                StatusId = TutorStatus.Approved,
                Page = 2,
                PageSize = 10
            };

            var fakeSearchResult = new List<TutorProfile>
            {
                new TutorProfile { Id = 3, Bio = "Math Tutor" }
            };

            _mockRepo.Setup(r => r.SearchTutorsAsync(
                filter.Keyword,
                filter.Gender,
                filter.City,
                filter.TeachingMode,
                filter.StatusId,
                filter.Page,
                filter.PageSize
            )).Returns(Task.FromResult(fakeSearchResult as IEnumerable<TutorProfile>));

            // Act
            var result = await _service.SearchTutorsAsync(filter);

            // Assert
            result.Should().BeEquivalentTo(fakeSearchResult);
            _mockRepo.Verify(r => r.SearchTutorsAsync(
                "Math", Gender.Male,1,TeachingMode.Online,TutorStatus.Approved,2, 10 ), Times.Once);
        }

        [Fact]
        public async Task SearchTutorsAsync_ShouldReturnEmptyList_WhenRepoReturnsEmpty()
        {
            // Arrange
            var filter = new TutorFilterDto { Keyword = "History" };
            var emptyTutors = new List<TutorProfile>();
            _mockRepo.Setup(r => r.SearchTutorsAsync(
                It.IsAny<string>(), It.IsAny<Gender?>(), It.IsAny<int?>(),
                It.IsAny<TeachingMode?>(), It.IsAny<TutorStatus?>(),
                It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(emptyTutors as IEnumerable<TutorProfile>));

            // Act
            var result = await _service.SearchTutorsAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchTutorsAsync_ShouldPropagateException_WhenRepoThrowsException()
        {
            // Arrange
            var filter = new TutorFilterDto { Keyword = "Test" };
            var dbException = new InvalidOperationException("Search index failed");
            _mockRepo.Setup(r => r.SearchTutorsAsync(
                It.IsAny<string>(), It.IsAny<Gender?>(), It.IsAny<int?>(),
                It.IsAny<TeachingMode?>(), It.IsAny<TutorStatus?>(),
                It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(dbException);

            // Act
            Func<Task> act = async () => await _service.SearchTutorsAsync(filter);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Search index failed");
        }

        [Fact]
        public async Task SearchTutorsAsync_ShouldThrowArgumentNullException_WhenFilterIsNull()
        {
            // Arrange
            TutorFilterDto filter = null;

            // Act
            Func<Task> act = async () => await _service.SearchTutorsAsync(filter);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("filter");
        }

        #endregion
    }
}
