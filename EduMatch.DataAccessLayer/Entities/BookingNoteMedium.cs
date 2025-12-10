using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class BookingNoteMedium
{
    public int Id { get; set; }

    public int BookingNoteId { get; set; }

    public int MediaType { get; set; }

    public string FileUrl { get; set; } = null!;

    public string? FilePublicId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual BookingNote BookingNote { get; set; } = null!;
}
