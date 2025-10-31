using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class Level
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ClassRequest> ClassRequests { get; set; } = new List<ClassRequest>();

    public virtual ICollection<TutorSubject> TutorSubjects { get; set; } = new List<TutorSubject>();
}
