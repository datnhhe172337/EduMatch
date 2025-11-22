// Filename: UserBankAccountsController.cs
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests; // <-- ADD THIS
using EduMatch.BusinessLogicLayer.Requests.Bank;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    /// <summary>
    /// Handles CRUD operations for the authenticated user's saved bank accounts.
    /// </summary>
    public class UserBankAccountsController : ControllerBase
    {
        private readonly IUserBankAccountService _accountService;
        private readonly CurrentUserService _currentUserService;

        public UserBankAccountsController(IUserBankAccountService accountService, CurrentUserService currentUserService)
        {
            _accountService = accountService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Returns all bank accounts linked to the current user.
        /// </summary>
        [HttpGet("my-accounts")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserBankAccountDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMyAccounts()
        {
            try
            {
                string? userEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

                var accounts = await _accountService.GetAccountsByUserEmailAsync(userEmail);
                return Ok(ApiResponse<IEnumerable<UserBankAccountDto>>.Ok(accounts));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail("Failed to get user bank accounts.", ex.Message));
            }
        }

        /// <summary>
        /// Adds a new bank account for the current user.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserBankAccountDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddBankAccount([FromBody] AddUserBankAccountRequest request)
        {
            try
            {
                string? userEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

                var newAccountDto = await _accountService.AddAccountAsync(request, userEmail);

                return CreatedAtAction(nameof(GetMyAccounts), new { }, ApiResponse<UserBankAccountDto>.Ok(newAccountDto));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail("Failed to add bank account.", ex.Message));
            }
        }

        /// <summary>
        /// Removes the specified bank account if it belongs to the current user.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveBankAccount(int id)
        {
            try
            {
                string? userEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

                bool result = await _accountService.RemoveAccountAsync(id, userEmail);

                if (!result)
                {
                    return NotFound(ApiResponse<string>.Fail("Bank account not found or you do not have permission to delete it."));
                }

                return Ok(ApiResponse<string>.Ok("Bank account removed successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail("Failed to remove bank account. It may be in use.", ex.Message));
            }
        }
    }
}
