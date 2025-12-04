using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EduMatch.BusinessLogicLayer.Interfaces;
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
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TutorPayoutService(
            ITutorPayoutRepository tutorPayoutRepository,
            IBookingRepository bookingRepository,
            IWalletRepository walletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            IUnitOfWork unitOfWork)
        {
            _tutorPayoutRepository = tutorPayoutRepository;
            _bookingRepository = bookingRepository;
            _walletRepository = walletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _unitOfWork = unitOfWork;
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

                if (payout.SystemFeeAmount > 0)
                {
                    var sysTx = new WalletTransaction
                    {
                        WalletId = systemWallet.Id,
                        Amount = payout.SystemFeeAmount,
                        TransactionType = WalletTransactionType.Credit,
                        Reason = WalletTransactionReason.PlatformFee,
                        Status = TransactionStatus.Completed,
                        BalanceBefore = systemBalanceBefore,
                        BalanceAfter = systemWallet.Balance,
                        CreatedAt = now,
                        ReferenceCode = $"BOOKING_PLATFORM_FEE_{payout.BookingId}",
                        BookingId = payout.BookingId
                    };
                    await _walletTransactionRepository.AddAsync(sysTx);
                }

                payout.Status = (byte)TutorPayoutStatus.Paid;
                payout.ReleasedAt = now;
                payout.WalletTransaction = transaction;
                _tutorPayoutRepository.Update(payout);

                processedCount++;
            }

            await _unitOfWork.CompleteAsync();
            return processedCount;
        }
    }
}
