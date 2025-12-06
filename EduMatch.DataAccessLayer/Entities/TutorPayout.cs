using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class TutorPayout
{
    public int Id { get; set; }

    public int ScheduleId { get; set; }

    public int BookingId { get; set; }

    public int TutorWalletId { get; set; }

    public decimal Amount { get; set; }

    public decimal SystemFeeAmount { get; set; }

    public byte Status { get; set; }

    public byte PayoutTrigger { get; set; }

    public DateOnly ScheduledPayoutDate { get; set; }

    public DateTime? ReleasedAt { get; set; }

    public int? WalletTransactionId { get; set; }

    public string? HoldReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Schedule Schedule { get; set; } = null!;

    public virtual Wallet TutorWallet { get; set; } = null!;

    public virtual WalletTransaction? WalletTransaction { get; set; }
}
