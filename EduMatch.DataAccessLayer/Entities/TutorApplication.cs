using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TutorApplication
{
    public int Id { get; set; }

    public int ClassRequestId { get; set; }

    public int TutorId { get; set; }

    public string Message { get; set; } = null!;

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ClassRequest ClassRequest { get; set; } = null!;

    public virtual TutorProfile Tutor { get; set; } = null!;
}
