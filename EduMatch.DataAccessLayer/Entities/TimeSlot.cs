using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TimeSlot
{
    public int Id { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public virtual ICollection<ClassRequestSlotsAvailability> ClassRequestSlotsAvailabilities { get; set; } = new List<ClassRequestSlotsAvailability>();

    public virtual ICollection<TutorAvailability> TutorAvailabilities { get; set; } = new List<TutorAvailability>();
}
