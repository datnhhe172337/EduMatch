// Filename: WalletService.cs
using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
// using EduMatch.BusinessLogicLayer.Responses; // <-- No longer needed
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WalletService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<WalletDto?> GetOrCreateWalletForUserAsync(string userEmail)
        {
            var wallet = await _unitOfWork.Wallets.GetWalletByUserEmailAsync(userEmail);

            if (wallet == null)
            {
                var newWallet = new Wallet
                {
                    UserEmail = userEmail,
                    Balance = 0,
                    LockedBalance = 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Wallets.AddAsync(newWallet);
                await _unitOfWork.CompleteAsync();

                wallet = newWallet;
            }

            return _mapper.Map<WalletDto>(wallet);
        }

        public async Task<IEnumerable<WalletTransactionDto>> GetTransactionHistoryAsync(string userEmail)
        {
            var wallet = await _unitOfWork.Wallets.GetWalletByUserEmailAsync(userEmail);
            if (wallet == null)
            {
                return Enumerable.Empty<WalletTransactionDto>();
            }

            var transactions = await _unitOfWork.WalletTransactions.GetTransactionsByWalletIdAsync(wallet.Id);

            return _mapper.Map<IEnumerable<WalletTransactionDto>>(transactions);
        }
    }
}