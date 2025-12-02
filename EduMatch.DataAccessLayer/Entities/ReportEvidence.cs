using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class ReportEvidence
{
    public int Id { get; set; }

    public int ReportId { get; set; }

    public string? SubmittedByEmail { get; set; }

    public int MediaType { get; set; }

    public string FileUrl { get; set; } = null!;

    public string? FilePublicId { get; set; }

    public string? Caption { get; set; }

    public DateTime CreatedAt { get; set; }

    public int EvidenceType { get; set; }

    public int? DefenseId { get; set; }

    public virtual ReportDefense? Defense { get; set; }

    public virtual Report Report { get; set; } = null!;
}
