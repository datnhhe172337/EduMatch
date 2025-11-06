using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class WithdrawalDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public WithdrawalStatus Status { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? RejectReason { get; set; }
        public UserBankAccountDto UserBankAccount { get; set; }
    }
}
