using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class GoogleToken
{
    public int Id { get; set; }

    public string AccountEmail { get; set; } = null!;

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public string? TokenType { get; set; }

    public string? Scope { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<MeetingSession> MeetingSessions { get; set; } = new List<MeetingSession>();
}
