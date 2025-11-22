using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Requests.Report
{
    public class ReportDefenseCreateRequest
    {
        [Required]
        [StringLength(2000, MinimumLength = 3)]
        public string Note { get; set; } = string.Empty;

        public List<BasicEvidenceRequest>? Evidences { get; set; }
    }
}
