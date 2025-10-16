using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class ChatMessage
{
    public int Id { get; set; }

    public int ChatRoomId { get; set; }

    public string SenderEmail { get; set; } = null!;

    public string ReceiverEmail { get; set; } = null!;

    public string MessageText { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }

    public virtual ChatRoom ChatRoom { get; set; } = null!;
}
