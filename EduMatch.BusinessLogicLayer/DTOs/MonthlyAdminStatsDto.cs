namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class MonthlyAdminStatsDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public UserStatsDto Users { get; set; } = new();
        public BookingStatsDto Bookings { get; set; } = new();
        public MonthlyRevenueStatsDto Revenue { get; set; } = new();
    }

    public class MonthlyRevenueStatsDto
    {
        public decimal TutorPayoutAmount { get; set; }
        public decimal RefundedAmount { get; set; }
        public decimal NetPlatformRevenueAmount { get; set; }
    }
}
