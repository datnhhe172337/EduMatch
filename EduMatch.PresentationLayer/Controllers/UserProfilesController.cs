using Microsoft.AspNetCore.Mvc;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Services;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfilesController : ControllerBase
    {
        private readonly IUserProfileService _service;

        public UserProfilesController(IUserProfileService service)
        {
            _service = service;
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var profile = await _service.GetByEmailAsync(email);
            if (profile == null)
                return NotFound(new { message = $"User profile with email '{email}' not found." });

            return Ok(profile);
        }

        [HttpPut("{email}")]
        public async Task<IActionResult> UpdateUserProfile(string email, [FromBody] UpdateUserProfileDto dto)
        {
            var success = await _service.UpdateUserProfileAsync(email, dto);
            if (!success)
                return NotFound(new { message = "User profile not found." });

            return Ok(new { message = "User profile updated successfully." });
        }
    }
}
