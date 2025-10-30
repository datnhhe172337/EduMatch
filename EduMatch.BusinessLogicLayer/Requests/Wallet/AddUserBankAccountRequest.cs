using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Requests.Bank
{
    public class AddUserBankAccountRequest
    {
        [Required]
        public int BankId { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string AccountHolderName { get; set; }
    }
}
