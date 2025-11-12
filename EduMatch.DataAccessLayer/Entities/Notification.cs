using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class Notification
{
    public int Id { get; set; }

    public string UserEmail { get; set; } = null!;

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public string? LinkUrl { get; set; }

    public virtual User UserEmailNavigation { get; set; } = null!;
}
