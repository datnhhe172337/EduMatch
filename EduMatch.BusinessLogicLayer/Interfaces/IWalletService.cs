using Azure;
using EduMatch.BusinessLogicLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IWalletService
    {
        Task<WalletDto?> GetOrCreateWalletForUserAsync(string userEmail);
        Task<IEnumerable<WalletTransactionDto>> GetTransactionHistoryAsync(string userEmail);
    }
}
