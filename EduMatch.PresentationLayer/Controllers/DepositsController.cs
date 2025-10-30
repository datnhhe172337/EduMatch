// Filename: DepositsController.cs
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.BusinessLogicLayer.Requests.Wallet;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepositsController : ControllerBase
    {
        private readonly IDepositService _depositService;
        private readonly CurrentUserService _currentUserService;

        public DepositsController(IDepositService depositService, CurrentUserService currentUserService)
        {
            _depositService = depositService;
            _currentUserService = currentUserService;
        }

        // POST: api/deposits/create-request
        [HttpPost("create-request")]
        [ProducesResponseType(typeof(ApiResponse<CreateDepositResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDepositRequest([FromBody] CreateDepositRequest request)
        {
            try
            {
                string? userEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(userEmail))
                    return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

                var response = await _depositService.CreateDepositRequestAsync(request, userEmail);

                // Return 201 Created status, which is standard for a successful POST
                return StatusCode(StatusCodes.Status201Created, ApiResponse<CreateDepositResponseDto>.Ok(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail("Failed to create deposit request.", ex.Message));
            }
        }
    }
}