using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var systemWallet = await _unitOfWork.Wallets.GetWalletByUserEmailAsync(SYSTEM_WALLET_EMAIL);

            var totalTutorLocked = await _unitOfWork.Wallets.GetTotalLockedBalanceForRoleAsync(TUTOR_ROLE_NAME);

            var totalUserAvailable = await _unitOfWork.Wallets.GetTotalAvailableBalanceAsync();

            return new SystemWalletDashboardDto
            {
                PlatformRevenueBalance = systemWallet.Balance,
                TotalTutorLockedBalance = totalTutorLocked,
                TotalUserAvailableBalance = totalUserAvailable
            };
        }
    }
}
