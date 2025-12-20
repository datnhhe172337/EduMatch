using EduMatch.BusinessLogicLayer.Constants;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class AdminStatsService : IAdminStatsService
    {
        private readonly EduMatchContext _context;
        private readonly IAdminWalletService _adminWalletService;
        private const string SystemWalletEmail = "system@edumatch.com";

        public AdminStatsService(EduMatchContext context, IAdminWalletService adminWalletService)
        {
            _context = context;
            _adminWalletService = adminWalletService;
        }

        public async Task<AdminSummaryStatsDto> GetSummaryAsync(DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            var now = DateTime.UtcNow;
            var cutoff30Days = now.AddDays(-30);

            var usersQuery = _context.Users.AsNoTracking();
            var tutorProfilesQuery = _context.TutorProfiles.AsNoTracking();
            var bookingsQuery = _context.Bookings.AsNoTracking();
            var refundsQuery = _context.BookingRefundRequests.AsNoTracking();
            var reportsQuery = _context.Reports.AsNoTracking();

            var scopedUsersQuery = usersQuery.Where(u =>
                u.Role.RoleName == Roles.Learner || u.Role.RoleName == Roles.Tutor);

            var totalUsers = await scopedUsersQuery.CountAsync();
            var activeUsers = await scopedUsersQuery.CountAsync(u => u.IsActive == true);
            var learners = await scopedUsersQuery.CountAsync(u => u.Role.RoleName == Roles.Learner);
            var tutors = await scopedUsersQuery.CountAsync(u => u.Role.RoleName == Roles.Tutor);
            var newUsers30 = await scopedUsersQuery.CountAsync(u => u.CreatedAt >= cutoff30Days);

            var tutorApproved = await tutorProfilesQuery.CountAsync(t => t.Status == (int)TutorStatus.Approved);
            var tutorPending = await tutorProfilesQuery.CountAsync(t => t.Status == (int)TutorStatus.Pending);
            var tutorRejected = await tutorProfilesQuery.CountAsync(t => t.Status == (int)TutorStatus.Rejected);
            var tutorSuspended = await tutorProfilesQuery.CountAsync(t => t.Status == (int)TutorStatus.Suspended);
            var tutorDeactivated = await tutorProfilesQuery.CountAsync(t => t.Status == (int)TutorStatus.Deactivated);

            var bookingTotal = await bookingsQuery.CountAsync();
            var bookingPending = await bookingsQuery.CountAsync(b => b.Status == (int)BookingStatus.Pending);
            var bookingConfirmed = await bookingsQuery.CountAsync(b => b.Status == (int)BookingStatus.Confirmed);
            var bookingCompleted = await bookingsQuery.CountAsync(b => b.Status == (int)BookingStatus.Completed);
            var bookingCancelled = await bookingsQuery.CountAsync(b => b.Status == (int)BookingStatus.Cancelled);

            var refundPending = await refundsQuery.CountAsync(r => r.Status == (int)BookingRefundRequestStatus.Pending);
            var refundApproved = await refundsQuery.CountAsync(r => r.Status == (int)BookingRefundRequestStatus.Approved);
            var refundRejected = await refundsQuery.CountAsync(r => r.Status == (int)BookingRefundRequestStatus.Rejected);

            var reportPending = await reportsQuery.CountAsync(r => r.Status == (int)ReportStatus.Pending);
            var reportUnderReview = await reportsQuery.CountAsync(r => r.Status == (int)ReportStatus.UnderReview);
            var reportResolved = await reportsQuery.CountAsync(r => r.Status == (int)ReportStatus.Resolved);
            var reportDismissed = await reportsQuery.CountAsync(r => r.Status == (int)ReportStatus.Dismissed);
            var reportOverdue = await reportsQuery.CountAsync(r =>
                r.Status == (int)ReportStatus.Pending && r.CreatedAt <= now.AddDays(-2));

            var wallet = await _adminWalletService.GetSystemWalletDashboardAsync();

            return new AdminSummaryStatsDto
            {
                Users = new UserStatsDto
                {
                    Total = totalUsers,
                    Active = activeUsers,
                    Learners = learners,
                    Tutors = tutors,
                    NewLast30Days = newUsers30
                },
                Tutors = new TutorStatsDto
                {
                    Approved = tutorApproved,
                    Pending = tutorPending,
                    Rejected = tutorRejected,
                    Suspended = tutorSuspended,
                    Deactivated = tutorDeactivated
                },
                Bookings = new BookingStatsDto
                {
                    Total = bookingTotal,
                    Pending = bookingPending,
                    Confirmed = bookingConfirmed,
                    Completed = bookingCompleted,
                    Cancelled = bookingCancelled
                },
                Revenue = new RevenueStatsDto
                {
                    PlatformRevenueBalance = wallet.PlatformRevenueBalance,
                    PendingTutorPayoutBalance = wallet.PendingTutorPayoutBalance,
                    TotalUserAvailableBalance = wallet.TotalUserAvailableBalance
                },
                Refunds = new RefundStatsDto
                {
                    Pending = refundPending,
                    Approved = refundApproved,
                    Rejected = refundRejected
                },
                Reports = new ReportStatsDto
                {
                    Pending = reportPending,
                    UnderReview = reportUnderReview,
                    Resolved = reportResolved,
                    Dismissed = reportDismissed,
                    OverduePending = reportOverdue
                }
            };
        }

        // Trend endpoints are disabled
        // public async Task<IReadOnlyList<SignupTrendPointDto>> GetSignupTrendAsync(...)
        // public async Task<IReadOnlyList<BookingTrendPointDto>> GetBookingTrendAsync(...)

        public async Task<IReadOnlyList<MonthlyAdminStatsDto>> GetMonthlyStatsAsync(int year)
        {
            var start = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddYears(1);

            var users = await _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .Where(u => u.CreatedAt >= start && u.CreatedAt < end)
                .ToListAsync();

            var bookings = await _context.Bookings
                .AsNoTracking()
                .Where(b => b.CreatedAt >= start && b.CreatedAt < end)
                .ToListAsync();

            var systemWallet = await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserEmail == SystemWalletEmail);
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            var systemWalletBalance = systemWallet?.Balance ?? 0m;

            var results = new List<MonthlyAdminStatsDto>();

            for (var month = 1; month <= 12; month++)
            {
                var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var monthEnd = monthStart.AddMonths(1);

                var monthUsers = users.Where(u => u.CreatedAt >= monthStart && u.CreatedAt < monthEnd);
                var monthBookings = bookings.Where(b => b.CreatedAt >= monthStart && b.CreatedAt < monthEnd);
                var monthSystemTx = await _context.WalletTransactions
                    .AsNoTracking()
                    .Include(tx => tx.Wallet)
                    .Where(tx =>
                        tx.Wallet.UserEmail == SystemWalletEmail &&
                        tx.Status == TransactionStatus.Completed &&
                        tx.CreatedAt >= monthStart &&
                        tx.CreatedAt < monthEnd)
                    .ToListAsync();

                results.Add(new MonthlyAdminStatsDto
                {
                    Year = year,
                    Month = month,
                    Users = new UserStatsDto
                    {
                        Total = monthUsers.Count(),
                        Active = monthUsers.Count(u => u.IsActive == true),
                        Learners = monthUsers.Count(u => u.Role.RoleName == Roles.Learner),
                        Tutors = monthUsers.Count(u => u.Role.RoleName == Roles.Tutor),
                        NewLast30Days = monthUsers.Count() // per-month new == total for the bucket
                    },
                    Bookings = new BookingStatsDto
                    {
                        Total = monthBookings.Count(),
                        Pending = monthBookings.Count(b => b.Status == (int)BookingStatus.Pending),
                        Confirmed = monthBookings.Count(b => b.Status == (int)BookingStatus.Confirmed),
                        Completed = monthBookings.Count(b => b.Status == (int)BookingStatus.Completed),
                        Cancelled = monthBookings.Count(b => b.Status == (int)BookingStatus.Cancelled)
                    },
                    Revenue = new MonthlyRevenueStatsDto
                    {
                        TutorPayoutAmount = monthSystemTx
                            .Where(tx => tx.Reason == WalletTransactionReason.BookingPayout && tx.TransactionType == WalletTransactionType.Debit)
                            .Sum(tx => tx.Amount),
                        RefundedAmount = monthSystemTx
                            .Where(tx => tx.Reason == WalletTransactionReason.BookingRefund && tx.TransactionType == WalletTransactionType.Debit)
                            .Sum(tx => tx.Amount),
                        // For the current month, surface the system wallet's available balance as platform revenue.
                        // For other months (historical), fall back to platform fee movements within that month.
                        NetPlatformRevenueAmount = (year == currentYear && month == currentMonth)
                            ? systemWalletBalance
                            : monthSystemTx
                                .Where(tx => tx.Reason == WalletTransactionReason.PlatformFee)
                                .Sum(tx => tx.TransactionType == WalletTransactionType.Credit ? tx.Amount : -tx.Amount)
                    }
                });
            }

            return results;
        }
    }
}
