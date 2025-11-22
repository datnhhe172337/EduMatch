using System.Collections.Generic;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class ReportFullDetailDto : ReportDetailDto
    {
        public IReadOnlyList<ReportDefenseDto>? Defenses { get; set; }
        public IReadOnlyList<ReportEvidenceDto>? ReporterEvidences { get; set; }
        public IReadOnlyList<ReportEvidenceDto>? TutorEvidences { get; set; }
        public IReadOnlyList<ReportEvidenceDto>? AdminEvidences { get; set; }
    }
}
