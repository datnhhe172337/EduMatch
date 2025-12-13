using EduMatch.DataAccessLayer.Enum;
using System;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class ReportListItemDto
    {
        public int Id { get; set; }
        public string ReporterEmail { get; set; } = string.Empty;
        public string? ReporterName { get; set; }
        public string? ReporterAvatarUrl { get; set; }
        public string ReportedUserEmail { get; set; } = string.Empty;
        public string? ReportedUserName { get; set; }
        public string? ReportedAvatarUrl { get; set; }
        public string Reason { get; set; } = string.Empty;
        public ReportStatus Status { get; set; }
        public int? BookingId { get; set; }
        public int? ScheduleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
