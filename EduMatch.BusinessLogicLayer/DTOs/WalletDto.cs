using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.DTOs
{
    public class WalletDto
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public decimal Balance { get; set; }
        public decimal LockedBalance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
