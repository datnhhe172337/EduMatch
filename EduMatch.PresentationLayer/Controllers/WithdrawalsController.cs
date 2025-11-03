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
    public class WithdrawalsController : ControllerBase
    {
        private readonly IWithdrawalService _withdrawalService;
        private readonly CurrentUserService _currentUserService;

        public WithdrawalsController(IWithdrawalService withdrawalService, CurrentUserService currentUserService)
        {
            _withdrawalService = withdrawalService;
            _currentUserService = currentUserService;
        }

        // POST: api/withdrawals/create-request
        [HttpPost("create-request")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateWithdrawalRequest([FromBody] CreateWithdrawalRequest request)
        {
            try
            {
                string? userEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

                await _withdrawalService.CreateWithdrawalRequestAsync(request, userEmail);

                return StatusCode(StatusCodes.Status201Created, ApiResponse<string>.Ok("Withdrawal request submitted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail($"Failed to create withdrawal request: {ex.Message}"));
            }
        }
    }
}