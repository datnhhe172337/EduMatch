// Filename: WalletsController.cs
using EduMatch.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduMatch.PresentationLayer.Common; // For ApiResponse
using EduMatch.BusinessLogicLayer.Services;   // For CurrentUserService
using EduMatch.BusinessLogicLayer.DTOs;      // For WalletDto
using Microsoft.AspNetCore.Http;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
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

        // GET: api/Wallets/my-wallet
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
    }
}