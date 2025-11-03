using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Wallet;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class WithdrawalService : IWithdrawalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EduMatchContext _context; // For transactions

        public WithdrawalService(IUnitOfWork unitOfWork, EduMatchContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task CreateWithdrawalRequestAsync(CreateWithdrawalRequest request, string userEmail)
        {
            // 1. Get the user's wallet
            var wallet = await _unitOfWork.Wallets.GetWalletByUserEmailAsync(userEmail);
            if (wallet == null)
            {
                throw new Exception("Wallet not found.");
            }

            // 2. Check if they have enough balance
            if (wallet.Balance < request.Amount)
            {
                throw new Exception("Insufficient funds. (Không đủ số dư)");
            }

            // 3. Verify their bank account belongs to them
            var bankAccount = await _unitOfWork.UserBankAccounts.GetByIdAsync(request.UserBankAccountId);
            if (bankAccount == null || bankAccount.UserEmail != userEmail)
            {
                throw new Exception("Invalid bank account.");
            }

            // 4. Start a database transaction
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var balanceBefore = wallet.Balance;
                var lockedBalanceBefore = wallet.LockedBalance;

                // 5. Create the Withdrawal record
                var newWithdrawal = new Withdrawal
                {
                    WalletId = wallet.Id,
                    Amount = request.Amount,
                    Status = WithdrawalStatus.Pending, // 0 = Chờ duyệt
                    UserBankAccountId = request.UserBankAccountId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Withdrawals.AddAsync(newWithdrawal);
                await _unitOfWork.CompleteAsync(); // Save to get the newWithdrawal.Id

                // 6. Lock the funds: Move money from 'Balance' to 'LockedBalance'
                wallet.Balance -= request.Amount;
                wallet.LockedBalance += request.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Wallets.Update(wallet);

                // 7. Create the transaction log
                var newTransaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Amount = request.Amount,
                    TransactionType = WalletTransactionType.Debit,
                    Reason = WalletTransactionReason.Withdrawal,
                    Status = TransactionStatus.Pending, // Pending until admin approves
                    BalanceBefore = balanceBefore,
                    BalanceAfter = wallet.Balance,
                    CreatedAt = DateTime.UtcNow,
                    ReferenceCode = $"RUTTIEN_{newWithdrawal.Id}",
                    WithdrawalId = newWithdrawal.Id
                };
                await _unitOfWork.WalletTransactions.AddAsync(newTransaction);

                // 8. Save all changes
                await _unitOfWork.CompleteAsync();

                // 9. Commit the transaction
                await dbTransaction.CommitAsync();
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync();
                throw; // Let the controller know something went wrong
            }
        }
    }
}