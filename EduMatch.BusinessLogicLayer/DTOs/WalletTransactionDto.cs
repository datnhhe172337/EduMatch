using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class WalletTransactionDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public WalletTransactionType TransactionType { get; set; } // 0=Debit, 1=Credit
        public WalletTransactionReason Reason { get; set; } // 0=Deposit, 1=Withdrawal, etc.
        public TransactionStatus Status { get; set; } // 0=Pending, 1=Completed, 2=Failed
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ReferenceCode { get; set; }
        public BookingDto? Booking { get; set; }
    }
}
