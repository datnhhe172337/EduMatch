using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Moq;
using Xunit;

namespace EduMatch.UnitTests
{
    public class TutorFeedbackServiceUnitTests
    {
        private readonly Mock<ITutorFeedbackRepository> _repoMock;
        private readonly Mock<ITutorRatingSummaryService> _summaryMock;
        private readonly TutorFeedbackService _service;

        public TutorFeedbackServiceUnitTests()
        {
            _repoMock = new Mock<ITutorFeedbackRepository>();
            _summaryMock = new Mock<ITutorRatingSummaryService>();
            _service = new TutorFeedbackService(_repoMock.Object, _summaryMock.Object);
        }

        // --------------------- CREATE FEEDBACK -----------------------

        [Fact]
        public async Task CreateFeedbackAsync_ShouldThrow_WhenExistingFeedbackFound_UnitTest()
        {
            // Arrange
            var req = CreateRequest();
            _repoMock.Setup(r => r.GetFeedbackByBookingAsync(req.BookingId, "test@mail.com", req.TutorId))
                     .ReturnsAsync(new TutorFeedback());

            // Act + Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateFeedbackAsync(req, "test@mail.com"));

            Assert.Equal("Bạn đã đánh giá buổi học này rồi.", ex.Message);
        }

        [Fact]
        public async Task CreateFeedbackAsync_ShouldThrow_WhenCompletedBelow80Percent_UnitTest()
        {
            // Arrange
            var req = CreateRequest();

            _repoMock.Setup(r => r.GetFeedbackByBookingAsync(req.BookingId, "test@mail.com", req.TutorId))
                     .ReturnsAsync((TutorFeedback)null);

            _repoMock.Setup(r => r.CountCompletedSessionsAsync(req.BookingId)).ReturnsAsync(1);
            _repoMock.Setup(r => r.GetTotalSessionsAsync(req.BookingId)).ReturnsAsync(5); // 20% < 80%

            // Act + Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateFeedbackAsync(req, "test@mail.com"));

            Assert.Equal("Bạn chỉ có thể đánh giá sau khi hoàn thành ít nhất 80% số buổi học.", ex.Message);
        }

        [Fact]
        public async Task CreateFeedbackAsync_ShouldCreateFeedbackSuccessfully_UnitTest()
        {
            // Arrange
            var req = CreateRequest();
            _repoMock.Setup(r => r.GetFeedbackByBookingAsync(req.BookingId, "test@mail.com", req.TutorId))
                     .ReturnsAsync((TutorFeedback)null);

            _repoMock.Setup(r => r.CountCompletedSessionsAsync(req.BookingId)).ReturnsAsync(4);
            _repoMock.Setup(r => r.GetTotalSessionsAsync(req.BookingId)).ReturnsAsync(5); // 80%

            TutorFeedback savedEntity = null;

            _repoMock.Setup(r => r.AddFeedbackAsync(It.IsAny<TutorFeedback>()))
                     .Callback<TutorFeedback>(f =>
                     {
                         f.Id = 999;
                         savedEntity = f;
                     })
                     .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.AddFeedbackDetailRangeAsync(It.IsAny<List<TutorFeedbackDetail>>()))
                     .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.GetByIdIncludeDetailsAsync(999))
                     .ReturnsAsync(new TutorFeedback
                     {
                         Id = 999,
                         BookingId = req.BookingId,
                         TutorId = req.TutorId,
                         LearnerEmail = "test@mail.com",
                         Comment = req.Comment,
                         OverallRating = 4.5,
                         CreatedAt = DateTime.UtcNow,
                         TutorFeedbackDetails = new List<TutorFeedbackDetail>
                         {
                             new TutorFeedbackDetail { CriterionId = 1, Rating = 4, Criterion = new FeedbackCriterion{ Name="Kỹ năng"} }
                         }
                     });

            // Act
            var result = await _service.CreateFeedbackAsync(req, "test@mail.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(999, result.Id);
            Assert.Single(result.FeedbackDetails);
            _summaryMock.Verify(s => s.EnsureAndUpdateSummaryAsync(req.TutorId), Times.Once);
        }

