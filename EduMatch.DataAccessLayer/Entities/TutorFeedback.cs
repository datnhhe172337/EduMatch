using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TutorFeedback
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public string LearnerEmail { get; set; } = null!;

    public int TutorId { get; set; }

    public double OverallRating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<TutorFeedbackDetail> TutorFeedbackDetails { get; set; } = new List<TutorFeedbackDetail>();
}
