using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class LearnerTrialLesson
{
    public int Id { get; set; }

    public string LearnerEmail { get; set; } = null!;

    public int TutorId { get; set; }

    public int SubjectId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User LearnerEmailNavigation { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;

    public virtual TutorProfile Tutor { get; set; } = null!;
}
