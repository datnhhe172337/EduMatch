using Microsoft.AspNetCore.Mvc;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.TutorProfile;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.PresentationLayer.Common;
using EduMatch.BusinessLogicLayer.Services;
using Microsoft.AspNetCore.Authorization;
using EduMatch.BusinessLogicLayer.Requests.User;


namespace EduMatch.PresentationLayer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ManageTutorProfilesController : ControllerBase
    {
        // --- RENAMED this field for clarity ---
        private readonly IManageTutorProfileService _manageTutorProfileService;
        private readonly ILogger<ManageTutorProfilesController> _logger;
        private readonly ITutorProfileService _tutorProfileService;

        // --- ADDED these two fields ---
        private readonly CurrentUserService _currentUserService;
        private readonly EduMatchContext _eduMatch; // For transactions

        public ManageTutorProfilesController(
            IManageTutorProfileService service,
            ILogger<ManageTutorProfilesController> logger,
            // --- ADD these to your constructor ---
            CurrentUserService currentUserService,
            ITutorProfileService tutorProfileService,
            EduMatchContext eduMatch)
        {
            _manageTutorProfileService = service; 
            _logger = logger;
            _tutorProfileService = tutorProfileService;
            _currentUserService = currentUserService; 
            _eduMatch = eduMatch; 
        }

        /// <summary>
        /// Get tutor profile by Id
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                // --- Fixed variable name ---
                var profile = await _manageTutorProfileService.GetByIdFullAsync(id); // Use GetByIdFullAsync
                if (profile == null)
                    return NotFound(ApiResponse<TutorProfileDto>.Fail($"Tutor profile with id {id} not found."));

                return Ok(ApiResponse<TutorProfileDto>.Ok(profile, "Tutor profile retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tutor profile with ID {Id}", id);
                return StatusCode(500, ApiResponse<TutorProfileDto>.Fail("An error occurred while retrieving tutor profile.", ex.Message));
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
                // --- Fixed variable name ---
                var profile = await _manageTutorProfileService.GetByEmailAsync(email);
                if (profile == null)
                    return NotFound(ApiResponse<TutorProfileDto>.Fail($"Tutor profile with email '{email}' not found."));

                return Ok(ApiResponse<TutorProfileDto>.Ok(profile, "Tutor profile retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tutor profile with email {Email}", email);
                return StatusCode(500, ApiResponse<TutorProfileDto>.Fail("An error occurred while retrieving tutor profile.", ex.Message));
            }
        }


       
    }
}


///// <summary>
///// Update tutor profile
///// </summary>
//[HttpPut("{email}")]
//public async Task<IActionResult> UpdateTutorProfile(string email, [FromBody] UpdateTutorProfileDto dto)
//{
//    if (dto == null)
//        return BadRequest(ApiResponse<string>.Fail("Invalid request body."));

//    try
//    {
//        var success = await _service.UpdateTutorProfileAsync(email, dto);

//        if (!success)
//            return NotFound(ApiResponse<string>.Fail("Tutor profile not found or update failed."));

//        return Ok(ApiResponse<string>.Ok(null, "Tutor profile updated successfully."));
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "Error updating tutor profile for {Email}", email);
//        return StatusCode(500, ApiResponse<string>.Fail("An error occurred while updating tutor profile.", ex.Message));
//    }
//}