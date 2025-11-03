using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Wallet;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using EduMatch.DataAccessLayer.Entities; // Make sure this is here
using EduMatch.DataAccessLayer.Enum; // Make sure this is here

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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

        [HttpPost("admin/cleanup-expired")]
        [Authorize(Roles = "Admin")] 
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