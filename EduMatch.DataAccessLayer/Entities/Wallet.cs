using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class Wallet
{
    public int Id { get; set; }

    public string UserEmail { get; set; } = null!;

    public decimal Balance { get; set; }

    public decimal LockedBalance { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();

    public virtual ICollection<TutorPayout> TutorPayouts { get; set; } = new List<TutorPayout>();

    public virtual User UserEmailNavigation { get; set; } = null!;

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();

    public virtual ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
}
