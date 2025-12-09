using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class BookingNote
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? CreatedByEmail { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual ICollection<BookingNoteMedium> BookingNoteMedia { get; set; } = new List<BookingNoteMedium>();
}
