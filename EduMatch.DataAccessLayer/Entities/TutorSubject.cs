using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TutorSubject
{
    public int Id { get; set; }

    public int TutorId { get; set; }

    public int SubjectId { get; set; }

    public decimal? HourlyRate { get; set; }

    public int? LevelId { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Level? Level { get; set; }

    public virtual Subject Subject { get; set; } = null!;

    public virtual TutorProfile Tutor { get; set; } = null!;
}
