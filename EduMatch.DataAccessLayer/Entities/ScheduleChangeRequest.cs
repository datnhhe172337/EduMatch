using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class ScheduleChangeRequest
{
    public int Id { get; set; }

    public int ScheduleId { get; set; }

    public string RequesterEmail { get; set; } = null!;

    public string RequestedToEmail { get; set; } = null!;

    public int OldAvailabilitiId { get; set; }

    public int NewAvailabilitiId { get; set; }

    public string? Reason { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public virtual TutorAvailability NewAvailabiliti { get; set; } = null!;

    public virtual TutorAvailability OldAvailabiliti { get; set; } = null!;

    public virtual User RequestedToEmailNavigation { get; set; } = null!;

    public virtual User RequesterEmailNavigation { get; set; } = null!;

    public virtual Schedule Schedule { get; set; } = null!;
}
