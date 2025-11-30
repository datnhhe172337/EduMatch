using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class BookingNote
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public string Content { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public string? VideoUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}
