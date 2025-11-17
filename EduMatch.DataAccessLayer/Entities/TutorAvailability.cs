using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TutorAvailability
{
    public int Id { get; set; }

    public int TutorId { get; set; }

    public int SlotId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ScheduleChangeRequest> ScheduleChangeRequestNewAvailabilitis { get; set; } = new List<ScheduleChangeRequest>();

    public virtual ICollection<ScheduleChangeRequest> ScheduleChangeRequestOldAvailabilitis { get; set; } = new List<ScheduleChangeRequest>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual TimeSlot Slot { get; set; } = null!;

    public virtual TutorProfile Tutor { get; set; } = null!;
}
