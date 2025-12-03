using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EduMatchContext _context; 

        public IWalletRepository Wallets { get; private set; }
        public IBankRepository Banks { get; private set; }
        public IUserBankAccountRepository UserBankAccounts { get; private set; }
        public IDepositRepository Deposits { get; private set; }
        public IWalletTransactionRepository WalletTransactions { get; private set; }
        public IWithdrawalRepository Withdrawals { get; private set; }
        public ISystemFeeRepository SystemFees { get; private set; }
        public ITutorPayoutRepository TutorPayouts { get; private set; }
        public IScheduleCompletionRepository ScheduleCompletions { get; private set; }
        public IBookingRepository Bookings { get; private set; }
        public UnitOfWork(EduMatchContext context)
        {
            _context = context;
            Wallets = new WalletRepository(_context);
            Banks = new BankRepository(_context);
            UserBankAccounts = new UserBankAccountRepository(_context);
            Deposits = new DepositRepository(_context);
            WalletTransactions = new WalletTransactionRepository(_context);
            Withdrawals = new WithdrawalRepository(_context);
            SystemFees = new SystemFeeRepository(_context);
            TutorPayouts = new TutorPayoutRepository(_context);
            ScheduleCompletions = new ScheduleCompletionRepository(_context);
            Bookings = new BookingRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
