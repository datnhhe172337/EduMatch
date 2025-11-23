using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TutorRatingSummary
{
    public int Id { get; set; }

    public int TutorId { get; set; }

    public double AverageRating { get; set; }

    public int TotalFeedbackCount { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual TutorProfile Tutor { get; set; } = null!;
}
