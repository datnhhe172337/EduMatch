using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.Wallet;
using EduMatch.DataAccessLayer.Entities;
using PayOS.Models.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IDepositService
    {
        //Task<CreateDepositResponseDto> CreateDepositRequestAsync(WalletDepositRequest request, string userEmail);
        Task<Deposit> CreateDepositRequestAsync(WalletDepositRequest request, string userEmail);
        Task<bool> ProcessVnpayPaymentAsync(int depositId, string transactionId, decimal amountPaid);

        Task<int> CleanupExpiredDepositsAsync();

        Task<bool> CancelDepositRequestAsync(int depositId, string userEmail);
    }
}
