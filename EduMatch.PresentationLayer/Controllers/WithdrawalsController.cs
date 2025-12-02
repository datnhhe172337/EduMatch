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

                return StatusCode(StatusCodes.Status201Created, ApiResponse<string>.Ok("Yeu cau rut tien cua ban dang duoc xu ly."));
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

        //// Admin review is no longer required for withdrawals.
        //// [HttpGet("pending")]
        //// [Authorize(Roles = "3")] 
        //// [ProducesResponseType(typeof(ApiResponse<IEnumerable<AdminWithdrawalDto>>), StatusCodes.Status200OK)]
        //// [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        //// public async Task<IActionResult> GetPendingWithdrawals()
        //// {
        ////     var requests = await _withdrawalService.GetPendingWithdrawalsAsync();
        ////     return Ok(ApiResponse<IEnumerable<AdminWithdrawalDto>>.Ok(requests));
        //// }
    }
}
