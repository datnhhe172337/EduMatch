using Microsoft.AspNetCore.Mvc;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.PresentationLayer.Common;
using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.PresentationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfilesController : ControllerBase
    {
        private readonly IUserProfileService _service;
        private readonly ILogger<UserProfilesController> _logger;

        public UserProfilesController(IUserProfileService service, ILogger<UserProfilesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get user profile by email
        /// </summary>
        [HttpGet("{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(ApiResponse<string>.Fail("Email is required."));

            try
            {
                var profile = await _service.GetByEmailAsync(email);

                if (profile == null)
                    return NotFound(ApiResponse<string>.Fail($"User profile with email '{email}' not found."));

                return Ok(ApiResponse<UserProfile>.Ok(profile, "User profile retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user profile for email: {Email}", email);
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while retrieving the user profile.", ex.Message));
            }
        }

        /// <summary>
        /// Update user profile by email
        /// </summary>
        [HttpPut("{email}")]
        public async Task<IActionResult> UpdateUserProfile(string email, [FromBody] UpdateUserProfileDto dto)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(ApiResponse<string>.Fail("Email is required."));

            if (dto == null)
                return BadRequest(ApiResponse<string>.Fail("Invalid request body."));

            try
            {
                var success = await _service.UpdateUserProfileAsync(email, dto);

                if (!success)
                    return NotFound(ApiResponse<string>.Fail($"User profile with email '{email}' not found."));

                return Ok(ApiResponse<string>.Ok("User profile updated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile for email: {Email}", email);
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while updating the user profile.", ex.Message));
            }
        }
    }
}
