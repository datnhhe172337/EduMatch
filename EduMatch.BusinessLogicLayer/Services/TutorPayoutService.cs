using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class TutorPayoutService : ITutorPayoutService
    {
        private const string SystemWalletEmail = "system@edumatch.com";

        private readonly ITutorPayoutRepository _tutorPayoutRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly INotificationService _notificationService;
        private readonly EmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;

        public TutorPayoutService(
            ITutorPayoutRepository tutorPayoutRepository,
            IBookingRepository bookingRepository,
            IScheduleRepository scheduleRepository,
            IWalletRepository walletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            INotificationService notificationService,
            EmailService emailService,
            IUnitOfWork unitOfWork)
        {
            _tutorPayoutRepository = tutorPayoutRepository;
            _bookingRepository = bookingRepository;
            _scheduleRepository = scheduleRepository;
            _walletRepository = walletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _notificationService = notificationService;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<TutorPayoutDto>> GetByBookingIdAsync(int bookingId)
        {
            if (bookingId <= 0)
                throw new ArgumentException("BookingId must be greater than 0.");

            var payouts = await _tutorPayoutRepository.GetByBookingIdAsync(bookingId);
            var result = new List<TutorPayoutDto>(payouts.Count);
            foreach (var p in payouts)
            {
                result.Add(new TutorPayoutDto
                {
                    Id = p.Id,
                    ScheduleId = p.ScheduleId,
                    BookingId = p.BookingId,
                    Amount = p.Amount,
                    SystemFeeAmount = p.SystemFeeAmount,
                    Status = (TutorPayoutStatus) p.Status,
                    PayoutTrigger = p.PayoutTrigger,
                    ScheduledPayoutDate = p.ScheduledPayoutDate,
                    ReleasedAt = p.ReleasedAt,
                    WalletTransactionId = p.WalletTransactionId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });
            }
            return result;
        }

        public async Task<int> ProcessDuePayoutsAsync()
        {
            var vietnamTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTz));
            var readyPayouts = await _tutorPayoutRepository.GetReadyForPayoutAsync(today);
            if (readyPayouts.Count == 0)
                return 0;

            var processedCount = 0;
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTz);

            // Ensure system wallet is available for balance adjustments
            var systemWallet = await _walletRepository.GetWalletByUserEmailAsync(SystemWalletEmail);
            if (systemWallet == null)
                throw new InvalidOperationException("System wallet not found.");

            foreach (var payout in readyPayouts)
            {
                var tutorWallet = await _walletRepository.GetByIdAsync(payout.TutorWalletId)
                    ?? throw new InvalidOperationException($"Tutor wallet not found for payout {payout.Id}.");

                // Fetch booking to release learner locked balance
                var booking = await _bookingRepository.GetByIdAsync(payout.BookingId)
                    ?? throw new InvalidOperationException($"Booking not found for payout {payout.Id}.");

                var totalToRelease = payout.Amount + payout.SystemFeeAmount;

                if (systemWallet.LockedBalance < totalToRelease)
                    throw new InvalidOperationException("System locked balance is insufficient to release payout.");

                var learnerWallet = await _walletRepository.GetWalletByUserEmailAsync(booking.LearnerEmail)
                    ?? throw new InvalidOperationException("Learner wallet not found for payout release.");
                if (learnerWallet.LockedBalance < totalToRelease)
                    throw new InvalidOperationException("Learner locked balance is insufficient to release payout.");

                var tutorBalanceBefore = tutorWallet.Balance;
                tutorWallet.Balance += payout.Amount;
                tutorWallet.UpdatedAt = now;
                _walletRepository.Update(tutorWallet);

                learnerWallet.LockedBalance -= totalToRelease;
                learnerWallet.UpdatedAt = now;
                _walletRepository.Update(learnerWallet);

                var systemBalanceBefore = systemWallet.Balance;
                var systemLockedBefore = systemWallet.LockedBalance;
                
                systemWallet.LockedBalance -= totalToRelease;
                if (payout.SystemFeeAmount > 0)
                {
                    systemWallet.Balance += payout.SystemFeeAmount;
                }
                systemWallet.UpdatedAt = now;
                _walletRepository.Update(systemWallet);

                var transaction = new WalletTransaction
                {
                    WalletId = tutorWallet.Id,
                    Amount = payout.Amount,
                    TransactionType = WalletTransactionType.Credit,
                    Reason = WalletTransactionReason.BookingPayout,
                    Status = TransactionStatus.Completed,
                    BalanceBefore = tutorBalanceBefore,
                    BalanceAfter = tutorWallet.Balance,
                    CreatedAt = now,
                    ReferenceCode = $"BOOKING_PAYOUT_{payout.BookingId}",
                    BookingId = payout.BookingId
                };

                await _walletTransactionRepository.AddAsync(transaction);

                await _walletTransactionRepository.AddAsync(new WalletTransaction
                {
                    WalletId = systemWallet.Id,
                    Amount = totalToRelease,
                    TransactionType = WalletTransactionType.Debit,
                    Reason = WalletTransactionReason.BookingPayout,
                    Status = TransactionStatus.Completed,
                    BalanceBefore = systemLockedBefore,
                    BalanceAfter = systemWallet.LockedBalance,
                    CreatedAt = now,
                    ReferenceCode = $"BOOKING_PAYOUT_LOCK_{payout.BookingId}",
                    BookingId = payout.BookingId
                });

                if (payout.SystemFeeAmount > 0)
                {
                    await _walletTransactionRepository.AddAsync(new WalletTransaction
                    {
                        WalletId = systemWallet.Id,
                        Amount = payout.SystemFeeAmount,
                        TransactionType = WalletTransactionType.Credit,
                        Reason = WalletTransactionReason.PlatformFee,
                        Status = TransactionStatus.Completed,
                        BalanceBefore = systemBalanceBefore,
                        BalanceAfter = systemWallet.Balance,
                        CreatedAt = now,
                        ReferenceCode = $"BOOKING_PAYOUT_FEE_{payout.BookingId}",
                        BookingId = payout.BookingId
                    });
                }

                payout.Status = (byte)TutorPayoutStatus.Paid;
                payout.ReleasedAt = now;
                payout.WalletTransaction = transaction;
                _tutorPayoutRepository.Update(payout);

                processedCount++;

                // Notify tutor
                await NotifyTutorAsync(booking, payout.Amount, scheduleId: payout.ScheduleId);
            }

            await _unitOfWork.CompleteAsync();
            return processedCount;
        }

        private async Task NotifyTutorAsync(Booking booking, decimal amount, int? scheduleId)
        {
            var tutorEmail = booking.TutorSubject?.Tutor?.UserEmail;
            if (string.IsNullOrWhiteSpace(tutorEmail))
                return;

            string? scheduleDetail = null;
            if (scheduleId.HasValue)
            {
                var schedule = await _scheduleRepository.GetByIdAsync(scheduleId.Value);
                if (schedule?.Availabiliti?.Slot != null)
                {
                    var dateText = schedule.Availabiliti.StartDate.ToString("dd/MM/yyyy");
                    var startText = schedule.Availabiliti.Slot.StartTime.ToString(@"hh\:mm");
                    var endText = schedule.Availabiliti.Slot.EndTime.ToString(@"hh\:mm");
                    scheduleDetail = $"Ngày {dateText}, {startText} - {endText}";
                }
            }

            string message;
            if (scheduleId.HasValue)
            {
                var detailText = !string.IsNullOrWhiteSpace(scheduleDetail)
                    ? scheduleDetail
                    : $"buổi học #{scheduleId.Value}";

                message = $"Bạn đã nhận được {amount:N0} VND cho {detailText}.";
            }
            else
            {
                message = $"Bạn đã nhận {amount:N0} VND từ payout.";
            }

            await _notificationService.CreateNotificationAsync(tutorEmail, message, "/wallet/my-wallet");
            await _emailService.SendMailAsync(new MailContent
            {
                To = tutorEmail,
                Subject = "Thanh toán buổi học",
                Body = message
            });
        }
    }
}

