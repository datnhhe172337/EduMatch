using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class AdminWithdrawalDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public WithdrawalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public UserBankAccountDto UserBankAccount { get; set; } // The account to send money to
        public WalletDto Wallet { get; set; } // The user's wallet info (like their email)
    }
}
