using System;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Entities;

public partial class UserBankAccount
{
    public int Id { get; set; }

    public string UserEmail { get; set; } = null!;

    public int BankId { get; set; }

    public string AccountNumber { get; set; } = null!;

    public string AccountHolderName { get; set; } = null!;

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Bank Bank { get; set; } = null!;

    public virtual User UserEmailNavigation { get; set; } = null!;

    public virtual ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
}
