using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class WalletTransaction
{
    public int Id { get; set; }

    public int WalletId { get; set; }

    public decimal Amount { get; set; }

    public int TransactionType { get; set; }

    public int Reason { get; set; }

    public int Status { get; set; }

    public decimal BalanceBefore { get; set; }

    public decimal BalanceAfter { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? ReferenceCode { get; set; }

    public int? BookingId { get; set; }

    public int? DepositId { get; set; }

    public int? WithdrawalId { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Deposit? Deposit { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;

    public virtual Withdrawal? Withdrawal { get; set; }
}
