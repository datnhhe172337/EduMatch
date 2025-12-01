using System.Threading.Tasks;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Moq;
using Xunit;

namespace EduMatch.UnitTests
{
    public class LearnerTrialLessonServiceTests
    {
        private readonly Mock<ILearnerTrialLessonRepository> _repo = new();

        private LearnerTrialLessonService CreateService() => new(_repo.Object);

        [Fact]
        public async Task HasTrialedAsync_ForwardsToRepository()
        {
            _repo.Setup(r => r.ExistsAsync("a@test.com", 1, 2)).ReturnsAsync(true);

            var service = CreateService();
            var result = await service.HasTrialedAsync("a@test.com", 1, 2);

            Assert.True(result);
            _repo.Verify(r => r.ExistsAsync("a@test.com", 1, 2), Times.Once);
        }

        [Fact]
        public async Task RecordTrialAsync_WhenAlreadyExists_ReturnsFalse_DoesNotAdd()
        {
            _repo.Setup(r => r.ExistsAsync("b@test.com", 2, 3)).ReturnsAsync(true);

            var service = CreateService();
            var result = await service.RecordTrialAsync("b@test.com", 2, 3);

            Assert.False(result);
            _repo.Verify(r => r.ExistsAsync("b@test.com", 2, 3), Times.Once);
            _repo.Verify(r => r.AddAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task RecordTrialAsync_WhenNotExists_AddsAndReturnsTrue()
        {
            _repo.Setup(r => r.ExistsAsync("c@test.com", 4, 5)).ReturnsAsync(false);
            _repo.Setup(r => r.AddAsync("c@test.com", 4, 5)).ReturnsAsync(new LearnerTrialLesson());

            var service = CreateService();
            var result = await service.RecordTrialAsync("c@test.com", 4, 5);

            Assert.True(result);
            _repo.Verify(r => r.ExistsAsync("c@test.com", 4, 5), Times.Once);
            _repo.Verify(r => r.AddAsync("c@test.com", 4, 5), Times.Once);
        }
    }
}
