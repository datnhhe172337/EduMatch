using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class ScheduleCompletion
{
    public int Id { get; set; }

    public int ScheduleId { get; set; }

    public int BookingId { get; set; }

    public int TutorId { get; set; }

    public string LearnerEmail { get; set; } = null!;

    public byte Status { get; set; }

    public DateTime ConfirmationDeadline { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? AutoCompletedAt { get; set; }

    public int? ReportId { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual User LearnerEmailNavigation { get; set; } = null!;

    public virtual Report? Report { get; set; }

    public virtual Schedule Schedule { get; set; } = null!;

    public virtual TutorProfile Tutor { get; set; } = null!;
}
