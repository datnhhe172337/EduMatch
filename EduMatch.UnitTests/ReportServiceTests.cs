using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Mappings;
using EduMatch.BusinessLogicLayer.Requests.Report;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using Moq;
using Xunit;

namespace EduMatch.UnitTests
{
    public class ReportServiceTests
    {
        private readonly Mock<IReportRepository> _reportRepo = new();
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<IReportContentValidator> _validator = new();
        private readonly Mock<INotificationService> _notifier = new();
        private readonly Mock<IReportEvidenceRepository> _evidenceRepo = new();
        private readonly Mock<IReportDefenseRepository> _defenseRepo = new();
        private readonly Mock<EmailService> _emailService = new();
		private readonly IMapper _mapper;

        public ReportServiceTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = new Mapper(config);
        }

        private ReportService CreateService()
        {
            return new ReportService(
                _reportRepo.Object,
                _userRepo.Object,
                _mapper,
                _validator.Object,
                _notifier.Object,
                _evidenceRepo.Object,
                _defenseRepo.Object,
                _emailService.Object);
        }

        [Fact]
        public async Task CreateReportAsync_CreatesReport_SavesEvidence_AndNotifiesTutor()
        {
            // Arrange
            var reporter = new User { Email = "reporter@test.com" };
            var reported = new User { Email = "tutor@test.com" };
            var createdReport = new Report { Id = 1, ReporterUserEmail = reporter.Email, ReportedUserEmail = reported.Email, CreatedAt = DateTime.UtcNow };

            _userRepo.Setup(r => r.GetUserByEmailAsync(reporter.Email)).ReturnsAsync(reporter);
            _userRepo.Setup(r => r.GetUserByEmailAsync(reported.Email)).ReturnsAsync(reported);
            _reportRepo.Setup(r => r.CreateAsync(It.IsAny<Report>())).ReturnsAsync(createdReport);
            _reportRepo.Setup(r => r.GetByIdAsync(createdReport.Id)).ReturnsAsync(createdReport);
            _evidenceRepo.Setup(r => r.AddAsync(It.IsAny<ReportEvidence>())).ReturnsAsync(new ReportEvidence { Id = 10 });
            _validator.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null)).Returns(Task.CompletedTask);

            var service = CreateService();
            var request = new ReportCreateRequest
            {
                ReportedUserEmail = reported.Email,
                Reason = "Test reason content",
                Evidences = new List<BasicEvidenceRequest>
                {
                    new BasicEvidenceRequest { MediaType = MediaType.Image, FileUrl = "https://x/img.jpg" }
                }
            };

            // Act
            var result = await service.CreateReportAsync(request, reporter.Email);

            // Assert
            _reportRepo.Verify(r => r.CreateAsync(It.IsAny<Report>()), Times.Once);
            _evidenceRepo.Verify(r => r.AddAsync(It.IsAny<ReportEvidence>()), Times.Once);
            _notifier.Verify(n => n.CreateNotificationAsync(reported.Email, It.IsAny<string>(), null), Times.Once);
            Assert.Equal(reported.Email, result.ReportedUserEmail);
            Assert.Equal(reporter.Email, result.ReporterEmail);
        }

        [Fact]
        public async Task UpdateReportAsync_BeforeTwoDays_ThrowsInvalidOperation()
        {
            var report = new Report
            {
                Id = 1,
                CreatedAt = DateTime.UtcNow,
                StatusEnum = ReportStatus.Pending
            };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);

            var service = CreateService();
            var request = new ReportUpdateRequest { Status = ReportStatus.Resolved };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateReportAsync(report.Id, request, "admin@test.com"));
        }

        [Fact]
        public async Task AddDefenseAsync_SetsUnderReview_AndNotifies()
        {
            var report = new Report
            {
                Id = 2,
                ReportedUserEmail = "tutor@test.com",
                ReporterUserEmail = "reporter@test.com",
                StatusEnum = ReportStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddHours(-12)
            };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);
            _defenseRepo.Setup(r => r.AddAsync(It.IsAny<ReportDefense>())).ReturnsAsync(new ReportDefense { Id = 20, ReportId = report.Id });
            _reportRepo.Setup(r => r.UpdateAsync(report)).ReturnsAsync(report);

            var service = CreateService();
            var request = new ReportDefenseCreateRequest
            {
                Note = "Tutor response",
                Evidences = new List<BasicEvidenceRequest>
                {
                    new BasicEvidenceRequest{ MediaType = MediaType.Image, FileUrl = "https://x/def.jpg" }
                }
            };

            var dto = await service.AddDefenseAsync(report.Id, request, report.ReportedUserEmail, false);

            Assert.Equal(ReportStatus.UnderReview, report.StatusEnum);
            _reportRepo.Verify(r => r.UpdateAsync(report), Times.Once);
            _notifier.Verify(n => n.CreateNotificationAsync(report.ReporterUserEmail, It.IsAny<string>(), null), Times.Once);
            _notifier.Verify(n => n.CreateNotificationAsync(report.ReportedUserEmail, It.IsAny<string>(), null), Times.Once);
            Assert.Equal(20, dto.Id);
        }

        [Theory]
        [InlineData(ReportStatus.Pending, -1, true)]
        [InlineData(ReportStatus.Pending, -3, false)]
        [InlineData(ReportStatus.Resolved, -1, false)]
        public async Task CanSubmitDefenseAsync_RespectsStatusAndWindow(ReportStatus status, int daysOffset, bool expected)
        {
            var report = new Report
            {
                Id = 3,
                ReportedUserEmail = "tutor@test.com",
                StatusEnum = status,
                CreatedAt = DateTime.UtcNow.AddDays(daysOffset)
            };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);

            var service = CreateService();
            var can = await service.CanSubmitDefenseAsync(report.Id, report.ReportedUserEmail, false);

            Assert.Equal(expected, can);
        }

        [Fact]
        public async Task UpdateReportAsync_AfterTwoDays_AllowsResolvedAndNotifies()
        {
            var report = new Report
            {
                Id = 4,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                StatusEnum = ReportStatus.Pending
            };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);
            _reportRepo.Setup(r => r.UpdateAsync(report)).ReturnsAsync(report);

            var service = CreateService();
            var request = new ReportUpdateRequest { Status = ReportStatus.Resolved, AdminNotes = "ok" };

            var result = await service.UpdateReportAsync(report.Id, request, "admin@test.com");

            Assert.Equal(ReportStatus.Resolved, report.StatusEnum);
            Assert.Equal(ReportStatus.Resolved, result.Status);
            _notifier.Verify(n => n.CreateNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), null), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AddDefenseAsync_WrongTutor_ThrowsUnauthorized()
        {
            var report = new Report
            {
                Id = 5,
                ReportedUserEmail = "tutor@test.com",
                ReporterUserEmail = "reporter@test.com",
                StatusEnum = ReportStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);

            var service = CreateService();
            var request = new ReportDefenseCreateRequest { Note = "response" };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.AddDefenseAsync(report.Id, request, "othertutor@test.com", false));
        }

        [Fact]
        public async Task AddEvidenceAsync_WithMismatchedDefense_Throws()
        {
            var report = new Report { Id = 6, ReporterUserEmail = "a@test.com", ReportedUserEmail = "b@test.com", StatusEnum = ReportStatus.Pending };
            var defense = new ReportDefense { Id = 99, ReportId = 999 };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);
            _defenseRepo.Setup(r => r.GetByIdAsync(defense.Id)).ReturnsAsync(defense);

            var service = CreateService();
            var req = new ReportEvidenceCreateRequest
            {
                MediaType = MediaType.Image,
                FileUrl = "https://x/file.jpg",
                DefenseId = defense.Id
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AddEvidenceAsync(report.Id, req, report.ReporterUserEmail, false));
        }

        [Fact]
        public async Task GetFullReportDetailAsync_GroupsEvidencesAndDefenses()
        {
            var report = new Report
            {
                Id = 7,
                ReporterUserEmail = "reporter@test.com",
                ReportedUserEmail = "tutor@test.com",
                StatusEnum = ReportStatus.Pending
            };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);

            var defenses = new List<ReportDefense>
            {
                new ReportDefense { Id = 1, ReportId = report.Id, TutorEmail = report.ReportedUserEmail, Note = "def1" }
            };
            _defenseRepo.Setup(r => r.GetByReportIdAsync(report.Id)).ReturnsAsync(defenses);

            var evidences = new List<ReportEvidence>
            {
                new ReportEvidence { Id = 1, ReportId = report.Id, EvidenceType = (int)ReportEvidenceType.ReporterEvidence, MediaType = (int)MediaType.Image, FileUrl = "r1" },
                new ReportEvidence { Id = 2, ReportId = report.Id, EvidenceType = (int)ReportEvidenceType.TutorDefense, MediaType = (int)MediaType.Image, FileUrl = "d1", DefenseId = 1 }
            };
            _evidenceRepo.Setup(r => r.GetByReportIdAsync(report.Id)).ReturnsAsync(evidences);

            var service = CreateService();
            var full = await service.GetFullReportDetailAsync(report.Id, report.ReporterUserEmail, false);

            Assert.NotNull(full);
            Assert.Single(full.Defenses);
            Assert.Single(full.ReporterEvidences);
            Assert.NotEmpty(full.Defenses![0].Evidences!);
        }

        [Fact]
        public async Task CreateReportAsync_SelfReport_Throws()
        {
            var email = "same@test.com";
            _userRepo.Setup(r => r.GetUserByEmailAsync(email)).ReturnsAsync(new User { Email = email });

            var service = CreateService();
            var request = new ReportCreateRequest { ReportedUserEmail = email, Reason = "invalid" };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateReportAsync(request, email));
        }

        [Fact]
        public async Task AddDefenseAsync_OnResolvedReport_Throws()
        {
            var report = new Report { Id = 8, ReportedUserEmail = "tutor@test.com", ReporterUserEmail = "rep@test.com", StatusEnum = ReportStatus.Resolved, CreatedAt = DateTime.UtcNow.AddDays(-3) };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);

            var service = CreateService();
            var request = new ReportDefenseCreateRequest { Note = "response" };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AddDefenseAsync(report.Id, request, report.ReportedUserEmail, false));
        }

        [Fact]
        public async Task AddDefenseAsync_AdminBypassesWindow()
        {
            var report = new Report { Id = 9, ReportedUserEmail = "tutor@test.com", ReporterUserEmail = "rep@test.com", StatusEnum = ReportStatus.Pending, CreatedAt = DateTime.UtcNow.AddDays(-5) };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);
            _defenseRepo.Setup(r => r.AddAsync(It.IsAny<ReportDefense>())).ReturnsAsync(new ReportDefense { Id = 30, ReportId = report.Id });
            _reportRepo.Setup(r => r.UpdateAsync(report)).ReturnsAsync(report);

            var service = CreateService();
            var dto = await service.AddDefenseAsync(report.Id, new ReportDefenseCreateRequest { Note = "admin add" }, "admin@test.com", true);

            Assert.Equal(ReportStatus.UnderReview, report.StatusEnum);
            Assert.Equal(30, dto.Id);
        }

        [Fact]
        public async Task UpdateDefenseAsync_AdminAllowed()
        {
            var report = new Report { Id = 10, ReportedUserEmail = "tutor@test.com", StatusEnum = ReportStatus.Pending, CreatedAt = DateTime.UtcNow.AddDays(-1) };
            var defense = new ReportDefense { Id = 40, ReportId = report.Id, TutorEmail = "tutor@test.com", Note = "old" };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);
            _defenseRepo.Setup(r => r.GetByIdAsync(defense.Id)).ReturnsAsync(defense);
            _defenseRepo.Setup(r => r.UpdateAsync(defense)).ReturnsAsync(defense);

            var service = CreateService();
            var updated = await service.UpdateDefenseAsync(report.Id, defense.Id, new ReportDefenseUpdateRequest { Note = "new" }, "admin@test.com", true);

            Assert.Equal("new", defense.Note);
            Assert.Equal(defense.Id, updated.Id);
        }

        [Fact]
        public async Task UpdateDefenseAsync_UnauthorizedUser_Throws()
        {
            var report = new Report { Id = 10, ReportedUserEmail = "tutor@test.com", StatusEnum = ReportStatus.Pending, CreatedAt = DateTime.UtcNow.AddDays(-1) };
            var defense = new ReportDefense { Id = 41, ReportId = report.Id, TutorEmail = "tutor@test.com", Note = "old" };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);
            _defenseRepo.Setup(r => r.GetByIdAsync(defense.Id)).ReturnsAsync(defense);

            var service = CreateService();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.UpdateDefenseAsync(report.Id, defense.Id, new ReportDefenseUpdateRequest { Note = "new" }, "other@test.com", false));
        }

        [Fact]
        public async Task DeleteDefenseAsync_Unauthorized_Throws()
        {
            var report = new Report { Id = 11, ReportedUserEmail = "tutor@test.com", ReporterUserEmail = "rep@test.com", StatusEnum = ReportStatus.Pending, CreatedAt = DateTime.UtcNow };
            var defense = new ReportDefense { Id = 41, ReportId = report.Id, TutorEmail = report.ReportedUserEmail, Note = "old" };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);
            _defenseRepo.Setup(r => r.GetByIdAsync(defense.Id)).ReturnsAsync(defense);

            var service = CreateService();
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.DeleteDefenseAsync(report.Id, defense.Id, "other@test.com", false));
        }

        [Fact]
        public async Task DeleteDefenseAsync_WhenResolved_Throws()
        {
            var report = new Report { Id = 12, ReportedUserEmail = "tutor@test.com", ReporterUserEmail = "rep@test.com", StatusEnum = ReportStatus.Resolved, CreatedAt = DateTime.UtcNow.AddDays(-5) };
            var defense = new ReportDefense { Id = 42, ReportId = report.Id, TutorEmail = report.ReportedUserEmail, Note = "old" };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);
            _defenseRepo.Setup(r => r.GetByIdAsync(defense.Id)).ReturnsAsync(defense);

            var service = CreateService();
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.DeleteDefenseAsync(report.Id, defense.Id, report.ReportedUserEmail, false));
        }

        [Fact]
        public async Task AddEvidenceAsync_Unauthorized_Throws()
        {
            var report = new Report { Id = 13, ReporterUserEmail = "rep@test.com", ReportedUserEmail = "tutor@test.com", StatusEnum = ReportStatus.Pending };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);

            var service = CreateService();
            var req = new ReportEvidenceCreateRequest { MediaType = MediaType.Image, FileUrl = "https://x/img.jpg" };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.AddEvidenceAsync(report.Id, req, "other@test.com", false));
        }

        [Fact]
        public async Task AddEvidenceAsync_WhenResolved_Throws()
        {
            var report = new Report { Id = 14, ReporterUserEmail = "rep@test.com", ReportedUserEmail = "tutor@test.com", StatusEnum = ReportStatus.Resolved };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);

            var service = CreateService();
            var req = new ReportEvidenceCreateRequest { MediaType = MediaType.Image, FileUrl = "https://x/img.jpg" };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AddEvidenceAsync(report.Id, req, report.ReporterUserEmail, false));
        }

        [Fact]
        public async Task UpdateEvidenceAsync_WhenResolved_Throws()
        {
            var report = new Report { Id = 15, ReporterUserEmail = "rep@test.com", ReportedUserEmail = "tutor@test.com", StatusEnum = ReportStatus.Resolved };
            var evidence = new ReportEvidence { Id = 50, ReportId = report.Id, SubmittedByEmail = report.ReporterUserEmail };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);
            _evidenceRepo.Setup(r => r.GetByIdAsync(evidence.Id)).ReturnsAsync(evidence);

            var service = CreateService();
            var req = new ReportEvidenceUpdateRequest { Caption = "new" };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateEvidenceAsync(report.Id, evidence.Id, req, report.ReporterUserEmail, false));
        }

        [Fact]
        public async Task UpdateEvidenceAsync_Unauthorized_Throws()
        {
            var report = new Report { Id = 16, ReporterUserEmail = "rep@test.com", ReportedUserEmail = "tutor@test.com", StatusEnum = ReportStatus.Pending };
            var evidence = new ReportEvidence { Id = 51, ReportId = report.Id, SubmittedByEmail = report.ReporterUserEmail };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);
            _evidenceRepo.Setup(r => r.GetByIdAsync(evidence.Id)).ReturnsAsync(evidence);

            var service = CreateService();
            var req = new ReportEvidenceUpdateRequest { Caption = "new" };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.UpdateEvidenceAsync(report.Id, evidence.Id, req, "other@test.com", false));
        }

        [Fact]
        public async Task UpdateReportByLearner_WrongUser_Throws()
        {
            var report = new Report { Id = 17, ReporterUserEmail = "rep@test.com", StatusEnum = ReportStatus.Pending };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);

            var service = CreateService();
            var req = new ReportUpdateByLearnerRequest { Reason = "new reason" };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.UpdateReportByLearnerAsync(report.Id, req, "other@test.com"));
        }

        [Fact]
        public async Task CancelReportByLearner_NotPending_Throws()
        {
            var report = new Report { Id = 18, ReporterUserEmail = "rep@test.com", StatusEnum = ReportStatus.Resolved };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);

            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CancelReportByLearnerAsync(report.Id, report.ReporterUserEmail));
        }

        [Fact]
        public async Task GetDefensesAsync_Unauthorized_Throws()
        {
            var report = new Report { Id = 19, ReporterUserEmail = "rep@test.com", ReportedUserEmail = "tutor@test.com" };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);

            var service = CreateService();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.GetDefensesAsync(report.Id, "stranger@test.com", false));
        }

        [Fact]
        public async Task GetEvidenceByReportIdAsync_Unauthorized_Throws()
        {
            var report = new Report { Id = 20, ReporterUserEmail = "rep@test.com", ReportedUserEmail = "tutor@test.com" };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);

            var service = CreateService();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.GetEvidenceByReportIdAsync(report.Id, "stranger@test.com", false));
        }

        [Fact]
        public async Task GetFullReportDetailAsync_Unauthorized_Throws()
        {
            var report = new Report { Id = 21, ReporterUserEmail = "rep@test.com", ReportedUserEmail = "tutor@test.com" };
            _reportRepo.Setup(r => r.GetByIdAsync(report.Id)).ReturnsAsync(report);

            var service = CreateService();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.GetFullReportDetailAsync(report.Id, "stranger@test.com", false));
        }
    }
}
