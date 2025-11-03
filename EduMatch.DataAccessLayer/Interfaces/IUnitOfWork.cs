using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IWalletRepository Wallets { get; }
        IBankRepository Banks { get; }
        IUserBankAccountRepository UserBankAccounts { get; }
        IDepositRepository Deposits { get; }
        IWalletTransactionRepository WalletTransactions { get; }
        IWithdrawalRepository Withdrawals { get; }

        Task<int> CompleteAsync(); 
    }
}
