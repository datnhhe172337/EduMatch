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
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class DepositService : IDepositService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly EduMatchContext _context; 
        private readonly INotificationService _notificationService;
        private readonly EmailService _emailService;

        public DepositService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            EduMatchContext context,
            INotificationService notificationService,
            EmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<Deposit> CreateDepositRequestAsync(WalletDepositRequest request, string userEmail)
        {
            if (request.Amount < 50_000)
            {
                throw new Exception("Số tiền tối thiểu là 50,000 VND.");
            }

            var wallet = await _unitOfWork.Wallets.GetWalletByUserEmailAsync(userEmail);
            if (wallet == null)
            {
                wallet = new Wallet { UserEmail = userEmail };
                await _unitOfWork.Wallets.AddAsync(wallet);
                await _unitOfWork.CompleteAsync();
            }

            var newDeposit = new Deposit
            {
                WalletId = wallet.Id,
                Amount = request.Amount,
                Status = TransactionStatus.Pending,
                PaymentGateway = "VNPay", // Set the gateway name
                CreatedAt = System.DateTime.UtcNow
            };
            await _unitOfWork.Deposits.AddAsync(newDeposit);
            await _unitOfWork.CompleteAsync();

            return newDeposit;
        }

        //THIS IS THE WEBHOOK METHOD FOR VNPAY
        public async Task<bool> ProcessVnpayPaymentAsync(int depositId, string transactionId, decimal amountPaid)
        {
            bool amountMismatch = false;
            string? mismatchMessage = null;
            bool depositCompleted = false;
            string? notifiedUserEmail = null;
            decimal creditedAmount = 0;

            using var dbTransaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var deposit = await _unitOfWork.Deposits.GetByIdAsync(depositId);
                if (deposit == null) throw new Exception($"Deposit {depositId} not found.");

                if (deposit.Status == TransactionStatus.Completed)
                {
                    await dbTransaction.RollbackAsync();
                    return true;
                }

                if (deposit.Status != TransactionStatus.Pending)
                {
                    await dbTransaction.RollbackAsync();
                    return false;
                }

                if (deposit.Amount != amountPaid)
                {
                    deposit.Status = TransactionStatus.Failed;
                    deposit.CompletedAt = DateTime.UtcNow;
                    _unitOfWork.Deposits.Update(deposit);
                    amountMismatch = true;
                    mismatchMessage = $"Amount mismatch for deposit {depositId}. Expected: {deposit.Amount}, Got: {amountPaid}";
                }
                else
                {
                    var wallet = deposit.Wallet;
                    if (wallet == null) throw new Exception($"Wallet {deposit.WalletId} not found.");

                    var balanceBefore = wallet.Balance;
                    var balanceAfter = balanceBefore + deposit.Amount;

                    deposit.Status = TransactionStatus.Completed;
                    deposit.GatewayTransactionCode = transactionId;
                    deposit.CompletedAt = DateTime.UtcNow;
                    _unitOfWork.Deposits.Update(deposit);

                    wallet.Balance = balanceAfter;
                    wallet.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Wallets.Update(wallet);

                    var newTransaction = new WalletTransaction
                    {
                        WalletId = wallet.Id,
                        Amount = deposit.Amount,
                        TransactionType = WalletTransactionType.Credit,
                        Reason = WalletTransactionReason.Deposit,
                        Status = TransactionStatus.Completed,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        CreatedAt = DateTime.UtcNow,
                        ReferenceCode = $"VNPAY_{transactionId}",
                        DepositId = deposit.Id
                    };
                    await _unitOfWork.WalletTransactions.AddAsync(newTransaction);

                    depositCompleted = true;
                    notifiedUserEmail = wallet.UserEmail;
                    creditedAmount = deposit.Amount;
                }

                await _unitOfWork.CompleteAsync();
                await dbTransaction.CommitAsync();
            }
            catch (Exception)
            {
                await dbTransaction.RollbackAsync();
                throw;
            }

            if (depositCompleted && notifiedUserEmail != null)
            {
                await _notificationService.CreateNotificationAsync(
                    notifiedUserEmail,
                    $"Nạp thành công {creditedAmount:N0} VND vào ví.",
                    "/wallet/my-wallet");

                await _emailService.SendMailAsync(new MailContent
                {
                    To = notifiedUserEmail,
                    Subject = "Nạp tiền thành công",
                    Body = $"Bạn đã nạp thành công {creditedAmount:N0} VND vào ví của bạn ở hệ thống EduMatch. Mã giao dịch: VNPAY_{transactionId}"
                });
            }
            if (amountMismatch && mismatchMessage != null)
            {
                throw new Exception(mismatchMessage);
            }

            return !amountMismatch;
        }


        public async Task<int> CleanupExpiredDepositsAsync()
        {
            // Find all deposits that are still "Pending"
            // AND were created more than 24 hours ago
            var expirationTime = DateTime.UtcNow.AddHours(-24);

            var expiredDeposits = await _unitOfWork.Deposits
                .FindAsync(d => d.Status == TransactionStatus.Pending && d.CreatedAt < expirationTime);

            if (expiredDeposits == null || !expiredDeposits.Any())
            {
                return 0; // Nothing to clean up
            }

            foreach (var deposit in expiredDeposits)
            {
                // Change status to 2 (Failed/Expired/Cancelled)
                deposit.Status = TransactionStatus.Failed;
                _unitOfWork.Deposits.Update(deposit);
            }

            // Save all changes to the database
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> CancelDepositRequestAsync(int depositId, string userEmail)
        {
            var deposit = await _unitOfWork.Deposits.GetByIdAsync(depositId);

            if (deposit == null)
            {
                throw new Exception("Deposit request not found.");
            }
            if (deposit.Wallet.UserEmail != userEmail)
            {
                throw new Exception("You do not have permission to cancel this request.");
            }
            if (deposit.Status != TransactionStatus.Pending)
            {
                throw new Exception("This request cannot be cancelled as it is no longer pending.");
            }

            deposit.Status = TransactionStatus.Failed;
            _unitOfWork.Deposits.Update(deposit);

            int result = await _unitOfWork.CompleteAsync();
            return result > 0;
        }
    }
}




