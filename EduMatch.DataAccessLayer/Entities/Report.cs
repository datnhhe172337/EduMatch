using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class Report
{
    public int Id { get; set; }

    public string ReporterUserEmail { get; set; } = null!;

    public string ReportedUserEmail { get; set; } = null!;

    public string Reason { get; set; } = null!;

    public string? TutorDefenseNote { get; set; }

    public int Status { get; set; }

    public string? AdminNotes { get; set; }

    public string? HandledByAdminEmail { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? HandledByAdminEmailNavigation { get; set; }

    public virtual User ReportedUserEmailNavigation { get; set; } = null!;

    public virtual User ReporterUserEmailNavigation { get; set; } = null!;
}
