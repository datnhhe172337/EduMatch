using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class ChatSession
{
    public int Id { get; set; }

    public string? UserEmail { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ChatbotMessage> ChatbotMessages { get; set; } = new List<ChatbotMessage>();

    public virtual User? UserEmailNavigation { get; set; }
}
