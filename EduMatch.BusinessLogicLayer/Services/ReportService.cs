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
        private readonly INotificationService _notificationService;
        private readonly IReportEvidenceRepository _reportEvidenceRepository;

        public ReportService(
            IReportRepository reportRepository,
            IUserRepository userRepository,
            IMapper mapper,
            IReportContentValidator contentValidator,
            INotificationService notificationService,
            IReportEvidenceRepository reportEvidenceRepository)
        {
            _reportRepository = reportRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _contentValidator = contentValidator;
            _notificationService = notificationService;
            _reportEvidenceRepository = reportEvidenceRepository;
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

            var reporterName = string.IsNullOrWhiteSpace(reporter.UserName) ? reporter.Email : reporter.UserName;
            var tutorMessage = $"Bạn vừa nhận được một báo cáo mới từ {reporterName}. Lý do: {report.Reason}. Bạn có 2 ngày để gửi giải trình trước khi quản trị viên xem xét.";
            await _notificationService.CreateNotificationAsync(reported.Email, tutorMessage);

            return _mapper.Map<ReportDetailDto>(created);
        }

        /// <summary>
        /// Deletes the specified report permanently. Intended for admin cleanup only.
        /// </summary>
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

            if (report.StatusEnum == ReportStatus.Resolved || report.StatusEnum == ReportStatus.Dismissed)
                throw new InvalidOperationException("Báo cáo đã được xử lý, không thể gửi giải trình.");

            if (DateTime.UtcNow > report.CreatedAt.AddDays(2))
                throw new InvalidOperationException("Đã quá hạn 2 ngày để gửi giải trình.");

            report.TutorDefenseNote = request.DefenseNote.Trim();
            report.UpdatedAt = DateTime.UtcNow;

            var updated = await _reportRepository.UpdateAsync(report);

            await NotifyReporterStatusChangeAsync(report);

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

            var reviewAllowedAt = report.CreatedAt.AddDays(2);
            if (DateTime.UtcNow < reviewAllowedAt)
                throw new InvalidOperationException("Quản trị viên chỉ được xử lý báo cáo sau 2 ngày kể từ khi được tạo để gia sư có thời gian giải trình.");

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

        public async Task<ReportEvidenceDto> AddEvidenceAsync(int reportId, ReportEvidenceCreateRequest request, string currentUserEmail, bool currentUserIsAdmin)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(currentUserEmail))
                throw new ArgumentException("Email người dùng là bắt buộc.", nameof(currentUserEmail));

            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo cáo.");

            var normalizedEmail = currentUserEmail.Trim();
            var isReporter = normalizedEmail.Equals(report.ReporterUserEmail, StringComparison.OrdinalIgnoreCase);
            var isReported = normalizedEmail.Equals(report.ReportedUserEmail, StringComparison.OrdinalIgnoreCase);

            if (!currentUserIsAdmin && !isReporter && !isReported)
                throw new UnauthorizedAccessException("Bạn không thể đính kèm bằng chứng cho báo cáo này.");

            if (report.StatusEnum == ReportStatus.Resolved || report.StatusEnum == ReportStatus.Dismissed)
                throw new InvalidOperationException("Báo cáo đã được xử lý, không thể đính kèm thêm bằng chứng.");

            if (!Enum.IsDefined(typeof(MediaType), request.MediaType))
                throw new ArgumentException("Loại media không hợp lệ.");
            if (string.IsNullOrWhiteSpace(request.FileUrl))
                throw new ArgumentException("FileUrl là bắt buộc.");

            var evidence = new ReportEvidence
            {
                ReportId = reportId,
                SubmittedByEmail = normalizedEmail,
                MediaType = (int)request.MediaType,
                FileUrl = request.FileUrl.Trim(),
                FilePublicId = string.IsNullOrWhiteSpace(request.FilePublicId) ? null : request.FilePublicId.Trim(),
                Caption = string.IsNullOrWhiteSpace(request.Caption) ? null : request.Caption.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            var saved = await _reportEvidenceRepository.AddAsync(evidence);
            return _mapper.Map<ReportEvidenceDto>(saved);
        }

        public async Task<IReadOnlyList<ReportEvidenceDto>> GetEvidenceByReportIdAsync(int reportId, string currentUserEmail, bool currentUserIsAdmin)
        {
            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo cáo.");

            if (!currentUserIsAdmin)
            {
                if (string.IsNullOrWhiteSpace(currentUserEmail))
                    throw new UnauthorizedAccessException("Bạn không có quyền xem bằng chứng của báo cáo này.");

                var normalizedEmail = currentUserEmail.Trim();
                var isReporter = normalizedEmail.Equals(report.ReporterUserEmail, StringComparison.OrdinalIgnoreCase);
                var isReported = normalizedEmail.Equals(report.ReportedUserEmail, StringComparison.OrdinalIgnoreCase);

                if (!isReporter && !isReported)
                    throw new UnauthorizedAccessException("Bạn không có quyền xem bằng chứng của báo cáo này.");
            }

            var evidences = await _reportEvidenceRepository.GetByReportIdAsync(reportId);
            return _mapper.Map<IReadOnlyList<ReportEvidenceDto>>(evidences);
        }

        public async Task<ReportEvidenceDto> UpdateEvidenceAsync(int reportId, int evidenceId, ReportEvidenceUpdateRequest request, string currentUserEmail, bool currentUserIsAdmin)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(currentUserEmail))
                throw new ArgumentException("Email người dùng là bắt buộc.", nameof(currentUserEmail));

            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo cáo.");

            var evidence = await _reportEvidenceRepository.GetByIdAsync(evidenceId)
                ?? throw new KeyNotFoundException("Không tìm thấy bằng chứng.");

            if (evidence.ReportId != report.Id)
                throw new InvalidOperationException("Bằng chứng không thuộc báo cáo này.");

            var normalizedEmail = currentUserEmail.Trim();
            var isOwner = !string.IsNullOrWhiteSpace(evidence.SubmittedByEmail) &&
                          normalizedEmail.Equals(evidence.SubmittedByEmail, StringComparison.OrdinalIgnoreCase);
            var isReporter = normalizedEmail.Equals(report.ReporterUserEmail, StringComparison.OrdinalIgnoreCase);
            var isReported = normalizedEmail.Equals(report.ReportedUserEmail, StringComparison.OrdinalIgnoreCase);

            if (!currentUserIsAdmin && !isOwner && !isReporter && !isReported)
                throw new UnauthorizedAccessException("Bạn không thể chỉnh sửa bằng chứng này.");

            if (report.StatusEnum == ReportStatus.Resolved || report.StatusEnum == ReportStatus.Dismissed)
                throw new InvalidOperationException("Báo cáo đã được xử lý, không thể chỉnh sửa bằng chứng.");

            if (request.MediaType.HasValue)
            {
                if (!Enum.IsDefined(typeof(MediaType), request.MediaType.Value))
                    throw new ArgumentException("Loại media không hợp lệ.");
                evidence.MediaType = (int)request.MediaType.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.FileUrl))
                evidence.FileUrl = request.FileUrl.Trim();
            if (request.FilePublicId != null)
                evidence.FilePublicId = string.IsNullOrWhiteSpace(request.FilePublicId) ? null : request.FilePublicId.Trim();
            if (request.Caption != null)
                evidence.Caption = string.IsNullOrWhiteSpace(request.Caption) ? null : request.Caption.Trim();

            var updated = await _reportEvidenceRepository.UpdateAsync(evidence);
            return _mapper.Map<ReportEvidenceDto>(updated);
        }

        public async Task DeleteEvidenceAsync(int reportId, int evidenceId, string currentUserEmail, bool currentUserIsAdmin)
        {
            if (string.IsNullOrWhiteSpace(currentUserEmail))
                throw new ArgumentException("Email người dùng là bắt buộc.", nameof(currentUserEmail));

            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo cáo.");

            var evidence = await _reportEvidenceRepository.GetByIdAsync(evidenceId)
                ?? throw new KeyNotFoundException("Không tìm thấy bằng chứng.");

            if (evidence.ReportId != report.Id)
                throw new InvalidOperationException("Bằng chứng không thuộc báo cáo này.");

            var normalizedEmail = currentUserEmail.Trim();
            var isOwner = !string.IsNullOrWhiteSpace(evidence.SubmittedByEmail) &&
                          normalizedEmail.Equals(evidence.SubmittedByEmail, StringComparison.OrdinalIgnoreCase);
            var isReporter = normalizedEmail.Equals(report.ReporterUserEmail, StringComparison.OrdinalIgnoreCase);
            var isReported = normalizedEmail.Equals(report.ReportedUserEmail, StringComparison.OrdinalIgnoreCase);

            if (!currentUserIsAdmin && !isOwner && !isReporter && !isReported)
                throw new UnauthorizedAccessException("Bạn không thể xóa bằng chứng này.");

            if (report.StatusEnum == ReportStatus.Resolved || report.StatusEnum == ReportStatus.Dismissed)
                throw new InvalidOperationException("Báo cáo đã được xử lý, không thể xóa bằng chứng.");

            await _reportEvidenceRepository.DeleteAsync(evidence);
        }

        private async Task NotifyReporterStatusChangeAsync(Report report)
        {
            var statusMessage = report.StatusEnum switch
            {
                ReportStatus.Pending => "đang chờ xử lý",
                ReportStatus.UnderReview => "đang được xem xét",
                ReportStatus.Resolved => "đã được giải quyết",
                ReportStatus.Dismissed => "đã bị từ chối",
                _ => report.StatusEnum.ToString()
            };

            var message = $"Báo cáo của bạn đối với {report.ReportedUserEmail} hiện {statusMessage}.";
            await _notificationService.CreateNotificationAsync(report.ReporterUserEmail, message);
        }
    }
}
