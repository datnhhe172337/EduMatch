using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Report;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ReportService(IReportRepository reportRepository, IUserRepository userRepository, IMapper mapper)
        {
            _reportRepository = reportRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ReportDetailDto> CreateReportAsync(ReportCreateRequest request, string reporterEmail)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(reporterEmail))
                throw new ArgumentException("Reporter email is required.", nameof(reporterEmail));

            var normalizedReporterEmail = reporterEmail.Trim();
            var normalizedReportedEmail = request.ReportedUserEmail.Trim();

            if (normalizedReporterEmail.Equals(normalizedReportedEmail, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("You cannot report yourself.");

            var reporter = await _userRepository.GetUserByEmailAsync(normalizedReporterEmail)
                ?? throw new ArgumentException("Reporter account does not exist.");

            var reported = await _userRepository.GetUserByEmailAsync(normalizedReportedEmail)
                ?? throw new ArgumentException("Reported user does not exist.");

            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new ArgumentException("Reason is required.", nameof(request.Reason));

            var report = new Report
            {
                ReporterUserEmail = reporter.Email,
                ReportedUserEmail = reported.Email,
                Reason = request.Reason.Trim(),
                CreatedAt = DateTime.UtcNow,
            };
            report.StatusEnum = ReportStatus.Pending;

            var created = await _reportRepository.CreateAsync(report);
            return _mapper.Map<ReportDetailDto>(created);
        }

        public async Task DeleteReportAsync(int reportId)
        {
            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Report not found.");

            await _reportRepository.DeleteAsync(report);
        }

        public async Task<ReportDetailDto?> GetReportDetailAsync(int reportId, string requesterEmail, bool requesterIsAdmin)
        {
            var report = await _reportRepository.GetByIdAsync(reportId);
            if (report == null)
                return null;

            if (!requesterIsAdmin)
            {
                if (string.IsNullOrWhiteSpace(requesterEmail))
                    throw new UnauthorizedAccessException("You are not allowed to view this report.");

                var normalizedEmail = requesterEmail.Trim();
                var isReporter = normalizedEmail.Equals(report.ReporterUserEmail, StringComparison.OrdinalIgnoreCase);
                var isReported = normalizedEmail.Equals(report.ReportedUserEmail, StringComparison.OrdinalIgnoreCase);

                if (!isReporter && !isReported)
                    throw new UnauthorizedAccessException("You are not allowed to view this report.");
            }

            return _mapper.Map<ReportDetailDto>(report);
        }

        public async Task<IReadOnlyList<ReportListItemDto>> GetReportsByReportedUserAsync(string reportedEmail)
        {
            if (string.IsNullOrWhiteSpace(reportedEmail))
                throw new ArgumentException("Reported user email is required.", nameof(reportedEmail));

            var reports = await _reportRepository.GetByReportedUserAsync(reportedEmail.Trim());
            return _mapper.Map<IReadOnlyList<ReportListItemDto>>(reports);
        }

        public async Task<IReadOnlyList<ReportListItemDto>> GetReportsByReporterAsync(string reporterEmail)
        {
            if (string.IsNullOrWhiteSpace(reporterEmail))
                throw new ArgumentException("Reporter email is required.", nameof(reporterEmail));

            var reports = await _reportRepository.GetByReporterAsync(reporterEmail.Trim());
            return _mapper.Map<IReadOnlyList<ReportListItemDto>>(reports);
        }

        public async Task<ReportDetailDto> SubmitTutorComplaintAsync(int reportId, TutorComplaintRequest request, string tutorEmail)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(tutorEmail))
                throw new ArgumentException("Tutor email is required.", nameof(tutorEmail));

            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Report not found.");

            if (!tutorEmail.Trim().Equals(report.ReportedUserEmail, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("You cannot respond to this report.");

            if (report.StatusEnum != ReportStatus.UnderReview)
                throw new InvalidOperationException("Tutor defense is allowed only after the report is approved for review by an admin.");

            report.TutorDefenseNote = request.DefenseNote.Trim();
            report.UpdatedAt = DateTime.UtcNow;

            var updated = await _reportRepository.UpdateAsync(report);
            return _mapper.Map<ReportDetailDto>(updated);
        }

        public async Task<ReportDetailDto> UpdateReportAsync(int reportId, ReportUpdateRequest request, string adminEmail)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(adminEmail))
                throw new ArgumentException("Admin email is required.", nameof(adminEmail));

            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Report not found.");

            report.StatusEnum = request.Status;
            report.AdminNotes = string.IsNullOrWhiteSpace(request.AdminNotes) ? null : request.AdminNotes.Trim();
            report.HandledByAdminEmail = adminEmail.Trim();
            report.UpdatedAt = DateTime.UtcNow;

            var updated = await _reportRepository.UpdateAsync(report);
            return _mapper.Map<ReportDetailDto>(updated);
        }

        public async Task<ReportDetailDto> UpdateReportByLearnerAsync(int reportId, ReportUpdateByLearnerRequest request, string learnerEmail)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(learnerEmail))
                throw new ArgumentException("Learner email is required.", nameof(learnerEmail));

            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Report not found.");

            if (!learnerEmail.Trim().Equals(report.ReporterUserEmail, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("You cannot edit this report.");

            if (report.StatusEnum != ReportStatus.Pending)
                throw new InvalidOperationException("Reports can only be edited while pending review.");

            report.Reason = request.Reason.Trim();
            report.UpdatedAt = DateTime.UtcNow;

            var updated = await _reportRepository.UpdateAsync(report);
            return _mapper.Map<ReportDetailDto>(updated);
        }
    }
}
