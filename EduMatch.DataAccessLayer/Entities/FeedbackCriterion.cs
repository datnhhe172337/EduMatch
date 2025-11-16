using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class FeedbackCriterion
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<TutorFeedbackDetail> TutorFeedbackDetails { get; set; } = new List<TutorFeedbackDetail>();
}
