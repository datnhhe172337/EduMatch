using EduMatch.DataAccessLayer.Enum;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.Report
{
    public class BasicEvidenceRequest
    {
        [Required]
        public MediaType MediaType { get; set; }

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