        // --------------------- GET ALL CRITERIA -----------------------

        [Fact]
        public async Task GetAllCriteriaAsync_ShouldReturnList_UnitTest()
        {
            _repoMock.Setup(r => r.GetAllCriteriaAsync()).ReturnsAsync(new List<FeedbackCriterion>
            {
                new FeedbackCriterion { Id = 1, Name = "Skill" }
            });

            var result = await _service.GetAllCriteriaAsync();

            Assert.Single(result);
            Assert.Equal("Skill", result[0].Name);
        }

        // --------------------- GET ALL FEEDBACKS -----------------------

        [Fact]
        public async Task GetAllFeedbacksAsync_ShouldReturnMappedList_UnitTest()
        {
            _repoMock.Setup(r => r.GetAllFeedbacksAsync()).ReturnsAsync(new List<TutorFeedback>
            {
                new TutorFeedback
                {
                    Id = 10,
                    BookingId = 1,
                    TutorId = 2,
                    LearnerEmail="aaa@mail.com",
                    OverallRating=5,
                    CreatedAt=DateTime.UtcNow,
                    TutorFeedbackDetails = new List<TutorFeedbackDetail>
                    {
                        new TutorFeedbackDetail { CriterionId=1, Rating=5 }
                    }
                }
            });

            var result = await _service.GetAllFeedbacksAsync();

            Assert.Single(result);
            Assert.Equal(10, result[0].Id);
            Assert.Single(result[0].FeedbackDetails);
        }

        // --------------------- GET FEEDBACK BY ID -----------------------

        [Fact]
        public async Task GetFeedbackByIdAsync_ShouldThrow_WhenNotFound_UnitTest()
        {
            _repoMock.Setup(r => r.GetByIdIncludeDetailsAsync(99))
                     .ReturnsAsync((TutorFeedback)null);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.GetFeedbackByIdAsync(99));

