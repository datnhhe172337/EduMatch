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
        private readonly IReportContentValidator _contentValidator;

        public ReportService(IReportRepository reportRepository, IUserRepository userRepository, IMapper mapper, IReportContentValidator contentValidator)
        {
            _reportRepository = reportRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _contentValidator = contentValidator;
        }

        public async Task<ReportDetailDto> CreateReportAsync(ReportCreateRequest request, string reporterEmail)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(reporterEmail))
                throw new ArgumentException("Email người báo cáo là bắt buộc.", nameof(reporterEmail));

            var normalizedReporterEmail = reporterEmail.Trim();
            var normalizedReportedEmail = request.ReportedUserEmail.Trim();

            if (normalizedReporterEmail.Equals(normalizedReportedEmail, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Bạn không thể tự báo cáo chính mình.");

            var reporter = await _userRepository.GetUserByEmailAsync(normalizedReporterEmail)
                ?? throw new ArgumentException("Không tìm thấy tài khoản người báo cáo.");

            var reported = await _userRepository.GetUserByEmailAsync(normalizedReportedEmail)
                ?? throw new ArgumentException("Không tìm thấy người bị báo cáo.");

            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new ArgumentException("Lý do là bắt buộc.", nameof(request.Reason));

            await _contentValidator.ValidateAsync(reporter.Email, reported.Email, request.Reason);

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
                ?? throw new KeyNotFoundException("Không tìm thấy báo cáo.");

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
                    throw new UnauthorizedAccessException("Bạn không có quyền xem báo cáo này.");

                var normalizedEmail = requesterEmail.Trim();
                var isReporter = normalizedEmail.Equals(report.ReporterUserEmail, StringComparison.OrdinalIgnoreCase);
                var isReported = normalizedEmail.Equals(report.ReportedUserEmail, StringComparison.OrdinalIgnoreCase);

                if (!isReporter && !isReported)
                    throw new UnauthorizedAccessException("Bạn không có quyền xem báo cáo này.");
            }

            return _mapper.Map<ReportDetailDto>(report);
        }

        public async Task<IReadOnlyList<ReportListItemDto>> GetReportsByReportedUserAsync(string reportedEmail)
        {
            if (string.IsNullOrWhiteSpace(reportedEmail))
                throw new ArgumentException("Email người bị báo cáo là bắt buộc.", nameof(reportedEmail));

            var reports = await _reportRepository.GetByReportedUserAsync(reportedEmail.Trim());
            return _mapper.Map<IReadOnlyList<ReportListItemDto>>(reports);
        }

        public async Task<IReadOnlyList<ReportListItemDto>> GetReportsByReporterAsync(string reporterEmail)
        {
            if (string.IsNullOrWhiteSpace(reporterEmail))
                throw new ArgumentException("Email người báo cáo là bắt buộc.", nameof(reporterEmail));

            var reports = await _reportRepository.GetByReporterAsync(reporterEmail.Trim());
            return _mapper.Map<IReadOnlyList<ReportListItemDto>>(reports);
        }

        public async Task<ReportDetailDto> SubmitTutorComplaintAsync(int reportId, TutorComplaintRequest request, string tutorEmail)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(tutorEmail))
                throw new ArgumentException("Email gia sư là bắt buộc.", nameof(tutorEmail));

            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo cáo.");

            if (!tutorEmail.Trim().Equals(report.ReportedUserEmail, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Bạn không thể phản hồi báo cáo này.");

            if (report.StatusEnum != ReportStatus.UnderReview)
                throw new InvalidOperationException("Giải trình chỉ được gửi sau khi báo cáo được chuyển sang trạng thái xem xét.");

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
                throw new ArgumentException("Email quản trị viên là bắt buộc.", nameof(adminEmail));

            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo cáo.");

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
                throw new ArgumentException("Email học viên là bắt buộc.", nameof(learnerEmail));

            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo cáo.");

            if (!learnerEmail.Trim().Equals(report.ReporterUserEmail, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Bạn không thể chỉnh sửa báo cáo này.");

            if (report.StatusEnum != ReportStatus.Pending)
                throw new InvalidOperationException("Báo cáo chỉ có thể chỉnh sửa khi đang chờ xử lý.");

            await _contentValidator.ValidateAsync(report.ReporterUserEmail, report.ReportedUserEmail, request.Reason, report.Id);

            report.Reason = request.Reason.Trim();
            report.UpdatedAt = DateTime.UtcNow;

            var updated = await _reportRepository.UpdateAsync(report);
            return _mapper.Map<ReportDetailDto>(updated);
        }

        public async Task<ReportDetailDto> CancelReportByLearnerAsync(int reportId, string learnerEmail)
        {
            if (string.IsNullOrWhiteSpace(learnerEmail))
                throw new ArgumentException("Email học viên là bắt buộc.", nameof(learnerEmail));

            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo cáo.");

            if (!learnerEmail.Trim().Equals(report.ReporterUserEmail, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Bạn không thể hủy báo cáo này.");

            if (report.StatusEnum != ReportStatus.Pending)
                throw new InvalidOperationException("Báo cáo chỉ có thể hủy trước khi quản trị viên xử lý.");

            report.StatusEnum = ReportStatus.Dismissed;
            report.UpdatedAt = DateTime.UtcNow;

            var updated = await _reportRepository.UpdateAsync(report);
            return _mapper.Map<ReportDetailDto>(updated);
        }
    }
}
