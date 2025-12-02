using EduMatch.DataAccessLayer.Enum;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.Report
{
    public class ReportEvidenceUpdateRequest
    {
        public MediaType? MediaType { get; set; }

        public ReportEvidenceType? EvidenceType { get; set; }

        [Url]
        [StringLength(500)]
        public string? FileUrl { get; set; }

        [StringLength(255)]
        public string? FilePublicId { get; set; }

        [StringLength(255)]
        public string? Caption { get; set; }
    }
}
