using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TutorProfile
{
    public int Id { get; set; }

    public string UserEmail { get; set; } = null!;

    public string? Gender { get; set; }

    public DateOnly? Dob { get; set; }

    public string? Title { get; set; }

    public string? Bio { get; set; }

    public string? TeachingExp { get; set; }

    public string? VideoIntroUrl { get; set; }

    public string? TeachingModes { get; set; }

    public int StatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual TutorStatus Status { get; set; } = null!;

    public virtual ICollection<TutorAvailability> TutorAvailabilities { get; set; } = new List<TutorAvailability>();

    public virtual ICollection<TutorCertificate> TutorCertificates { get; set; } = new List<TutorCertificate>();

    public virtual ICollection<TutorEducation> TutorEducations { get; set; } = new List<TutorEducation>();

    public virtual ICollection<TutorSubject> TutorSubjects { get; set; } = new List<TutorSubject>();

    public virtual User UserEmailNavigation { get; set; } = null!;
}
