using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Wallet;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class WithdrawalService : IWithdrawalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly EduMatchContext _context;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public WithdrawalService(IUnitOfWork unitOfWork, EduMatchContext context, IMapper mapper, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _notificationService = notificationService;
        }

        public async Task CreateWithdrawalRequestAsync(CreateWithdrawalRequest request, string userEmail)
        {
            if (request.Amount < 50_000)
            {
                throw new Exception("Số tiền tối thiểu là 50,000 VND.");
            }

            var bankAccount = await _unitOfWork.UserBankAccounts.GetByIdAsync(request.UserBankAccountId);
            if (bankAccount == null || bankAccount.UserEmail != userEmail)
            {
                throw new Exception("Invalid bank account.");
            }

            using var dbTransaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var wallet = await _unitOfWork.Wallets.GetWalletByUserEmailAsync(userEmail)
                    ?? throw new Exception("Wallet not found.");

                await _context.Entry(wallet).ReloadAsync();

                if (wallet.Balance < request.Amount)
                {
                    throw new Exception("Không đủ số dư.");
                }

                var balanceBefore = wallet.Balance;

                var newWithdrawal = new Withdrawal
                {
                    WalletId = wallet.Id,
                    Amount = request.Amount,
                    Status = WithdrawalStatus.Completed,
                    UserBankAccountId = request.UserBankAccountId,
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                };
                await _unitOfWork.Withdrawals.AddAsync(newWithdrawal);
                await _unitOfWork.CompleteAsync();

                wallet.Balance -= request.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Wallets.Update(wallet);

                var newTransaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Amount = request.Amount,
                    TransactionType = WalletTransactionType.Debit,
                    Reason = WalletTransactionReason.Withdrawal,
                    Status = TransactionStatus.Completed,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = wallet.Balance,
                    CreatedAt = DateTime.UtcNow,
                    ReferenceCode = $"RUTTIEN_{newWithdrawal.Id}",
                    WithdrawalId = newWithdrawal.Id
                };
                await _unitOfWork.WalletTransactions.AddAsync(newTransaction);

                await _unitOfWork.CompleteAsync();
                await dbTransaction.CommitAsync();

                await _notificationService.CreateNotificationAsync(
                    wallet.UserEmail,
                    $"Yêu cầu rút tiền #{newWithdrawal.Id} số tiền {request.Amount:N0} VND đã được xử lý thành công.",
                    "/wallet/withdrawals");
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
    }
}

