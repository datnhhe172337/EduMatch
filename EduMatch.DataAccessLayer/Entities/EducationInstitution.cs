using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class EducationInstitution
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int? InstitutionType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int Verified { get; set; }

    public string? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public virtual ICollection<TutorEducation> TutorEducations { get; set; } = new List<TutorEducation>();
}
