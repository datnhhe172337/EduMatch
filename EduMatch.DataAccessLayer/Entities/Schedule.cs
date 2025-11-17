using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class Schedule
{
    public int Id { get; set; }

    public int AvailabilitiId { get; set; }

    public int BookingId { get; set; }

    public int Status { get; set; }

    public string? AttendanceNote { get; set; }

    public bool IsRefunded { get; set; }

    public DateTime? RefundedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual TutorAvailability Availabiliti { get; set; } = null!;

    public virtual Booking Booking { get; set; } = null!;

    public virtual MeetingSession? MeetingSession { get; set; }

    public virtual ICollection<ScheduleChangeRequest> ScheduleChangeRequests { get; set; } = new List<ScheduleChangeRequest>();
}
