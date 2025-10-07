using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TutorAvailability
{
    public int AvailabilityId { get; set; }

    public int TutorId { get; set; }

    public byte DayOfWeek { get; set; }

    public int SlotId { get; set; }

    public bool IsRecurring { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public virtual TimeSlot Slot { get; set; } = null!;

    public virtual TutorProfile Tutor { get; set; } = null!;
}
