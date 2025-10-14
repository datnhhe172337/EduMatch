using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class EducationInstitution
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public byte? InstitutionType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<EducationInstitutionLevel> EducationInstitutionLevels { get; set; } = new List<EducationInstitutionLevel>();

    public virtual ICollection<TutorEducation> TutorEducations { get; set; } = new List<TutorEducation>();
}
