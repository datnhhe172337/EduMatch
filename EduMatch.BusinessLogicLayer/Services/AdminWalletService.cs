using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class AdminWalletService : IAdminWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private const string SYSTEM_WALLET_EMAIL = "system@edumatch.com";
        private const string TUTOR_ROLE_NAME = "Tutor";

        public AdminWalletService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SystemWalletDashboardDto> GetSystemWalletDashboardAsync()
        {
            var systemWallet = await EnsureSystemWalletAsync();

            var totalTutorLocked = await _unitOfWork.Wallets.GetTotalLockedBalanceForRoleAsync(TUTOR_ROLE_NAME);

            var totalUserAvailable = await _unitOfWork.Wallets.GetTotalAvailableBalanceAsync();

            return new SystemWalletDashboardDto
            {
                PendingTutorPayoutBalance = systemWallet.LockedBalance,
                PlatformRevenueBalance = systemWallet.Balance,
                TotalUserAvailableBalance = totalUserAvailable
            };
        }

        private async Task<Wallet> EnsureSystemWalletAsync()
        {
            var systemWallet = await _unitOfWork.Wallets.GetWalletByUserEmailAsync(SYSTEM_WALLET_EMAIL);
            if (systemWallet != null)
            {
                return systemWallet;
            }

            var newWallet = new Wallet
            {
                UserEmail = SYSTEM_WALLET_EMAIL,
                Balance = 0,
                LockedBalance = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Wallets.AddAsync(newWallet);
            await _unitOfWork.CompleteAsync();
            return newWallet;
        }
    }
}
