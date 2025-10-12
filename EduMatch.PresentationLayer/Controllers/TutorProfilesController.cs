using Microsoft.AspNetCore.Mvc;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Services;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TutorProfilesController : ControllerBase
    {
        private readonly ITutorProfileService _service;

        public TutorProfilesController(ITutorProfileService service)
        {
            _service = service;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var profile = await _service.GetByIdAsync(id);
            if (profile == null)
                return NotFound(new { message = $"Tutor profile with id {id} not found." });

            return Ok(profile);
        }

        /// <summary>
        /// Get tutor profile by user email
        /// </summary>
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var profile = await _service.GetByEmailAsync(email);
            if (profile == null)
                return NotFound(new { message = $"Tutor profile with email '{email}' not found." });

            return Ok(profile);
        }

        [HttpPut("{email}")]
        public async Task<IActionResult> UpdateTutorProfile(string email, [FromBody] UpdateTutorProfileDto dto)
        {
            var success = await _service.UpdateTutorProfileAsync(email, dto);
            if (!success)
                return NotFound(new { message = "Tutor profile not found." });

            return Ok(new { message = "Tutor profile updated successfully." });
        }
    }
}
