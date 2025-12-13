namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class ReportDetailDto : ReportListItemDto
    {
        public string? TutorDefenseNote { get; set; }
        public string? AdminNotes { get; set; }
        public string? HandledByAdminEmail { get; set; }
        public BookingDto? Booking { get; set; }
        public ScheduleDto? Schedule { get; set; }
    }
}
