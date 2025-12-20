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
        private readonly Mock<ITutorSubjectRepository> _tutorSubjectRepo = new();
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<ISubjectRepository> _subjectRepo = new();

		private LearnerTrialLessonService CreateService() => new(_repo.Object, _tutorSubjectRepo.Object, _userRepo.Object, _subjectRepo.Object);


        #region HasTrialedAsync
        [Fact]
        public async Task HasTrialedAsync_ForwardsToRepository()
        {
            _repo.Setup(r => r.ExistsAsync("a@test.com", 1, 2)).ReturnsAsync(true);

            var service = CreateService();
            var result = await service.HasTrialedAsync("a@test.com", 1, 2);

            Assert.True(result);
            _repo.Verify(r => r.ExistsAsync("a@test.com", 1, 2), Times.Once);
        }
        #endregion

        #region RecordTrialAsync
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
        #endregion

        #region GetSubjectTrialStatusesAsync
        [Fact]
        public async Task GetSubjectTrialStatusesAsync_ReturnsHasTrialedFlags()
        {
            _tutorSubjectRepo.Setup(r => r.GetByTutorIdAsync(10)).ReturnsAsync(new[]
            {
                new TutorSubject { SubjectId = 1, Subject = new Subject { SubjectName = "Math" } },
                new TutorSubject { SubjectId = 2, Subject = new Subject { SubjectName = "Physics" } }
            });
            _repo.Setup(r => r.GetByLearnerAndTutorAsync("l@test.com", 10)).ReturnsAsync(new[]
            {
                new LearnerTrialLesson { SubjectId = 2 }
            });

            var service = CreateService();
            var result = await service.GetSubjectTrialStatusesAsync("l@test.com", 10);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.SubjectId == 1 && r.HasTrialed == false);
            Assert.Contains(result, r => r.SubjectId == 2 && r.HasTrialed == true);
        }
        #endregion
    }
}
