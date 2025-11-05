using EduMatch.BusinessLogicLayer.DTOs;
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

        [HttpGet("my-requests")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<WithdrawalDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMyWithdrawalRequests()
        {
            try
            {
                string? userEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

                var requests = await _withdrawalService.GetWithdrawalHistoryAsync(userEmail);

                return Ok(ApiResponse<IEnumerable<WithdrawalDto>>.Ok(requests));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail("An error occurred while retrieving withdrawal requests.", ex.Message));
            }
        }

        [HttpGet("pending")]
        [Authorize(Roles = "3")] 
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AdminWithdrawalDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPendingWithdrawals()
        {
            try
            {
                var requests = await _withdrawalService.GetPendingWithdrawalsAsync();
                return Ok(ApiResponse<IEnumerable<AdminWithdrawalDto>>.Ok(requests));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        // POST: api/withdrawals/5/approve
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "3")] 
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApproveWithdrawal(int id)
        {
            try
            {
                string? adminEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(adminEmail))
                    return Unauthorized(ApiResponse<string>.Fail("Admin email not found in token."));

                await _withdrawalService.ApproveWithdrawalAsync(id, adminEmail);
                return Ok(ApiResponse<string>.Ok("Withdrawal approved."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        // POST: api/withdrawals/5/reject
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "3")] 
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RejectWithdrawal(int id, [FromBody] RejectRequest reason)
        {
            try
            {
                string? adminEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(adminEmail))
                    return Unauthorized(ApiResponse<string>.Fail("Admin email not found in token."));

                if (string.IsNullOrWhiteSpace(reason.Reason))
                    return BadRequest(ApiResponse<string>.Fail("A reason is required to reject a withdrawal."));

                await _withdrawalService.RejectWithdrawalAsync(id, adminEmail, reason.Reason);
                return Ok(ApiResponse<string>.Ok("Withdrawal rejected and funds returned to user."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }
    }
    public class RejectRequest
    {
        public string Reason { get; set; }
    }
}