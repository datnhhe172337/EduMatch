using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.PresentationLayer.Common;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.BusinessLogicLayer.Requests.User;
using EduMatch.BusinessLogicLayer.Requests.TutorProfile;
using EduMatch.BusinessLogicLayer.Constants;

namespace EduMatch.PresentationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfilesController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly ILogger<UserProfilesController> _logger;

        public UserProfilesController(IUserProfileService userProfileService, ILogger<UserProfilesController> logger)
        {
            _userProfileService = userProfileService;
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
                var profile = await _userProfileService.GetByEmailAsync(email);

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

       


        // Function of Datnh.m
         /// <summary>
		/// Cập nhật thông tin user profile
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Learner + "," + Roles.Tutor)]
		[HttpPut("update-user-profile")]
		[ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfileUpdateRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ApiResponse<string>.Fail("Invalid request."));

				var updatedProfile = await _userProfileService.UpdateAsync(request);
				return Ok(ApiResponse<UserProfileDto>.Ok(updatedProfile, "User profile updated successfully."));
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ApiResponse<string>.Fail(ex.Message));
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ApiResponse<string>.Fail(ex.Message));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Failed to update user profile.", ex.Message));
			}
		}

        /// <summary>
        /// Lấy ra danh sách tất cả các tỉnh
        /// </summary>
        /// <returns></returns>
        [HttpGet("provinves")]
        public async Task<IActionResult> GetProvincesAsync()
        {
            var provinces = await _userProfileService.GetProvincesAsync();
            return Ok(provinces);
        }

        /// <summary>
        /// Lấy ra danh sách tất cả các xã theo tỉnh
        /// </summary>
        /// <returns></returns>
        [HttpGet("subDistricts/{provinceId}")]
        public async Task<IActionResult> GetSubDistrictsByProvinceIdAsync(int provinceId)
        {
            var provinces = await _userProfileService.GetSubDistrictsByProvinceIdAsync(provinceId);
            return Ok(provinces);
        }

    }
}
