using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TutorVerificationRequest
{
    public int Id { get; set; }

    public string UserEmail { get; set; } = null!;

    public int? TutorId { get; set; }

    public int Status { get; set; }

    public string? Description { get; set; }

    public string? AdminNote { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string? ProcessedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual TutorProfile? Tutor { get; set; }
}