            Assert.Equal("Không tìm thấy feedback", ex.Message);
        }

        [Fact]
        public async Task GetFeedbackByIdAsync_ShouldReturnDto_WhenFound_UnitTest()
        {
            _repoMock.Setup(r => r.GetByIdIncludeDetailsAsync(10))
                     .ReturnsAsync(new TutorFeedback
                     {
                         Id = 10,
                         BookingId = 1,
                         TutorId = 2,
                         LearnerEmail = "a@mail.com",
                         OverallRating = 3,
                         CreatedAt = DateTime.UtcNow,
                         TutorFeedbackDetails = new List<TutorFeedbackDetail>
                         {
                             new TutorFeedbackDetail{ CriterionId =1, Rating=3 }
                         }
                     });

            var result = await _service.GetFeedbackByIdAsync(10);

            Assert.Equal(10, result.Id);
            Assert.Single(result.FeedbackDetails);
        }

        // --------------------- GET FEEDBACK BY LEARNER EMAIL -----------------------

        [Fact]
        public async Task GetFeedbackByLearnerEmailAsync_ShouldReturnList_UnitTest()
        {
            _repoMock.Setup(r => r.GetFeedbackByLearnerEmailAsync("a@mail.com"))
                     .ReturnsAsync(new List<TutorFeedback>
                     {
                          new TutorFeedback
                          {
                              Id=1,
                              BookingId=1,
                              TutorId=2,
                              LearnerEmail="a@mail.com",
                              OverallRating=4,
                              TutorFeedbackDetails = new List<TutorFeedbackDetail>
                              {
                                  new TutorFeedbackDetail{ CriterionId=1, Rating=4 }
                              }
                          }
                     });

            var result = await _service.GetFeedbackByLearnerEmailAsync("a@mail.com");

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        // --------------------- GET FEEDBACK BY TUTOR ID -----------------------

        [Fact]
        public async Task GetFeedbackByTutorIdAsync_ShouldReturnList_UnitTest()
        {
            _repoMock.Setup(r => r.GetFeedbackByTutorIdAsync(5))
                     .ReturnsAsync(new List<TutorFeedback>
                     {
                          new TutorFeedback
                          {
                              Id=2,
                              BookingId=3,
                              TutorId=5,
                              LearnerEmail="a@mail.com",
                              OverallRating=4,
                              TutorFeedbackDetails = new List<TutorFeedbackDetail>
                              {
                                  new TutorFeedbackDetail{ CriterionId=1, Rating=4 }
                              }
                          }
                     });

            var result = await _service.GetFeedbackByTutorIdAsync(5);

            Assert.Single(result);
            Assert.Equal(2, result[0].Id);
        }

        // --------------------- UPDATE FEEDBACK -----------------------

        [Fact]
        public async Task UpdateFeedbackAsync_ShouldThrow_WhenFeedbackNotFound_UnitTest()
        {
            var req = UpdateRequest();

            _repoMock.Setup(r => r.GetFeedbackByBookingAsync(req.BookingId, "a@mail.com", req.TutorId))
                     .ReturnsAsync((TutorFeedback)null);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateFeedbackAsync(req, "a@mail.com"));

            Assert.Equal("Không tìm thấy feedback", ex.Message);
        }

        [Fact]
        public async Task UpdateFeedbackAsync_ShouldThrow_WhenUpdatedBeforeOrExpired_UnitTest()
        {
            var req = UpdateRequest();

            _repoMock.Setup(r => r.GetFeedbackByBookingAsync(req.BookingId, "a@mail.com", req.TutorId))
                     .ReturnsAsync(new TutorFeedback
                     {
                         UpdatedAt = DateTime.UtcNow
                     });

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.UpdateFeedbackAsync(req, "a@mail.com"));

            Assert.Equal("Bạn không được phép cập nhật feedback nữa", ex.Message);
        }

        [Fact]
        public async Task UpdateFeedbackAsync_ShouldUpdateSuccessfully_UnitTest()
        {
            var req = UpdateRequest();

            var entity = new TutorFeedback
            {
                Id = 88,
                BookingId = req.BookingId,
                TutorId = req.TutorId,
                LearnerEmail = "a@mail.com",
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                TutorFeedbackDetails = new List<TutorFeedbackDetail>
                {
                    new TutorFeedbackDetail{ CriterionId=1, Rating=3 }
                }
            };

            _repoMock.Setup(r => r.GetFeedbackByBookingAsync(req.BookingId, "a@mail.com", req.TutorId))
                     .ReturnsAsync(entity);

            _repoMock.Setup(r => r.RemoveFeedbackDetailsAsync(It.IsAny<List<TutorFeedbackDetail>>()))
                     .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.UpdateFeedbackAsync(It.IsAny<TutorFeedback>()))
                     .Returns(Task.CompletedTask);

            var result = await _service.UpdateFeedbackAsync(req, "a@mail.com");

            Assert.Equal(88, result.Id);
            Assert.Single(result.FeedbackDetails);
            _summaryMock.Verify(s => s.EnsureAndUpdateSummaryAsync(req.TutorId), Times.Once);
        }

        // ---------- Helpers -----------

        private CreateTutorFeedbackRequest CreateRequest() =>
        new CreateTutorFeedbackRequest
        {
            BookingId = 10,
            TutorId = 20,
            Comment = "Great",
            FeedbackDetails = new List<CreateTutorFeedbackDetailRequest>
            {
                new CreateTutorFeedbackDetailRequest { CriterionId = 1, Rating = 5 },
                new CreateTutorFeedbackDetailRequest { CriterionId = 2, Rating = 4 }
            }
        };


        private UpdateTutorFeedbackRequest UpdateRequest() =>
            new UpdateTutorFeedbackRequest
            {
                BookingId = 10,
                TutorId = 20,
                Comment = "Updated",
                FeedbackDetails = new List<TutorFeedbackDetailDto>
                {
                    new TutorFeedbackDetailDto { CriterionId = 1, Rating = 4 }
                }
            };
    }
}
