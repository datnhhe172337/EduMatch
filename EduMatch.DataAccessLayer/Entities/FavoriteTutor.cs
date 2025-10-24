using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class FavoriteTutor
{
    public int Id { get; set; }

    public string UserEmail { get; set; } = null!;

    public int TutorId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual TutorProfile Tutor { get; set; } = null!;

    public virtual User UserEmailNavigation { get; set; } = null!;
}
