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

    public int? BookingId { get; set; }

    public int? ScheduleId { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual User? HandledByAdminEmailNavigation { get; set; }

    public virtual ICollection<ReportDefense> ReportDefenses { get; set; } = new List<ReportDefense>();

    public virtual ICollection<ReportEvidence> ReportEvidences { get; set; } = new List<ReportEvidence>();

    public virtual User ReportedUserEmailNavigation { get; set; } = null!;

    public virtual User ReporterUserEmailNavigation { get; set; } = null!;

    public virtual Schedule? Schedule { get; set; }

    public virtual ICollection<ScheduleCompletion> ScheduleCompletions { get; set; } = new List<ScheduleCompletion>();
}
