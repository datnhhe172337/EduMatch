using EduMatch.BusinessLogicLayer.Requests.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IWithdrawalService
    {
        Task CreateWithdrawalRequestAsync(CreateWithdrawalRequest request, string userEmail);
    }
}
