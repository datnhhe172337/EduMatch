using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class AdminSummaryStatsDto
    {
        public UserStatsDto Users { get; set; } = new();
        public TutorStatsDto Tutors { get; set; } = new();
        public BookingStatsDto Bookings { get; set; } = new();
        public RevenueStatsDto Revenue { get; set; } = new();
        public RefundStatsDto Refunds { get; set; } = new();
        public ReportStatsDto Reports { get; set; } = new();
    }

    public class UserStatsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Learners { get; set; }
        public int Tutors { get; set; }
        public int NewLast30Days { get; set; }
    }

    public class TutorStatsDto
    {
        public int Approved { get; set; }
        public int Pending { get; set; }
        public int Rejected { get; set; }
        public int Suspended { get; set; }
        public int Deactivated { get; set; }
    }

    public class BookingStatsDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }

    public class RevenueStatsDto
    {
        public decimal PlatformRevenueBalance { get; set; }
        public decimal PendingTutorPayoutBalance { get; set; }
        public decimal TotalUserAvailableBalance { get; set; }
    }

    public class RefundStatsDto
    {
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
    }

    public class ReportStatsDto
    {
        public int Pending { get; set; }
        public int UnderReview { get; set; }
        public int Resolved { get; set; }
        public int Dismissed { get; set; }
        public int OverduePending { get; set; }
    }
}
