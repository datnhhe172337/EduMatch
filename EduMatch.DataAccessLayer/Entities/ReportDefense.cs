using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class ReportDefense
{
    public int Id { get; set; }

    public int ReportId { get; set; }

    public string TutorEmail { get; set; } = null!;

    public string Note { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Report Report { get; set; } = null!;

    public virtual ICollection<ReportEvidence> ReportEvidences { get; set; } = new List<ReportEvidence>();
}
