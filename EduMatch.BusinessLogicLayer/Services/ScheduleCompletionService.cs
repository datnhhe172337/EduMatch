using System;
using System.Threading.Tasks;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class ScheduleCompletionService : IScheduleCompletionService
    {
        private const string SystemWalletEmail = "system@edumatch.com";
        private readonly IScheduleCompletionRepository _completionRepository;
        private readonly ITutorPayoutRepository _payoutRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITutorPayoutService _tutorPayoutService;
        private readonly IBookingRepository _bookingRepository;
        private readonly INotificationService _notificationService;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly EmailService _emailService;
        private readonly TimeZoneInfo _vietnamTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public ScheduleCompletionService(
            IScheduleCompletionRepository completionRepository,
            ITutorPayoutRepository payoutRepository,
            IUnitOfWork unitOfWork,
            ITutorPayoutService tutorPayoutService,
            IBookingRepository bookingRepository,
            INotificationService notificationService,
            IScheduleRepository scheduleRepository,
            EmailService emailService)
        {
            _completionRepository = completionRepository;
            _payoutRepository = payoutRepository;
            _unitOfWork = unitOfWork;
            _tutorPayoutService = tutorPayoutService;
            _bookingRepository = bookingRepository;
            _notificationService = notificationService;
            _scheduleRepository = scheduleRepository;
            _emailService = emailService;
        }

        public async Task<int> AutoCompletePastDueAsync()
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTz);
            var dueCompletions = await _completionRepository.GetPendingAutoCompleteAsync(now);
            if (dueCompletions.Count == 0)
                return 0;

            var updated = 0;
            foreach (var completion in dueCompletions)
            {
                completion.Status = (byte)ScheduleCompletionStatus.AutoCompleted;
                completion.AutoCompletedAt = now;
                completion.UpdatedAt = now;
                _completionRepository.Update(completion);

                var schedule = await _scheduleRepository.GetByIdAsync(completion.ScheduleId);
                if (schedule != null && schedule.Status != (int)ScheduleStatus.Completed)
                {
                    schedule.Status = (int)ScheduleStatus.Completed;
                    schedule.UpdatedAt = now;
                    await _scheduleRepository.UpdateAsync(schedule);
                }

                // If a payout exists and is still Pending, move it to ReadyForPayout
                var payout = await _payoutRepository.GetByScheduleIdAsync(completion.ScheduleId);
                if (payout != null && payout.Status == (byte)TutorPayoutStatus.Pending)
                {
                    payout.Status = (byte)TutorPayoutStatus.ReadyForPayout;
                    payout.PayoutTrigger = (byte)TutorPayoutTrigger.AutoCompleted;
                    payout.UpdatedAt = now;
                    _payoutRepository.Update(payout);
                }

                updated++;
            }

            await _unitOfWork.CompleteAsync();
            return updated;
        }

        public async Task<bool> ConfirmAsync(int scheduleId, bool releasePayoutImmediately = true, string? currentUserEmail = null, bool adminAction = false)
        {
            var completion = await _completionRepository.GetByScheduleIdAsync(scheduleId)
                ?? throw new InvalidOperationException("Schedule completion not found.");

            var currentStatus = (ScheduleCompletionStatus)completion.Status;
            if (currentStatus == ScheduleCompletionStatus.LearnerConfirmed)
                return false; // already confirmed

            if (currentStatus == ScheduleCompletionStatus.ReportedOnHold)
                throw new InvalidOperationException("Schedule is reported and cannot be confirmed until resolved.");

            // Ownership check: only learner can confirm
            if (!string.IsNullOrWhiteSpace(currentUserEmail) &&
                !string.Equals(completion.LearnerEmail, currentUserEmail, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Only the learner can confirm this schedule.");
            }

            // Prevent confirming before class starts
            var schedule = await _scheduleRepository.GetByIdAsync(completion.ScheduleId)
                ?? throw new InvalidOperationException("Schedule not found for confirmation.");
            if (schedule.Availabiliti == null || schedule.Availabiliti.Slot == null)
                throw new InvalidOperationException("Schedule availability or slot is missing.");
            var startTime = schedule.Availabiliti.Slot.StartTime;
            var lessonStart = schedule.Availabiliti.StartDate.Date.Add(startTime.ToTimeSpan());
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTz);
            if (now < lessonStart)
                throw new InvalidOperationException("Cannot confirm before the class start time.");

            completion.Status = (byte)ScheduleCompletionStatus.LearnerConfirmed;
            completion.ConfirmedAt = now;
            completion.UpdatedAt = now;
            _completionRepository.Update(completion);

            // Update schedule status to Completed
            if (schedule.Status != (int)ScheduleStatus.Completed)
            {
                schedule.Status = (int)ScheduleStatus.Completed;
                schedule.UpdatedAt = now;
                await _scheduleRepository.UpdateAsync(schedule);
            }

            var payout = await _payoutRepository.GetByScheduleIdAsync(scheduleId);
            if (payout != null)
            {
                // Only move forward if not on hold/cancelled/paid
                var payoutStatus = (TutorPayoutStatus)payout.Status;
                if (payoutStatus == TutorPayoutStatus.OnHold)
                    throw new InvalidOperationException("Payout is on hold; resolve the report first.");

                if (payoutStatus == TutorPayoutStatus.Pending)
                {
                    payout.Status = (byte)TutorPayoutStatus.ReadyForPayout;
                    payout.PayoutTrigger = (byte)(adminAction ? TutorPayoutTrigger.AdminApproved : TutorPayoutTrigger.LearnerConfirmed);
                    payout.UpdatedAt = now;
                    _payoutRepository.Update(payout);
                }
            }

            await _unitOfWork.CompleteAsync();

            if (releasePayoutImmediately && payout != null && payout.Status == (byte)TutorPayoutStatus.ReadyForPayout)
            {
                await _tutorPayoutService.ProcessDuePayoutsAsync();
            }

            var booking = await _bookingRepository.GetByIdAsync(completion.BookingId);
            //await SendNotificationsAsync(booking, completion.ScheduleId,
            //    learnerMessage: $"Bạn đã xác nhận buổi học #{completion.ScheduleId}. Tiền sẽ được giải ngân cho gia sư.",
            //    tutorMessage: $"Học viên đã xác nhận buổi học #{completion.ScheduleId}. Thanh toán sẽ được giải ngân.");
            if (adminAction)
            {
                await SendNotificationsAsync(booking, completion.ScheduleId,
                    learnerMessage: $"Khiếu nại cho buổi học #{completion.ScheduleId} đã được xử lý. Thanh toán cho buổi học này sẽ tiếp tục.",
                    tutorMessage: $"Khiếu nại cho buổi học #{completion.ScheduleId} đã được xử lý.Thanh toán sẽ được giải ngân.");
            }
            else
            {
                await SendNotificationsAsync(booking, completion.ScheduleId,
                learnerMessage: $"Bạn đã xác nhận buổi học #{completion.ScheduleId}. Tiền sẽ được giải ngân cho gia sư.",
                tutorMessage: $"Học viên đã xác nhận buổi học #{completion.ScheduleId}. Thanh toán sẽ được giải ngân.");
            }
            return true;
        }

        // New convenience method; does not change existing confirmation logic.
        public Task<bool> FinishAndPayAsync(int scheduleId, string? currentUserEmail = null, bool adminAction = false)
        {
            return ConfirmAsync(scheduleId, releasePayoutImmediately: true, currentUserEmail: currentUserEmail, adminAction: adminAction);
        }

        public async Task<bool> MarkReportedAsync(int scheduleId, int reportId, string? currentUserEmail = null)
        {
            var completion = await _completionRepository.GetByScheduleIdAsync(scheduleId)
                ?? throw new InvalidOperationException("Schedule completion not found.");

            var currentStatus = (ScheduleCompletionStatus)completion.Status;
            if (currentStatus == ScheduleCompletionStatus.ReportedOnHold)
                return false; // already on hold

            // Ownership check: only learner can report
            if (!string.IsNullOrWhiteSpace(currentUserEmail))
            {
                var isLearner = string.Equals(completion.LearnerEmail, currentUserEmail, StringComparison.OrdinalIgnoreCase);
                if (!isLearner)
                    throw new UnauthorizedAccessException("Only the learner can report this schedule.");
            }

            completion.Status = (byte)ScheduleCompletionStatus.ReportedOnHold;
            completion.ReportId = reportId;
            completion.UpdatedAt = DateTime.UtcNow;
            _completionRepository.Update(completion);

            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule != null && schedule.Status != (int)ScheduleStatus.Completed && schedule.Status != (int)ScheduleStatus.Cancelled)
            {
                schedule.Status = (int)ScheduleStatus.Processing;
                schedule.UpdatedAt = DateTime.UtcNow;
                await _scheduleRepository.UpdateAsync(schedule);
            }

            var payout = await _payoutRepository.GetByScheduleIdAsync(scheduleId);
            if (payout != null)
            {
                payout.Status = (byte)TutorPayoutStatus.OnHold;
                payout.UpdatedAt = DateTime.UtcNow;
                _payoutRepository.Update(payout);
            }

            await _unitOfWork.CompleteAsync();
            var booking = await _bookingRepository.GetByIdAsync(completion.BookingId);
            await SendNotificationsAsync(booking, completion.ScheduleId,
                learnerMessage: $"Bạn đã báo cáo buổi học #{completion.ScheduleId}. Thanh toán đang tạm giữ.",
                tutorMessage: $"Buổi học #{completion.ScheduleId} đã bị báo cáo. Thanh toán đang tạm giữ.");
            return true;
        }

        public async Task<bool> ResolveReportAsync(int scheduleId, bool releaseToTutor)
        {
            var completion = await _completionRepository.GetByScheduleIdAsync(scheduleId)
                ?? throw new InvalidOperationException("Schedule completion not found.");

            var currentStatus = (ScheduleCompletionStatus)completion.Status;
            if (currentStatus != ScheduleCompletionStatus.ReportedOnHold)
                return false; // nothing to resolve

            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTz);
            if (releaseToTutor)
            {
                completion.Status = (byte)ScheduleCompletionStatus.AutoCompleted;
                completion.AutoCompletedAt = now;
                completion.UpdatedAt = now;
                _completionRepository.Update(completion);

                var schedule = await _scheduleRepository.GetByIdAsync(completion.ScheduleId);
                if (schedule != null && schedule.Status != (int)ScheduleStatus.Completed)
                {
                    schedule.Status = (int)ScheduleStatus.Completed;
                    schedule.UpdatedAt = now;
                    await _scheduleRepository.UpdateAsync(schedule);
                }

                var payout = await _payoutRepository.GetByScheduleIdAsync(scheduleId);
                if (payout != null)
                {
                    payout.Status = (byte)TutorPayoutStatus.ReadyForPayout;
                    payout.PayoutTrigger = (byte)TutorPayoutTrigger.AdminApproved;
                    payout.UpdatedAt = now;
                    _payoutRepository.Update(payout);
                }
            }
            else
            {
                completion.Status = (byte)ScheduleCompletionStatus.Cancelled;
                completion.UpdatedAt = now;
                _completionRepository.Update(completion);

                var payout = await _payoutRepository.GetByScheduleIdAsync(scheduleId);
                if (payout != null)
                {
                    payout.Status = (byte)TutorPayoutStatus.Cancelled;
                    payout.UpdatedAt = now;
                    _payoutRepository.Update(payout);
                }
            }

            await _unitOfWork.CompleteAsync();
            var booking = await _bookingRepository.GetByIdAsync(completion.BookingId);
            if (releaseToTutor)
            {
                await SendNotificationsAsync(booking, completion.ScheduleId,
                    learnerMessage: $"Khiếu nại cho buổi học #{completion.ScheduleId} đã được xử lý. Thanh toán sẽ tiếp tục.",
                    tutorMessage: $"Khiếu nại cho buổi học #{completion.ScheduleId} đã được xử lý. Thanh toán sẽ tiếp tục.");
            }
            else
            {
                await SendNotificationsAsync(booking, completion.ScheduleId,
                    learnerMessage: $"Khiếu nại cho buổi học #{completion.ScheduleId} đã được xử lý. Thanh toán cho buổi học này bị hủy.",
                    tutorMessage: $"Khiếu nại cho buổi học #{completion.ScheduleId} đã được xử lý. Thanh toán bị hủy.");
            }
            return true;
        }

        public async Task<bool> CancelAsync(int scheduleId, string? currentUserEmail = null, bool adminAction = false)
        {
            var completion = await _completionRepository.GetByScheduleIdAsync(scheduleId)
                ?? throw new InvalidOperationException("Schedule completion not found.");

            var status = (ScheduleCompletionStatus)completion.Status;
            if (status == ScheduleCompletionStatus.Cancelled)
                return false;

            // Ownership check: learner or tutor can cancel (admin passes null to bypass)
            if (!string.IsNullOrWhiteSpace(currentUserEmail))
            {
                var isLearner = string.Equals(completion.LearnerEmail, currentUserEmail, StringComparison.OrdinalIgnoreCase);
                var tutorEmail = completion.Tutor?.UserEmail;
                var isTutor = !string.IsNullOrWhiteSpace(tutorEmail) &&
                              string.Equals(tutorEmail, currentUserEmail, StringComparison.OrdinalIgnoreCase);
                if (!isLearner && !isTutor)
                    throw new UnauthorizedAccessException("Only the learner or tutor can cancel this schedule.");
            }

            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTz);
            completion.Status = (byte)ScheduleCompletionStatus.Cancelled;
            completion.UpdatedAt = now;
            _completionRepository.Update(completion);

            var schedule = await _scheduleRepository.GetByIdAsync(completion.ScheduleId);
            if (schedule != null && schedule.Status != (int)ScheduleStatus.Cancelled)
            {
                schedule.Status = (int)ScheduleStatus.Cancelled;
                schedule.UpdatedAt = now;
                await _scheduleRepository.UpdateAsync(schedule);
            }

            var payout = await _payoutRepository.GetByScheduleIdAsync(scheduleId);
            if (payout != null && payout.Status != (byte)TutorPayoutStatus.Paid)
            {
                payout.Status = (byte)TutorPayoutStatus.Cancelled;
                payout.UpdatedAt = now;
                _payoutRepository.Update(payout);

                // Return funds to learner (no fee taken)
                var bookingForRefund = await _bookingRepository.GetByIdAsync(completion.BookingId);
                if (bookingForRefund != null && payout.Amount + payout.SystemFeeAmount > 0)
                {
                    var totalRefund = payout.Amount + payout.SystemFeeAmount;
                    var learnerWallet = await _unitOfWork.Wallets.GetWalletByUserEmailAsync(bookingForRefund.LearnerEmail)
                        ?? throw new InvalidOperationException("Learner wallet not found.");
                    var systemWallet = await _unitOfWork.Wallets.GetWalletByUserEmailAsync(SystemWalletEmail)
                        ?? throw new InvalidOperationException("System wallet not found.");

                    if (learnerWallet.LockedBalance < totalRefund || systemWallet.LockedBalance < totalRefund)
                        throw new InvalidOperationException("Locked balances are insufficient to refund.");

                    var learnerBefore = learnerWallet.Balance;
                    learnerWallet.LockedBalance -= totalRefund;
                    learnerWallet.Balance += totalRefund;
                    learnerWallet.UpdatedAt = now;
                    _unitOfWork.Wallets.Update(learnerWallet);

                    var sysLockedBefore = systemWallet.LockedBalance;
                    systemWallet.LockedBalance -= totalRefund;
                    systemWallet.UpdatedAt = now;
                    _unitOfWork.Wallets.Update(systemWallet);

                    // Cập nhật số tiền đã hoàn (không thay đổi PaymentStatus ở đây)
                    bookingForRefund.RefundedAmount += totalRefund;
                    bookingForRefund.UpdatedAt = now;
                    await _bookingRepository.UpdateAsync(bookingForRefund);

                    await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
                    {
                        WalletId = systemWallet.Id,
                        Amount = totalRefund,
                        TransactionType = WalletTransactionType.Debit,
                        Reason = WalletTransactionReason.BookingRefund,
                        Status = TransactionStatus.Completed,
                        BalanceBefore = sysLockedBefore,
                        BalanceAfter = systemWallet.LockedBalance,
                        CreatedAt = now,
                        ReferenceCode = $"SCHEDULE_CANCEL_REFUND_{scheduleId}",
                        BookingId = bookingForRefund.Id
                    });

                    await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
                    {
                        WalletId = learnerWallet.Id,
                        Amount = totalRefund,
                        TransactionType = WalletTransactionType.Credit,
                        Reason = WalletTransactionReason.BookingRefund,
                        Status = TransactionStatus.Completed,
                        BalanceBefore = learnerBefore,
                        BalanceAfter = learnerWallet.Balance,
                        CreatedAt = now,
                        ReferenceCode = $"SCHEDULE_CANCEL_REFUND_{scheduleId}",
                        BookingId = bookingForRefund.Id
                    });
                }
            }

            await _unitOfWork.CompleteAsync();
            var booking = await _bookingRepository.GetByIdAsync(completion.BookingId);
            if (adminAction)
            {
                await SendNotificationsAsync(booking, completion.ScheduleId,
                    learnerMessage: $"Khiếu nại cho buổi học #{completion.ScheduleId} đã được xử lý. Thanh toán cho buổi học này bị hủy.Bạn sẽ được hoàn tiền.",
                    tutorMessage: $"Khiếu nại cho buổi học #{completion.ScheduleId} đã được xử lý. Thanh toán bị hủy.");
            }
            else
            {
                await SendNotificationsAsync(booking, completion.ScheduleId,
                learnerMessage: $"Buổi học #{completion.ScheduleId} đã bị hủy. Thanh toán sẽ không được thực hiện.",
                tutorMessage: $"Buổi học #{completion.ScheduleId} đã bị hủy. Thanh toán sẽ không được thực hiện.");
            }
           
            return true;
        }

        private async Task<string?> BuildScheduleDetailAsync(int scheduleId)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule?.Availabiliti?.Slot == null)
                return null;

            var dateText = schedule.Availabiliti.StartDate.ToString("dd/MM/yyyy");
            var startText = schedule.Availabiliti.Slot.StartTime.ToString(@"hh\:mm");
            var endText = schedule.Availabiliti.Slot.EndTime.ToString(@"hh\:mm");

            return $"ngày {dateText}, {startText} - {endText}";
        }

        private async Task SendNotificationsAsync(Booking? booking, int scheduleId, string? learnerMessage, string? tutorMessage)
        {
            if (booking == null)
                return;

            var detail = await BuildScheduleDetailAsync(scheduleId);
            if (!string.IsNullOrWhiteSpace(detail))
            {
                var idToken = $"#{scheduleId}";
                if (!string.IsNullOrWhiteSpace(learnerMessage))
                {
                    learnerMessage = learnerMessage.Replace(idToken, detail);
                }
                if (!string.IsNullOrWhiteSpace(tutorMessage))
                {
                    tutorMessage = tutorMessage.Replace(idToken, detail);
                }
            }

            if (!string.IsNullOrWhiteSpace(learnerMessage))
            {
                await _notificationService.CreateNotificationAsync(booking.LearnerEmail, learnerMessage, "/schedules");
                await _emailService.SendMailAsync(new MailContent
                {
                    To = booking.LearnerEmail,
                    Subject = "Cập nhật buổi học",
                    Body = learnerMessage
                });
            }

            var tutorEmail = booking.TutorSubject?.Tutor?.UserEmail;
            if (!string.IsNullOrWhiteSpace(tutorEmail) && !string.IsNullOrWhiteSpace(tutorMessage))
            {
                await _notificationService.CreateNotificationAsync(tutorEmail, tutorMessage, "/schedules");
                await _emailService.SendMailAsync(new MailContent
                {
                    To = tutorEmail,
                    Subject = "Cập nhật buổi học",
                    Body = tutorMessage
                });
            }
        }
    }
}

