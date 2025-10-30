using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class User
{
    public string Email { get; set; } = null!;

    public string? UserName { get; set; }

    public string? PasswordHash { get; set; }

    public string? Phone { get; set; }

    public bool? IsEmailConfirmed { get; set; }

    public string LoginProvider { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public int RoleId { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();

    public virtual ICollection<ClassRequest> ClassRequests { get; set; } = new List<ClassRequest>();

    public virtual ICollection<FavoriteTutor> FavoriteTutors { get; set; } = new List<FavoriteTutor>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual Role Role { get; set; } = null!;

    public virtual TutorProfile? TutorProfile { get; set; }

    public virtual UserProfile? UserProfile { get; set; }
}
