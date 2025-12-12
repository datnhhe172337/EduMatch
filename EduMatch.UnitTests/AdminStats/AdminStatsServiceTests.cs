using System;
using System.Linq;
using System.Threading.Tasks;
using EduMatch.BusinessLogicLayer.Constants;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EduMatch.UnitTests.AdminStats
{
    public class AdminStatsServiceTests
    {
        private EduMatchContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<EduMatchContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new InMemoryEduMatchContext(options);
        }

        private static AdminStatsService CreateService(EduMatchContext context)
        {
            var walletMock = new Mock<IAdminWalletService>();
            walletMock.Setup(w => w.GetSystemWalletDashboardAsync())
                .ReturnsAsync(new SystemWalletDashboardDto());

            return new AdminStatsService(context, walletMock.Object);
        }

        [Fact]
        public async Task GetSummaryAsync_ReturnsExpectedCountsAndBalances()
        {
            using var context = CreateContext(nameof(GetSummaryAsync_ReturnsExpectedCountsAndBalances));

            // Roles
            var tutorRole = new Role { Id = 1, RoleName = Roles.Tutor };
            var learnerRole = new Role { Id = 2, RoleName = Roles.Learner };
            context.Roles.AddRange(tutorRole, learnerRole);

            // Users
            context.Users.AddRange(
                new User { Email = "t1@test.com", RoleId = tutorRole.Id, Role = tutorRole, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-1), LoginProvider = "Local", PasswordHash = "x" },
                new User { Email = "t2@test.com", RoleId = tutorRole.Id, Role = tutorRole, IsActive = false, CreatedAt = DateTime.UtcNow.AddDays(-10), LoginProvider = "Local", PasswordHash = "x" },
                new User { Email = "l1@test.com", RoleId = learnerRole.Id, Role = learnerRole, IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-2), LoginProvider = "Local", PasswordHash = "x" }
            );

            // Tutor profiles
            context.TutorProfiles.AddRange(
                new TutorProfile { UserEmail = "t1@test.com", Status = (int)TutorStatus.Approved, CreatedAt = DateTime.UtcNow },
                new TutorProfile { UserEmail = "t2@test.com", Status = (int)TutorStatus.Pending, CreatedAt = DateTime.UtcNow }
            );

            // Bookings
            context.Bookings.AddRange(
                new Booking { Id = 1, LearnerEmail = "l1@test.com", TutorSubjectId = 1, TotalAmount = 100, SystemFeeAmount = 10, TutorReceiveAmount = 90, Status = (int)BookingStatus.Pending, CreatedAt = DateTime.UtcNow },
                new Booking { Id = 2, LearnerEmail = "l1@test.com", TutorSubjectId = 1, TotalAmount = 200, SystemFeeAmount = 20, TutorReceiveAmount = 180, Status = (int)BookingStatus.Cancelled, CreatedAt = DateTime.UtcNow }
            );

            // Refund requests
            context.BookingRefundRequests.AddRange(
                new BookingRefundRequest { Id = 1, BookingId = 1, LearnerEmail = "l1@test.com", RefundPolicyId = 1, Status = (int)BookingRefundRequestStatus.Approved, CreatedAt = DateTime.UtcNow },
                new BookingRefundRequest { Id = 2, BookingId = 2, LearnerEmail = "l1@test.com", RefundPolicyId = 1, Status = (int)BookingRefundRequestStatus.Rejected, CreatedAt = DateTime.UtcNow }
            );

            // Reports (with one overdue pending)
            context.Reports.AddRange(
                new Report { Id = 1, ReporterUserEmail = "l1@test.com", ReportedUserEmail = "t1@test.com", Reason = "r1", Status = (int)ReportStatus.Pending, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new Report { Id = 2, ReporterUserEmail = "l1@test.com", ReportedUserEmail = "t1@test.com", Reason = "r2", Status = (int)ReportStatus.Resolved, CreatedAt = DateTime.UtcNow }
            );

            await context.SaveChangesAsync();

            var walletMock = new Mock<IAdminWalletService>();
            walletMock.Setup(w => w.GetSystemWalletDashboardAsync())
                .ReturnsAsync(new SystemWalletDashboardDto
                {
                    PlatformRevenueBalance = 50,
                    PendingTutorPayoutBalance = 150,
                    TotalUserAvailableBalance = 1_000
                });

            var service = new AdminStatsService(context, walletMock.Object);

            var summary = await service.GetSummaryAsync();

            summary.Users.Total.Should().Be(3);
            summary.Users.Active.Should().Be(2);
            summary.Users.Tutors.Should().Be(2);
            summary.Users.Learners.Should().Be(1);
            summary.Tutors.Approved.Should().Be(1);
            summary.Tutors.Pending.Should().Be(1);
            summary.Bookings.Total.Should().Be(2);
            summary.Refunds.Approved.Should().Be(1);
            summary.Refunds.Rejected.Should().Be(1);
            summary.Reports.Pending.Should().Be(1);
            summary.Reports.Resolved.Should().Be(1);
            summary.Reports.OverduePending.Should().Be(1);
            summary.Revenue.PlatformRevenueBalance.Should().Be(50);
            summary.Revenue.PendingTutorPayoutBalance.Should().Be(150);
            summary.Revenue.TotalUserAvailableBalance.Should().Be(1_000);
        }

        [Fact]
        public async Task GetMonthlyStatsAsync_CompletedPaidBooking_NoRefund_ReturnsSystemFeeAsNet()
        {
            using var context = CreateContext(nameof(GetMonthlyStatsAsync_CompletedPaidBooking_NoRefund_ReturnsSystemFeeAsNet));
            context.Bookings.Add(new Booking
            {
                Id = 1,
                LearnerEmail = "learner@test.com",
                TutorSubjectId = 1,
                TotalSessions = 1,
                UnitPrice = 100_000m,
                TotalAmount = 100_000m,
                SystemFeeAmount = 10_000m,
                TutorReceiveAmount = 90_000m,
                PaymentStatus = (int)PaymentStatus.Paid,
                Status = (int)BookingStatus.Completed,
                CreatedAt = new DateTime(2025, 11, 10, 0, 0, 0, DateTimeKind.Utc)
            });
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetMonthlyStatsAsync(2025);
            var november = result.Single(r => r.Month == 11);

            november.Revenue.NetPlatformRevenueAmount.Should().Be(10_000m);
        }

        [Fact]
        public async Task GetMonthlyStatsAsync_FullyRefunded_ReturnsZeroNet()
        {
            using var context = CreateContext(nameof(GetMonthlyStatsAsync_FullyRefunded_ReturnsZeroNet));
            context.Bookings.Add(new Booking
            {
                Id = 2,
                LearnerEmail = "learner@test.com",
                TutorSubjectId = 1,
                TotalSessions = 1,
                UnitPrice = 200_000m,
                TotalAmount = 200_000m,
                SystemFeeAmount = 20_000m,
                TutorReceiveAmount = 0m,
                PaymentStatus = (int)PaymentStatus.Refunded,
                RefundedAmount = 200_000m,
                Status = (int)BookingStatus.Cancelled,
                CreatedAt = new DateTime(2025, 11, 12, 0, 0, 0, DateTimeKind.Utc)
            });
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetMonthlyStatsAsync(2025);
            var november = result.Single(r => r.Month == 11);

            november.Revenue.NetPlatformRevenueAmount.Should().Be(0m);
        }

        [Fact]
        public async Task GetMonthlyStatsAsync_PartialRefund_UsesProratedFee()
        {
            using var context = CreateContext(nameof(GetMonthlyStatsAsync_PartialRefund_UsesProratedFee));
            context.Bookings.Add(new Booking
            {
                Id = 3,
                LearnerEmail = "learner@test.com",
                TutorSubjectId = 1,
                TotalSessions = 1,
                UnitPrice = 300_000m,
                TotalAmount = 300_000m,
                SystemFeeAmount = 30_000m,
                TutorReceiveAmount = 270_000m,
                PaymentStatus = (int)PaymentStatus.RefundPending,
                RefundedAmount = 150_000m,
                Status = (int)BookingStatus.Completed,
                CreatedAt = new DateTime(2025, 11, 15, 0, 0, 0, DateTimeKind.Utc)
            });
            await context.SaveChangesAsync();

            var service = CreateService(context);

            var result = await service.GetMonthlyStatsAsync(2025);
            var november = result.Single(r => r.Month == 11);

            // 30k fee, 50% refunded => keep 15k
            november.Revenue.NetPlatformRevenueAmount.Should().Be(15_000m);
        }

        [Fact]
        public async Task GetMonthlyStatsAsync_PendingOrNotCompleted_ReturnsZeroNet()
        {
            using var context = CreateContext(nameof(GetMonthlyStatsAsync_PendingOrNotCompleted_ReturnsZeroNet));
            context.Bookings.AddRange(
                new Booking
                {
                    Id = 4,
                    LearnerEmail = "learner@test.com",
                    TutorSubjectId = 1,
                    TotalAmount = 100_000m,
                    SystemFeeAmount = 10_000m,
                    TutorReceiveAmount = 90_000m,
                    PaymentStatus = (int)PaymentStatus.Pending,
                    Status = (int)BookingStatus.Completed,
                    CreatedAt = new DateTime(2025, 11, 18, 0, 0, 0, DateTimeKind.Utc)
                },
                new Booking
                {
                    Id = 5,
                    LearnerEmail = "learner@test.com",
                    TutorSubjectId = 1,
                    TotalAmount = 120_000m,
                    SystemFeeAmount = 12_000m,
                    TutorReceiveAmount = 108_000m,
                    PaymentStatus = (int)PaymentStatus.Paid,
                    Status = (int)BookingStatus.Pending,
                    CreatedAt = new DateTime(2025, 11, 19, 0, 0, 0, DateTimeKind.Utc)
                }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);
            var result = await service.GetMonthlyStatsAsync(2025);
            var november = result.Single(r => r.Month == 11);

            november.Revenue.NetPlatformRevenueAmount.Should().Be(0m);
        }

        [Fact]
        public async Task GetMonthlyStatsAsync_EmptyData_ReturnsTwelveZeroMonths()
        {
            using var context = CreateContext(nameof(GetMonthlyStatsAsync_EmptyData_ReturnsTwelveZeroMonths));
            var service = CreateService(context);

            var result = await service.GetMonthlyStatsAsync(2025);

            result.Should().HaveCount(12);
            result.Should().OnlyContain(m =>
                m.Users.Total == 0 &&
                m.Bookings.Total == 0 &&
                m.Revenue.NetPlatformRevenueAmount == 0m &&
                m.Revenue.RefundedAmount == 0m &&
                m.Revenue.TutorPayoutAmount == 0m);
        }

        private sealed class InMemoryEduMatchContext : EduMatchContext
        {
            public InMemoryEduMatchContext(DbContextOptions<EduMatchContext> options) : base(options) { }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                // Intentionally no-op to avoid overriding in-memory provider in tests.
            }
        }
    }
}
