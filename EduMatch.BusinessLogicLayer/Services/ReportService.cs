using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Report;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Utils;
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
        private readonly IReportDefenseRepository _reportDefenseRepository;
        private readonly EmailService _emailService;

        public ReportService(
            IReportRepository reportRepository,
            IUserRepository userRepository,
            IMapper mapper,
            IReportContentValidator contentValidator,
            INotificationService notificationService,
            IReportEvidenceRepository reportEvidenceRepository,
            IReportDefenseRepository reportDefenseRepository,
            EmailService emailService)
        {
            _reportRepository = reportRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _contentValidator = contentValidator;
            _notificationService = notificationService;
            _reportEvidenceRepository = reportEvidenceRepository;
            _reportDefenseRepository = reportDefenseRepository;
            _emailService = emailService;
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
                BookingId = request.BookingId,
                ScheduleId = request.ScheduleId,
                CreatedAt = VietnamTimeProvider.Now(),
            };
            report.StatusEnum = ReportStatus.Pending;

            var created = await _reportRepository.CreateAsync(report);

            var reporterName = string.IsNullOrWhiteSpace(reporter.UserName) ? reporter.Email : reporter.UserName;
            var tutorMessage = $"Bạn vừa nhận được một báo cáo mới từ {reporterName}. Lý do: {report.Reason}. Bạn có 2 ngày để gửi giải trình trước khi quản trị viên xem xét.";
            await _notificationService.CreateNotificationAsync(reported.Email, tutorMessage);
            await _emailService.SendMailAsync(new MailContent
            {
                To = reported.Email,
                Subject = "Báo cáo mới liên quan đến bạn",
                Body = tutorMessage
            });

            var reporterMessage = $"Bạn đã tạo báo cáo đối với {reported.Email}. Lý do: {report.Reason}.";
            await _notificationService.CreateNotificationAsync(reporter.Email, reporterMessage);
            await _emailService.SendMailAsync(new MailContent
            {
                To = reporter.Email,
                Subject = "Báo cáo đã được tạo",
                Body = reporterMessage
            });

            if (request.Evidences != null && request.Evidences.Count > 0)
            {
                foreach (var ev in request.Evidences)
                {
                    await AddEvidenceAsync(created.Id, new ReportEvidenceCreateRequest
                    {
                        MediaType = ev.MediaType,
                        FileUrl = ev.FileUrl,
                        FilePublicId = ev.FilePublicId,
                        Caption = ev.Caption,
                        EvidenceType = ReportEvidenceType.ReporterEvidence
                    }, reporterEmail, true);
                }
            }

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

        public async Task<IReadOnlyList<int>> GetReportIdsByBookingIdAsync(int bookingId, string? currentUserEmail, bool currentUserIsAdmin)
        {
            if (bookingId <= 0)
                throw new ArgumentException("BookingId must be greater than 0.", nameof(bookingId));

            var reports = await _reportRepository.GetByBookingIdAsync(bookingId);
            if (!currentUserIsAdmin)
            {
                if (string.IsNullOrWhiteSpace(currentUserEmail))
                    throw new UnauthorizedAccessException("User email not found.");

                var normalizedEmail = currentUserEmail.Trim();
                reports = reports
                    .Where(r => r.ReporterUserEmail.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase) ||
                                r.ReportedUserEmail.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return reports.Select(r => r.Id).ToList();
        }

        public async Task<IReadOnlyList<int>> GetReportIdsByScheduleIdAsync(int scheduleId, string? currentUserEmail, bool currentUserIsAdmin)
        {
            if (scheduleId <= 0)
                throw new ArgumentException("ScheduleId must be greater than 0.", nameof(scheduleId));

            var reports = await _reportRepository.GetByScheduleIdAsync(scheduleId);
            if (!currentUserIsAdmin)
            {
                if (string.IsNullOrWhiteSpace(currentUserEmail))
                    throw new UnauthorizedAccessException("User email not found.");

                var normalizedEmail = currentUserEmail.Trim();
                reports = reports
                    .Where(r => r.ReporterUserEmail.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase) ||
                                r.ReportedUserEmail.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return reports.Select(r => r.Id).ToList();
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

        public async Task<ReportDetailDto> UpdateReportAsync(int reportId, ReportUpdateRequest request, string adminEmail)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(adminEmail))
                throw new ArgumentException("Email quản trị viên là bắt buộc.", nameof(adminEmail));

            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo cáo.");

            var reviewAllowedAt = report.CreatedAt.AddDays(2);
            if (VietnamTimeProvider.Now() < reviewAllowedAt)
                throw new InvalidOperationException("Quản trị viên chỉ được xử lý báo cáo sau 2 ngày kể từ khi được tạo để gia sư có thời gian giải trình.");

            report.StatusEnum = request.Status;
            report.AdminNotes = string.IsNullOrWhiteSpace(request.AdminNotes) ? null : request.AdminNotes.Trim();
            report.HandledByAdminEmail = adminEmail.Trim();
            report.UpdatedAt = VietnamTimeProvider.Now();

            if (request.Status != ReportStatus.Resolved && request.Status != ReportStatus.Dismissed)
                throw new InvalidOperationException("Quản trị viên chỉ cập nhật trạng thái sang Resolved hoặc Dismissed.");

            var updated = await _reportRepository.UpdateAsync(report);

            await NotifyStatusChangeAsync(report);
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
            report.UpdatedAt = VietnamTimeProvider.Now();

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
            report.UpdatedAt = VietnamTimeProvider.Now();

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

            if (request.DefenseId.HasValue)
            {
                var defense = await _reportDefenseRepository.GetByIdAsync(request.DefenseId.Value);

                if (defense == null && !currentUserIsAdmin)
                    throw new KeyNotFoundException("Không tìm thấy báo vệ.");

                if (defense != null && defense.ReportId != report.Id)
                    throw new InvalidOperationException("Bằng chứng không thuộc báo cáo/báo vệ này.");
            }

            var evidenceType = request.EvidenceType ??
                (isReporter ? ReportEvidenceType.ReporterEvidence :
                 isReported ? ReportEvidenceType.TutorDefense :
                 ReportEvidenceType.AdminAttachment);

            var evidence = new ReportEvidence
            {
                ReportId = reportId,
                SubmittedByEmail = normalizedEmail,
                MediaType = (int)request.MediaType,
                EvidenceType = (int)evidenceType,
                FileUrl = request.FileUrl.Trim(),
                FilePublicId = string.IsNullOrWhiteSpace(request.FilePublicId) ? null : request.FilePublicId.Trim(),
                Caption = string.IsNullOrWhiteSpace(request.Caption) ? null : request.Caption.Trim(),
                CreatedAt = VietnamTimeProvider.Now()
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

        public async Task<IReadOnlyList<ReportListItemDto>> GetAllReportsAsync()
        {
            var reports = await _reportRepository.GetAllAsync();
            return _mapper.Map<IReadOnlyList<ReportListItemDto>>(reports);
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

            if (request.EvidenceType.HasValue)
            {
                if (!Enum.IsDefined(typeof(ReportEvidenceType), request.EvidenceType.Value))
                    throw new ArgumentException("Loại bằng chứng không hợp lệ.");
                if (!currentUserIsAdmin && !isReporter && !isReported)
                    throw new UnauthorizedAccessException("Bạn không thể thay đổi loại bằng chứng này.");
                evidence.EvidenceType = (int)request.EvidenceType.Value;
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

        public async Task<bool> CanSubmitDefenseAsync(int reportId, string tutorEmail, bool currentUserIsAdmin)
        {
            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo cáo.");

            if (report.StatusEnum == ReportStatus.Resolved || report.StatusEnum == ReportStatus.Dismissed)
                return false;

            return VietnamTimeProvider.Now() <= report.CreatedAt.AddDays(2);
        }

        public async Task<ReportDefenseDto> AddDefenseAsync(int reportId, ReportDefenseCreateRequest request, string tutorEmail, bool currentUserIsAdmin)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(tutorEmail))
                throw new ArgumentException("Email gia sư là bắt buộc.", nameof(tutorEmail));

            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo cáo.");

            if (!currentUserIsAdmin && !tutorEmail.Trim().Equals(report.ReportedUserEmail, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Bạn không thể gửi báo vệ cho báo cáo này.");

            if (report.StatusEnum == ReportStatus.Resolved || report.StatusEnum == ReportStatus.Dismissed)
                throw new InvalidOperationException("Báo cáo đã được xử lý, không thể gửi thêm báo vệ.");

            if (VietnamTimeProvider.Now() > report.CreatedAt.AddDays(2) && !currentUserIsAdmin)
                throw new InvalidOperationException("Đã quá hạn 2 ngày để gửi báo vệ.");

            var defense = new ReportDefense
            {
                ReportId = reportId,
                TutorEmail = tutorEmail.Trim(),
                Note = request.Note.Trim(),
                CreatedAt = VietnamTimeProvider.Now()
            };

            var savedDefense = await _reportDefenseRepository.AddAsync(defense);

            if (report.StatusEnum == ReportStatus.Pending)
            {
                report.StatusEnum = ReportStatus.UnderReview;
                report.UpdatedAt = VietnamTimeProvider.Now();
                await _reportRepository.UpdateAsync(report);
                await NotifyStatusChangeAsync(report);
            }

            // Notify reporter that the tutor has added a defense
            var defenseNotice = $"Gia sư {report.ReportedUserEmail} đã gửi phản hồi cho báo cáo của bạn (ID {report.Id}).";
            await _notificationService.CreateNotificationAsync(report.ReporterUserEmail, defenseNotice);
            await _emailService.SendMailAsync(new MailContent
            {
                To = report.ReporterUserEmail,
                Subject = "Báo cáo đã có phản hồi",
                Body = defenseNotice
            });

            // Confirm to tutor
            await _emailService.SendMailAsync(new MailContent
            {
                To = tutorEmail.Trim(),
                Subject = "Đã gửi phản hồi cho báo cáo",
                Body = $"Bạn đã gửi phản hồi cho báo cáo từ người dùng {report.ReporterUserEmail}."
            });

            if (request.Evidences != null && request.Evidences.Count > 0)
            {
                foreach (var ev in request.Evidences)
                {
                    await AddEvidenceAsync(reportId, new ReportEvidenceCreateRequest
                    {
                        MediaType = ev.MediaType,
                        FileUrl = ev.FileUrl,
                        FilePublicId = ev.FilePublicId,
                        Caption = ev.Caption,
                        EvidenceType = ReportEvidenceType.TutorDefense,
                        DefenseId = savedDefense.Id
                    }, tutorEmail, true);
                }
            }

            return _mapper.Map<ReportDefenseDto>(savedDefense);
        }

        public async Task<IReadOnlyList<ReportDefenseDto>> GetDefensesAsync(int reportId, string currentUserEmail, bool currentUserIsAdmin)
        {
            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo cáo.");

            if (!currentUserIsAdmin)
            {
                if (string.IsNullOrWhiteSpace(currentUserEmail))
                    throw new UnauthorizedAccessException("Bạn không có quyền xem báo vệ này.");

                var normalizedEmail = currentUserEmail.Trim();
                var isReporter = normalizedEmail.Equals(report.ReporterUserEmail, StringComparison.OrdinalIgnoreCase);
                var isReported = normalizedEmail.Equals(report.ReportedUserEmail, StringComparison.OrdinalIgnoreCase);

                if (!isReporter && !isReported)
                    throw new UnauthorizedAccessException("Bạn không có quyền xem báo vệ này.");
            }

            var defenses = await _reportDefenseRepository.GetByReportIdAsync(reportId);
            var defenseDtos = _mapper.Map<List<ReportDefenseDto>>(defenses);

            var evidences = await _reportEvidenceRepository.GetByReportIdAsync(reportId);
            var evidenceByDefense = evidences.Where(e => e.DefenseId.HasValue)
                .GroupBy(e => e.DefenseId!.Value)
                .ToDictionary(g => g.Key, g => _mapper.Map<IReadOnlyList<ReportEvidenceDto>>(g.ToList()));

            foreach (var dto in defenseDtos)
            {
                if (evidenceByDefense.TryGetValue(dto.Id, out var evs))
                    dto.Evidences = evs;
            }

            return defenseDtos;
        }

        public async Task<ReportDefenseDto> UpdateDefenseAsync(int reportId, int defenseId, ReportDefenseUpdateRequest request, string currentUserEmail, bool currentUserIsAdmin)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(currentUserEmail))
                throw new ArgumentException("Email người dùng là bắt buộc.", nameof(currentUserEmail));

            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo cáo.");

            var defense = await _reportDefenseRepository.GetByIdAsync(defenseId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo vệ.");

            if (defense.ReportId != report.Id)
                throw new InvalidOperationException("Báo vệ không thuộc báo cáo này.");

            var normalizedEmail = currentUserEmail.Trim();
            var isOwner = normalizedEmail.Equals(defense.TutorEmail, StringComparison.OrdinalIgnoreCase);
            var isReported = normalizedEmail.Equals(report.ReportedUserEmail, StringComparison.OrdinalIgnoreCase);

            if (!currentUserIsAdmin && !isOwner && !isReported)
                throw new UnauthorizedAccessException("Bạn không thể chỉnh sửa báo vệ này.");

            if (report.StatusEnum == ReportStatus.Resolved || report.StatusEnum == ReportStatus.Dismissed)
                throw new InvalidOperationException("Báo cáo đã được xử lý, không thể chỉnh sửa báo vệ.");

            if (VietnamTimeProvider.Now() > report.CreatedAt.AddDays(2) && !currentUserIsAdmin)
                throw new InvalidOperationException("Đã quá hạn 2 ngày để chỉnh sửa báo vệ.");

            defense.Note = request.Note.Trim();
            defense.CreatedAt = defense.CreatedAt; // giữ nguyên thời gian tạo

            var updated = await _reportDefenseRepository.UpdateAsync(defense);
            return _mapper.Map<ReportDefenseDto>(updated);
        }

        public async Task DeleteDefenseAsync(int reportId, int defenseId, string currentUserEmail, bool currentUserIsAdmin)
        {
            if (string.IsNullOrWhiteSpace(currentUserEmail))
                throw new ArgumentException("Email người dùng là bắt buộc.", nameof(currentUserEmail));

            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo cáo.");

            var defense = await _reportDefenseRepository.GetByIdAsync(defenseId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo vệ.");

            if (defense.ReportId != report.Id)
                throw new InvalidOperationException("Báo vệ không thuộc báo cáo này.");

            var normalizedEmail = currentUserEmail.Trim();
            var isOwner = normalizedEmail.Equals(defense.TutorEmail, StringComparison.OrdinalIgnoreCase);
            var isReported = normalizedEmail.Equals(report.ReportedUserEmail, StringComparison.OrdinalIgnoreCase);

            if (!currentUserIsAdmin && !isOwner && !isReported)
                throw new UnauthorizedAccessException("Bạn không thể xóa báo vệ này.");

            if (report.StatusEnum == ReportStatus.Resolved || report.StatusEnum == ReportStatus.Dismissed)
                throw new InvalidOperationException("Báo cáo đã xử lý, không thể xóa báo vệ.");

            await _reportDefenseRepository.DeleteAsync(defense);
        }

        public async Task<ReportFullDetailDto?> GetFullReportDetailAsync(int reportId, string requesterEmail, bool requesterIsAdmin)
        {
            var reportDetail = await GetReportDetailAsync(reportId, requesterEmail, requesterIsAdmin);
            if (reportDetail == null)
                return null;

            var evidences = await _reportEvidenceRepository.GetByReportIdAsync(reportId);
            var defenses = await _reportDefenseRepository.GetByReportIdAsync(reportId);

            var reporterEvidences = evidences
                .Where(e => !e.DefenseId.HasValue && e.EvidenceType == (int)ReportEvidenceType.ReporterEvidence);
            var tutorEvidences = evidences
                .Where(e => !e.DefenseId.HasValue && e.EvidenceType == (int)ReportEvidenceType.TutorDefense);
            var adminEvidences = evidences
                .Where(e => !e.DefenseId.HasValue && e.EvidenceType == (int)ReportEvidenceType.AdminAttachment);

            var defenseDtos = _mapper.Map<List<ReportDefenseDto>>(defenses);
            var defenseEvidenceLookup = evidences.Where(e => e.DefenseId.HasValue)
                .GroupBy(e => e.DefenseId!.Value)
                .ToDictionary(g => g.Key, g => _mapper.Map<IReadOnlyList<ReportEvidenceDto>>(g.ToList()));

            foreach (var dto in defenseDtos)
            {
                if (defenseEvidenceLookup.TryGetValue(dto.Id, out var evs))
                    dto.Evidences = evs;
            }

            return new ReportFullDetailDto
            {
                Id = reportDetail.Id,
                ReporterEmail = reportDetail.ReporterEmail,
                ReporterName = reportDetail.ReporterName,
                ReporterAvatarUrl = reportDetail.ReporterAvatarUrl,
                ReportedUserEmail = reportDetail.ReportedUserEmail,
                ReportedUserName = reportDetail.ReportedUserName,
                ReportedAvatarUrl = reportDetail.ReportedAvatarUrl,
                Reason = reportDetail.Reason,
                Status = reportDetail.Status,
                TutorDefenseNote = reportDetail.TutorDefenseNote,
                AdminNotes = reportDetail.AdminNotes,
                HandledByAdminEmail = reportDetail.HandledByAdminEmail,
                CreatedAt = reportDetail.CreatedAt,
                UpdatedAt = reportDetail.UpdatedAt,
                Booking = reportDetail.Booking,
                Defenses = defenseDtos,
                ReporterEvidences = _mapper.Map<IReadOnlyList<ReportEvidenceDto>>(reporterEvidences.ToList()),
                TutorEvidences = _mapper.Map<IReadOnlyList<ReportEvidenceDto>>(tutorEvidences.ToList()),
                AdminEvidences = _mapper.Map<IReadOnlyList<ReportEvidenceDto>>(adminEvidences.ToList())
            };
        }

        public async Task<bool> IsReportResolvedAsync(int reportId, string requesterEmail, bool requesterIsAdmin)
        {
            var report = await _reportRepository.GetByIdAsync(reportId)
                ?? throw new KeyNotFoundException("Không tìm thấy báo cáo.");

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

            return report.StatusEnum == ReportStatus.Resolved;
        }

        private async Task NotifyStatusChangeAsync(Report report)
        {
            var statusMessage = report.StatusEnum switch
            {
                ReportStatus.Pending => "đang chờ xử lý",
                ReportStatus.UnderReview => "đang được xem xét",
                ReportStatus.Resolved => "đã được giải quyết",
                ReportStatus.Dismissed => "đã bị từ chối",
                _ => report.StatusEnum.ToString()
            };

            var reporterMessage = $"Báo cáo của bạn đối với {report.ReportedUserEmail} hiện {statusMessage}.";
            await _notificationService.CreateNotificationAsync(report.ReporterUserEmail, reporterMessage);
            await _emailService.SendMailAsync(new MailContent
            {
                To = report.ReporterUserEmail,
                Subject = "Cập nhật trạng thái báo cáo",
                Body = reporterMessage
            });

            var tutorMessage = $"Báo cáo liên quan tới bạn từ {report.ReporterUserEmail} hiện {statusMessage}.";
            await _notificationService.CreateNotificationAsync(report.ReportedUserEmail, tutorMessage);
            await _emailService.SendMailAsync(new MailContent
            {
                To = report.ReportedUserEmail,
                Subject = "Cập nhật trạng thái báo cáo",
                Body = tutorMessage
            });
        }
    }
}
