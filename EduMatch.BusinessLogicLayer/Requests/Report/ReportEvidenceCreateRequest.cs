using EduMatch.DataAccessLayer.Enum;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.Report
{
    public class ReportEvidenceCreateRequest
    {
        [Required]
        public MediaType MediaType { get; set; }

        public ReportEvidenceType? EvidenceType { get; set; }

        public int? DefenseId { get; set; }

        [Required]
        [Url]
        [StringLength(500)]
        public string FileUrl { get; set; } = string.Empty;

        [StringLength(255)]
        public string? FilePublicId { get; set; }

        [StringLength(255)]
        public string? Caption { get; set; }
    }
}
