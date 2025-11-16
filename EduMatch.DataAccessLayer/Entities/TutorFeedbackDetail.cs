using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TutorFeedbackDetail
{
    public int Id { get; set; }

    public int FeedbackId { get; set; }

    public int CriterionId { get; set; }

    public int Rating { get; set; }

    public virtual FeedbackCriterion Criterion { get; set; } = null!;

    public virtual TutorFeedback Feedback { get; set; } = null!;
}
