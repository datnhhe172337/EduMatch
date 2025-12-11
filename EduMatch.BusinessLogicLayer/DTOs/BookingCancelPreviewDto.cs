namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class BookingCancelPreviewDto
    {
        public int BookingId { get; set; }
        public int UpcomingSchedules { get; set; }
        public decimal RefundableAmount { get; set; }
    }
}
