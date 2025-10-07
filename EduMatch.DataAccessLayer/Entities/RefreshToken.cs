using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class RefreshToken
{
    public int Id { get; set; }

    public string UserEmail { get; set; } = null!;

    public string TokenHash { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User UserEmailNavigation { get; set; } = null!;
}
