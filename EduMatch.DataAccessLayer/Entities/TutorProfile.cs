using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TutorProfile
{
    public int Id { get; set; }

    public string UserEmail { get; set; } = null!;

    public string? Bio { get; set; }

    public string? TeachingExp { get; set; }

    public string? VideoIntroUrl { get; set; }

    public string? VideoIntroPublicId { get; set; }

    public int TeachingModes { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public DateTime? LastSync { get; set; }

    public virtual ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();

    public virtual ICollection<FavoriteTutor> FavoriteTutors { get; set; } = new List<FavoriteTutor>();

    public virtual ICollection<LearnerTrialLesson> LearnerTrialLessons { get; set; } = new List<LearnerTrialLesson>();

    public virtual ICollection<ScheduleCompletion> ScheduleCompletions { get; set; } = new List<ScheduleCompletion>();

    public virtual ICollection<TutorApplication> TutorApplications { get; set; } = new List<TutorApplication>();

    public virtual ICollection<TutorAvailability> TutorAvailabilities { get; set; } = new List<TutorAvailability>();

    public virtual ICollection<TutorCertificate> TutorCertificates { get; set; } = new List<TutorCertificate>();

    public virtual ICollection<TutorEducation> TutorEducations { get; set; } = new List<TutorEducation>();

    public virtual ICollection<TutorRatingSummary> TutorRatingSummaries { get; set; } = new List<TutorRatingSummary>();

    public virtual ICollection<TutorSubject> TutorSubjects { get; set; } = new List<TutorSubject>();

    public virtual ICollection<TutorVerificationRequest> TutorVerificationRequests { get; set; } = new List<TutorVerificationRequest>();

    public virtual User UserEmailNavigation { get; set; } = null!;
}
