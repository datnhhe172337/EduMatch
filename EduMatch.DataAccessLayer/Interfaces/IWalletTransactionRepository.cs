using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IWalletTransactionRepository
    {
        Task AddAsync(WalletTransaction entity);

        Task<IEnumerable<WalletTransaction>> GetTransactionsByWalletIdAsync(int walletId);

        Task<WalletTransaction?> GetPendingWithdrawalTransactionAsync(int withdrawalId);
        void Update(WalletTransaction entity);
    }
}
