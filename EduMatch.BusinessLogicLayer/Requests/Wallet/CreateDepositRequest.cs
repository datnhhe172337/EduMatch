using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests.Wallet
{
    public class CreateDepositRequest
    {
        [Required]
        [Range(10000, 10000000, ErrorMessage = "Amount must be between 10,000 and 10,000,000 VND")]
        public int Amount { get; set; }
    }
}
