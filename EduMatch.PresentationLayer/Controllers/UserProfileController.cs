using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.User;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserProfileController : ControllerBase
	{
		private readonly IUserProfileService _userProfileService;

		public UserProfileController(IUserProfileService userProfileService)
		{
			_userProfileService = userProfileService;
		}

		/// <summary>
		/// Cập nhật thông tin user profile
		/// </summary>
		[Authorize]
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
	}
}
