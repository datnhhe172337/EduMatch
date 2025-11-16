using EduMatch.BusinessLogicLayer.Constants;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/admin/wallet")]
    [ApiController]
    [Authorize(Roles = Roles.BusinessAdmin + "," + Roles.SystemAdmin)]
    /// <summary>
    /// Enables administrators to inspect system wallet balances and history.
    /// </summary>
    public class AdminWalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private const string SYSTEM_WALLET_EMAIL = "system@edumatch.com";
        private readonly IAdminWalletService _adminWalletService;
        public AdminWalletController(IWalletService walletService, IAdminWalletService adminWalletService)
        {
            _walletService = walletService;
            _adminWalletService = adminWalletService;
        }

        // GET: api/admin/wallet/system
        /// <summary>
        /// Retrieves the platform's system wallet, creating it if needed.
        /// </summary>
        [HttpGet("system")]
        [ProducesResponseType(typeof(ApiResponse<WalletDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSystemWallet()
        {
            var wallet = await _walletService.GetOrCreateWalletForUserAsync(SYSTEM_WALLET_EMAIL);
            return Ok(ApiResponse<WalletDto>.Ok(wallet));
        }

        // GET: api/admin/wallet/system-transactions
        /// <summary>
        /// Retrieves the transaction history for the system wallet.
        /// </summary>
        [HttpGet("system-transactions")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<WalletTransactionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSystemWalletTransactions()
        {
            var history = await _walletService.GetTransactionHistoryAsync(SYSTEM_WALLET_EMAIL);
            return Ok(ApiResponse<IEnumerable<WalletTransactionDto>>.Ok(history));
        }

        /// <summary>
        /// Provides aggregated balances for the system wallet dashboard.
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ApiResponse<SystemWalletDashboardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSystemWalletDashboard()
        {
            try
            {
                var dashboardData = await _adminWalletService.GetSystemWalletDashboardAsync();
                return Ok(ApiResponse<SystemWalletDashboardDto>.Ok(dashboardData));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail($"Failed to get dashboard: {ex.Message}"));
            }
        }
    }
}
