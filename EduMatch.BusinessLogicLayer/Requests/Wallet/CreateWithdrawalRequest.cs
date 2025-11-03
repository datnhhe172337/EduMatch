using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests.Wallet
{
    public class CreateWithdrawalRequest
    {
        [Required]
        [Range(50000, 5000000, ErrorMessage = "Amount must be between 50,000 and 20,000,000 VND")]
        public decimal Amount { get; set; }

        [Required]
        public int UserBankAccountId { get; set; } // The ID of the bank account they saved
    }
}
