using EduMatch.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduMatch.PresentationLayer.Common; 
using EduMatch.BusinessLogicLayer.Services; 
using EduMatch.BusinessLogicLayer.DTOs;     
using Microsoft.AspNetCore.Http;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    /// <summary>
    /// Handles wallet queries for authenticated end users.
    /// </summary>
    public class WalletsController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly CurrentUserService _currentUserService;

        public WalletsController(
            IWalletService walletService,
            CurrentUserService currentUserService)
        {
            _walletService = walletService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Returns the current user's wallet, creating one if it does not exist.
        /// </summary>
        [HttpGet("my-wallet")]
        [ProducesResponseType(typeof(ApiResponse<WalletDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMyWallet()
        {
            try
            {
                string? userEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

                var wallet = await _walletService.GetOrCreateWalletForUserAsync(userEmail);

                if (wallet == null)
                {
                    return BadRequest(ApiResponse<string>.Fail("Could not get or create wallet for user."));
                }

                return Ok(ApiResponse<WalletDto>.Ok(wallet));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail("An error occurred while retrieving the wallet.", ex.Message));
            }
        }


        /// <summary>
        /// Returns the transaction history for the current user's wallet.
        /// </summary>
        [HttpGet("my-transactions")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<WalletTransactionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMyTransactionHistory()
        {
            try
            {
                string? userEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

                var history = await _walletService.GetTransactionHistoryAsync(userEmail);

                return Ok(ApiResponse<IEnumerable<WalletTransactionDto>>.Ok(history));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail("An error occurred while retrieving transaction history.", ex.Message));
            }
        }
    }
}
