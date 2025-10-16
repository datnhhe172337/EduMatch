using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TutorAvailability
{
    public int Id { get; set; }

    public int TutorId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public int SlotId { get; set; }

    public bool IsRecurring { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public virtual TimeSlot Slot { get; set; } = null!;

    public virtual TutorProfile Tutor { get; set; } = null!;
}
