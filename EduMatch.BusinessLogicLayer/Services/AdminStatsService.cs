using EduMatch.BusinessLogicLayer.Constants;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class AdminStatsService : IAdminStatsService
    {
        private readonly EduMatchContext _context;
        private readonly IAdminWalletService _adminWalletService;

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

            var totalUsersTask = usersQuery.CountAsync();
            var activeUsersTask = usersQuery.CountAsync(u => u.IsActive == true);
            var learnersTask = usersQuery.CountAsync(u => u.Role.RoleName == Roles.Learner);
            var tutorsTask = usersQuery.CountAsync(u => u.Role.RoleName == Roles.Tutor);
            var newUsers30Task = usersQuery.CountAsync(u => u.CreatedAt >= cutoff30Days);

            var tutorApprovedTask = tutorProfilesQuery.CountAsync(t => t.Status == (int)TutorStatus.Approved);
            var tutorPendingTask = tutorProfilesQuery.CountAsync(t => t.Status == (int)TutorStatus.Pending);
            var tutorRejectedTask = tutorProfilesQuery.CountAsync(t => t.Status == (int)TutorStatus.Rejected);
            var tutorSuspendedTask = tutorProfilesQuery.CountAsync(t => t.Status == (int)TutorStatus.Suspended);
            var tutorDeactivatedTask = tutorProfilesQuery.CountAsync(t => t.Status == (int)TutorStatus.Deactivated);

            var bookingTotalTask = bookingsQuery.CountAsync();
            var bookingPendingTask = bookingsQuery.CountAsync(b => b.Status == (int)BookingStatus.Pending);
            var bookingConfirmedTask = bookingsQuery.CountAsync(b => b.Status == (int)BookingStatus.Confirmed);
            var bookingCompletedTask = bookingsQuery.CountAsync(b => b.Status == (int)BookingStatus.Completed);
            var bookingCancelledTask = bookingsQuery.CountAsync(b => b.Status == (int)BookingStatus.Cancelled);

            var refundPendingTask = refundsQuery.CountAsync(r => r.Status == (int)BookingRefundRequestStatus.Pending);
            var refundApprovedTask = refundsQuery.CountAsync(r => r.Status == (int)BookingRefundRequestStatus.Approved);
            var refundRejectedTask = refundsQuery.CountAsync(r => r.Status == (int)BookingRefundRequestStatus.Rejected);

            var reportPendingTask = reportsQuery.CountAsync(r => r.Status == (int)ReportStatus.Pending);
            var reportUnderReviewTask = reportsQuery.CountAsync(r => r.Status == (int)ReportStatus.UnderReview);
            var reportResolvedTask = reportsQuery.CountAsync(r => r.Status == (int)ReportStatus.Resolved);
            var reportDismissedTask = reportsQuery.CountAsync(r => r.Status == (int)ReportStatus.Dismissed);
            var reportOverdueTask = reportsQuery.CountAsync(r =>
                r.Status == (int)ReportStatus.Pending && r.CreatedAt <= now.AddDays(-2));

            var walletTask = _adminWalletService.GetSystemWalletDashboardAsync();

            await Task.WhenAll(
                totalUsersTask, activeUsersTask, learnersTask, tutorsTask, newUsers30Task,
                tutorApprovedTask, tutorPendingTask, tutorRejectedTask, tutorSuspendedTask, tutorDeactivatedTask,
                bookingTotalTask, bookingPendingTask, bookingConfirmedTask, bookingCompletedTask, bookingCancelledTask,
                refundPendingTask, refundApprovedTask, refundRejectedTask,
                reportPendingTask, reportUnderReviewTask, reportResolvedTask, reportDismissedTask, reportOverdueTask,
                walletTask
            );

            return new AdminSummaryStatsDto
            {
                Users = new UserStatsDto
                {
                    Total = totalUsersTask.Result,
                    Active = activeUsersTask.Result,
                    Learners = learnersTask.Result,
                    Tutors = tutorsTask.Result,
                    NewLast30Days = newUsers30Task.Result
                },
                Tutors = new TutorStatsDto
                {
                    Approved = tutorApprovedTask.Result,
                    Pending = tutorPendingTask.Result,
                    Rejected = tutorRejectedTask.Result,
                    Suspended = tutorSuspendedTask.Result,
                    Deactivated = tutorDeactivatedTask.Result
                },
                Bookings = new BookingStatsDto
                {
                    Total = bookingTotalTask.Result,
                    Pending = bookingPendingTask.Result,
                    Confirmed = bookingConfirmedTask.Result,
                    Completed = bookingCompletedTask.Result,
                    Cancelled = bookingCancelledTask.Result
                },
                Revenue = new RevenueStatsDto
                {
                    PlatformRevenueBalance = walletTask.Result.PlatformRevenueBalance,
                    PendingTutorPayoutBalance = walletTask.Result.PendingTutorPayoutBalance,
                    TotalUserAvailableBalance = walletTask.Result.TotalUserAvailableBalance
                },
                Refunds = new RefundStatsDto
                {
                    Pending = refundPendingTask.Result,
                    Approved = refundApprovedTask.Result,
                    Rejected = refundRejectedTask.Result
                },
                Reports = new ReportStatsDto
                {
                    Pending = reportPendingTask.Result,
                    UnderReview = reportUnderReviewTask.Result,
                    Resolved = reportResolvedTask.Result,
                    Dismissed = reportDismissedTask.Result,
                    OverduePending = reportOverdueTask.Result
                }
            };
        }
    }
}
