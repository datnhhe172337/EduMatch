using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TutorEducation
{
    public int Id { get; set; }

    public int TutorId { get; set; }

    public int InstitutionId { get; set; }

    public DateTime? IssueDate { get; set; }

    public string? CertificateUrl { get; set; }

    public string? CertificatePublicId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int Verified { get; set; }

    public string? RejectReason { get; set; }

    public virtual EducationInstitution Institution { get; set; } = null!;

    public virtual TutorProfile Tutor { get; set; } = null!;
}
