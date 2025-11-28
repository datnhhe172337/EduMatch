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
        public decimal GrossAmount { get; set; }
        public decimal PlatformFeeAmount { get; set; }
        public decimal TutorPayoutAmount { get; set; }
        public decimal RefundedAmount { get; set; }
    }
}
