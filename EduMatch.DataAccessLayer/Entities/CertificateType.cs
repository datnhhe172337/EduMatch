using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class CertificateType
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public int Verified { get; set; }

    public string? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public virtual ICollection<CertificateTypeSubject> CertificateTypeSubjects { get; set; } = new List<CertificateTypeSubject>();

    public virtual ICollection<TutorCertificate> TutorCertificates { get; set; } = new List<TutorCertificate>();
}
