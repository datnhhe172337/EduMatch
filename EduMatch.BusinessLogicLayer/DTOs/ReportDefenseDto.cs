using System;
using System.Collections.Generic;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class ReportDefenseDto
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public string TutorEmail { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public IReadOnlyList<ReportEvidenceDto>? Evidences { get; set; }
    }
}
