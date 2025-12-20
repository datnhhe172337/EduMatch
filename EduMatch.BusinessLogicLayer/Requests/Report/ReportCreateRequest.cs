using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.Report
{
    public class ReportCreateRequest
    {
        [Required]
        [EmailAddress]
        public string ReportedUserEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public string Reason { get; set; } = string.Empty;

        public int? BookingId { get; set; }

        public int? ScheduleId { get; set; }

        public List<BasicEvidenceRequest>? Evidences { get; set; }
    }
}
