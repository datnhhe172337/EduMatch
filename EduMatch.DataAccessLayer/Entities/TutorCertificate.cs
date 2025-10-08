using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TutorCertificate
{
    public int Id { get; set; }

    public int TutorId { get; set; }

    public string? Title { get; set; }

    public string? Issuer { get; set; }

    public DateOnly? IssueDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string? CertificateUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool Verified { get; set; }

    public virtual TutorProfile Tutor { get; set; } = null!;
}
