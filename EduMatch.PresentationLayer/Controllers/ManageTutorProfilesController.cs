using Microsoft.AspNetCore.Mvc;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.PresentationLayer.Common;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageTutorProfilesController : ControllerBase
    {
        private readonly IManageTutorProfileService _service;
        private readonly ILogger<ManageTutorProfilesController> _logger;

        public ManageTutorProfilesController(IManageTutorProfileService service, ILogger<ManageTutorProfilesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get tutor profile by Id
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var profile = await _service.GetByIdAsync(id);
                if (profile == null)
                    return NotFound(ApiResponse<TutorProfile>.Fail($"Tutor profile with id {id} not found."));

                return Ok(ApiResponse<TutorProfile>.Ok(profile, "Tutor profile retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tutor profile with ID {Id}", id);
                return StatusCode(500, ApiResponse<TutorProfile>.Fail("An error occurred while retrieving tutor profile.", ex.Message));
            }
        }

        /// <summary>
        /// Get tutor profile by user email
        /// </summary>
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            try
            {
                var profile = await _service.GetByEmailAsync(email);
                if (profile == null)
                    return NotFound(ApiResponse<TutorProfile>.Fail($"Tutor profile with email '{email}' not found."));

                return Ok(ApiResponse<TutorProfile>.Ok(profile, "Tutor profile retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tutor profile with email {Email}", email);
                return StatusCode(500, ApiResponse<TutorProfile>.Fail("An error occurred while retrieving tutor profile.", ex.Message));
            }
        }

        /// <summary>
        /// Update tutor profile
        /// </summary>
        [HttpPut("{email}")]
        public async Task<IActionResult> UpdateTutorProfile(string email, [FromBody] UpdateTutorProfileDto dto)
        {
            if (dto == null)
                return BadRequest(ApiResponse<string>.Fail("Invalid request body."));

            try
            {
                var success = await _service.UpdateTutorProfileAsync(email, dto);

                if (!success)
                    return NotFound(ApiResponse<string>.Fail("Tutor profile not found or update failed."));

                return Ok(ApiResponse<string>.Ok(null, "Tutor profile updated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tutor profile for {Email}", email);
                return StatusCode(500, ApiResponse<string>.Fail("An error occurred while updating tutor profile.", ex.Message));
            }
        }
    }
}
