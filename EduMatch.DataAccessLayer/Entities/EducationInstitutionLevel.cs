using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class EducationInstitutionLevel
{
    public int Id { get; set; }

    public int InstitutionId { get; set; }

    public int EducationLevelId { get; set; }

    public virtual EducationLevel EducationLevel { get; set; } = null!;

    public virtual EducationInstitution Institution { get; set; } = null!;
}
