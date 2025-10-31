using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class ClassRequestSlotsAvailability
{
    public int Id { get; set; }

    public int ClassRequestId { get; set; }

    public int DayOfWeek { get; set; }

    public int SlotId { get; set; }

    public virtual ClassRequest ClassRequest { get; set; } = null!;

    public virtual TimeSlot Slot { get; set; } = null!;
}
