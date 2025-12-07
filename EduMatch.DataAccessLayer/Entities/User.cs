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

    public virtual ICollection<BookingRefundRequest> BookingRefundRequests { get; set; } = new List<BookingRefundRequest>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();

    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();

    public virtual ICollection<ClassRequest> ClassRequests { get; set; } = new List<ClassRequest>();

    public virtual ICollection<FavoriteTutor> FavoriteTutors { get; set; } = new List<FavoriteTutor>();

    public virtual ICollection<LearnerTrialLesson> LearnerTrialLessons { get; set; } = new List<LearnerTrialLesson>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<Report> ReportHandledByAdminEmailNavigations { get; set; } = new List<Report>();

    public virtual ICollection<Report> ReportReportedUserEmailNavigations { get; set; } = new List<Report>();

    public virtual ICollection<Report> ReportReporterUserEmailNavigations { get; set; } = new List<Report>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<ScheduleChangeRequest> ScheduleChangeRequestRequestedToEmailNavigations { get; set; } = new List<ScheduleChangeRequest>();

    public virtual ICollection<ScheduleChangeRequest> ScheduleChangeRequestRequesterEmailNavigations { get; set; } = new List<ScheduleChangeRequest>();

    public virtual ICollection<ScheduleCompletion> ScheduleCompletions { get; set; } = new List<ScheduleCompletion>();

    public virtual TutorProfile? TutorProfile { get; set; }

    public virtual ICollection<UserBankAccount> UserBankAccounts { get; set; } = new List<UserBankAccount>();

    public virtual UserProfile? UserProfile { get; set; }

    public virtual Wallet? Wallet { get; set; }
}
