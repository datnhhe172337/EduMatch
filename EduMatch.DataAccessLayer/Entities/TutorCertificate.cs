
using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TutorCertificate
{
    public int Id { get; set; }

    public int TutorId { get; set; }
	public CertificateType CertificateType { get; set; }

	public string? Title { get; set; }

    public string? Issuer { get; set; }

    public DateTime? IssueDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? CertificateUrl { get; set; }
	public string? CertificatePublicId { get; set; }

	public DateTime CreatedAt { get; set; }

    public VerifyStatus Verified { get; set; }

    public virtual TutorProfile Tutor { get; set; } = null!;
}
