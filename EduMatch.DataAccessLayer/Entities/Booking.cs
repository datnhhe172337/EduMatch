using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class Booking
{
    public int Id { get; set; }

    public string LearnerEmail { get; set; } = null!;

    public int TutorSubjectId { get; set; }

    public DateTime BookingDate { get; set; }

    public int TotalSessions { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalAmount { get; set; }

    public int PaymentStatus { get; set; }

    public decimal RefundedAmount { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int SystemFeeId { get; set; }

    public decimal SystemFeeAmount { get; set; }

    public decimal TutorReceiveAmount { get; set; }

    public virtual ICollection<BookingNote> BookingNotes { get; set; } = new List<BookingNote>();

    public virtual ICollection<BookingRefundRequest> BookingRefundRequests { get; set; } = new List<BookingRefundRequest>();

    public virtual User LearnerEmailNavigation { get; set; } = null!;

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual ICollection<ScheduleCompletion> ScheduleCompletions { get; set; } = new List<ScheduleCompletion>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual SystemFee SystemFee { get; set; } = null!;

    public virtual ICollection<TutorPayout> TutorPayouts { get; set; } = new List<TutorPayout>();

    public virtual TutorSubject TutorSubject { get; set; } = null!;

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}
