using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class CertificateTypeSubject
{
    public int Id { get; set; }

    public int CertificateTypeId { get; set; }

    public int SubjectId { get; set; }

    public virtual CertificateType CertificateType { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
