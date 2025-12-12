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
        [Range(50000, double.MaxValue, ErrorMessage = "Amount must be at least 50,000 VND")]
        public decimal Amount { get; set; }

        [Required]
        public int UserBankAccountId { get; set; }
    }
}
