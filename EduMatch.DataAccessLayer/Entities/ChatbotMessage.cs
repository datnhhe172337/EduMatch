using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class ChatbotMessage
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public string Role { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ChatSession Session { get; set; } = null!;
}
