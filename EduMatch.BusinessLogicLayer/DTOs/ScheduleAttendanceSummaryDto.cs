namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class ScheduleAttendanceSummaryDto
    {
        public int Studied { get; set; }
        public int NotStudiedYet { get; set; }
        public int Cancelled { get; set; }
    }
}
