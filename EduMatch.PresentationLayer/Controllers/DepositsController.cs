using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Wallet;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    /// <summary>
    /// Manages deposit requests and VNPay interactions for authenticated users.
    /// </summary>
    public class DepositsController : ControllerBase
    {
        private readonly IDepositService _depositService;
        private readonly IVnpayService _vnpayService; 
        private readonly CurrentUserService _currentUserService;

        public DepositsController(
            IDepositService depositService,
            IVnpayService vnpayService, 
            CurrentUserService currentUserService)
        {
            _depositService = depositService;
            _vnpayService = vnpayService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Creates a VNPay payment request for the current user.
        /// </summary>
        [HttpPost("create-vnpay-request")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateVnpayDepositRequest([FromBody] WalletDepositRequest request)
        {
            try
            {
                string? userEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

                var newDeposit = await _depositService.CreateDepositRequestAsync(request, userEmail);

                string paymentUrl = _vnpayService.CreatePaymentUrl(
                    newDeposit.Id.ToString(), 
                    newDeposit.Amount,
                    $"Nap tien {newDeposit.Id}",
                    HttpContext // Pass HttpContext to get the IP address
                );

                return StatusCode(StatusCodes.Status201Created, ApiResponse<string>.Ok(paymentUrl));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail($"Failed to create VNPay request: {ex.Message}"));
            }
        }

        /// <summary>
        /// Allows administrators to mark pending deposits older than 24h as failed.
        /// </summary>
        [HttpPost("admin/cleanup-expired")]
        [Authorize(Roles = "3")] 
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CleanupExpiredDeposits()
        {
            try
            {
                int count = await _depositService.CleanupExpiredDepositsAsync();
                return Ok(ApiResponse<string>.Ok($"Successfully cleaned up {count} expired deposits."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail($"Failed to clean up deposits: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cancels a pending deposit belonging to the current user.
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelDepositRequest(int id)
        {
            try
            {
                string? userEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

                bool success = await _depositService.CancelDepositRequestAsync(id, userEmail);

                if (!success)
                {
                    return BadRequest(ApiResponse<string>.Fail("Failed to cancel deposit request."));
                }

                return Ok(ApiResponse<string>.Ok("Deposit request cancelled successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }
    }
}
