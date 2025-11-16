using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Wallet;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class WithdrawalService : IWithdrawalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EduMatchContext _context;
        private readonly IMapper _mapper;

        public WithdrawalService(IUnitOfWork unitOfWork, EduMatchContext context, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        public async Task CreateWithdrawalRequestAsync(CreateWithdrawalRequest request, string userEmail)
        {
            var wallet = await _unitOfWork.Wallets.GetWalletByUserEmailAsync(userEmail);
            if (wallet == null)
            {
                throw new Exception("Wallet not found.");
            }

            if (wallet.Balance < request.Amount)
            {
                throw new Exception("Insufficient funds. (Không đủ số dư)");
            }

            var bankAccount = await _unitOfWork.UserBankAccounts.GetByIdAsync(request.UserBankAccountId);
            if (bankAccount == null || bankAccount.UserEmail != userEmail)
            {
                throw new Exception("Invalid bank account.");
            }

            using var dbTransaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                wallet = await _unitOfWork.Wallets.GetWalletByUserEmailAsync(userEmail)
                    ?? throw new Exception("Wallet not found.");

                await _context.Entry(wallet).ReloadAsync();

                if (wallet.Balance < request.Amount)
                {
                    throw new Exception("Insufficient funds. (Kh�ng d? s? du)");
                }

                var balanceBefore = wallet.Balance;

                var newWithdrawal = new Withdrawal
                {
                    WalletId = wallet.Id,
                    Amount = request.Amount,
                    Status = WithdrawalStatus.Pending, 
                    UserBankAccountId = request.UserBankAccountId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Withdrawals.AddAsync(newWithdrawal);
                await _unitOfWork.CompleteAsync(); 

                wallet.Balance -= request.Amount;
                wallet.LockedBalance += request.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Wallets.Update(wallet);

                var newTransaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Amount = request.Amount,
                    TransactionType = WalletTransactionType.Debit,
                    Reason = WalletTransactionReason.Withdrawal,
                    Status = TransactionStatus.Pending,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = wallet.Balance,
                    CreatedAt = DateTime.UtcNow,
                    ReferenceCode = $"RUTTIEN_{newWithdrawal.Id}",
                    WithdrawalId = newWithdrawal.Id
                };
                await _unitOfWork.WalletTransactions.AddAsync(newTransaction);

                await _unitOfWork.CompleteAsync();
                await dbTransaction.CommitAsync();
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<WithdrawalDto>> GetWithdrawalHistoryAsync(string userEmail)
        {
            var withdrawals = await _unitOfWork.Withdrawals.GetWithdrawalsByUserEmailAsync(userEmail);

            return _mapper.Map<IEnumerable<WithdrawalDto>>(withdrawals);
        }




        public async Task<IEnumerable<AdminWithdrawalDto>> GetPendingWithdrawalsAsync()
        {
            var withdrawals = await _unitOfWork.Withdrawals.GetPendingWithdrawalsAsync();
            return _mapper.Map<IEnumerable<AdminWithdrawalDto>>(withdrawals);
        }

        public async Task ApproveWithdrawalAsync(int withdrawalId, string adminEmail)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var withdrawal = await _unitOfWork.Withdrawals.GetWithdrawalByIdAsync(withdrawalId);
                if (withdrawal == null || withdrawal.Status != WithdrawalStatus.Pending)
                {
                    throw new Exception("Withdrawal request not found or is not pending.");
                }

                //get the locked transaction
                var tx = await _unitOfWork.WalletTransactions.GetPendingWithdrawalTransactionAsync(withdrawalId);
                if (tx == null)
                {
                    throw new Exception("Associated transaction log not found.");
                }

                //update statuses
                withdrawal.Status = WithdrawalStatus.Completed; 
                withdrawal.ProcessedAt = DateTime.UtcNow;
                withdrawal.AdminEmail = adminEmail;
                _unitOfWork.Withdrawals.Update(withdrawal);

                tx.Status = TransactionStatus.Completed;
                _unitOfWork.WalletTransactions.Update(tx);

                //update the user's wallet
                var wallet = withdrawal.Wallet;
                wallet.LockedBalance -= withdrawal.Amount; 
                wallet.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Wallets.Update(wallet);

                await _unitOfWork.CompleteAsync();
                await dbTransaction.CommitAsync();
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task RejectWithdrawalAsync(int withdrawalId, string adminEmail, string reason)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var withdrawal = await _unitOfWork.Withdrawals.GetWithdrawalByIdAsync(withdrawalId);
                if (withdrawal == null || withdrawal.Status != WithdrawalStatus.Pending)
                {
                    throw new Exception("Withdrawal request not found or is not pending.");
                }

                var tx = await _unitOfWork.WalletTransactions.GetPendingWithdrawalTransactionAsync(withdrawalId);
                if (tx == null)
                {
                    throw new Exception("Associated transaction log not found.");
                }

                withdrawal.Status = WithdrawalStatus.Rejected;
                withdrawal.ProcessedAt = DateTime.UtcNow;
                withdrawal.AdminEmail = adminEmail;
                withdrawal.RejectReason = reason;
                _unitOfWork.Withdrawals.Update(withdrawal);

                tx.Status = TransactionStatus.Failed; //mark the log as failed
                _unitOfWork.WalletTransactions.Update(tx);

                // return the money to the user's main balance
                var wallet = withdrawal.Wallet;
                wallet.LockedBalance -= withdrawal.Amount; 
                wallet.Balance += withdrawal.Amount; 
                wallet.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Wallets.Update(wallet);

                await _unitOfWork.CompleteAsync();
                await dbTransaction.CommitAsync();
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }
    }
}
