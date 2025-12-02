using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class Subject
{
    public int Id { get; set; }

    public string SubjectName { get; set; } = null!;

    public virtual ICollection<CertificateTypeSubject> CertificateTypeSubjects { get; set; } = new List<CertificateTypeSubject>();

    public virtual ICollection<ClassRequest> ClassRequests { get; set; } = new List<ClassRequest>();

    public virtual ICollection<LearnerTrialLesson> LearnerTrialLessons { get; set; } = new List<LearnerTrialLesson>();

    public virtual ICollection<TutorSubject> TutorSubjects { get; set; } = new List<TutorSubject>();
}
