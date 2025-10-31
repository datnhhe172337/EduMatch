using EduMatch.DataAccessLayer.Enum;
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

    public TutorAvailabilityStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual TimeSlot Slot { get; set; } = null!;

    public virtual TutorProfile Tutor { get; set; } = null!;
}
