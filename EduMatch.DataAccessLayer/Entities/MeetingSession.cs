using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class MeetingSession
{
    public int Id { get; set; }

    public int ScheduleId { get; set; }

    public string OrganizerEmail { get; set; } = null!;

    public string MeetLink { get; set; } = null!;

    public string? MeetCode { get; set; }

    public string? EventId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int MeetingType { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual GoogleToken OrganizerEmailNavigation { get; set; } = null!;

    public virtual Schedule Schedule { get; set; } = null!;
}
