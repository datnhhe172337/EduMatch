using Microsoft.AspNetCore.Mvc;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.PresentationLayer.Common;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.BusinessLogicLayer.Services;
using Microsoft.AspNetCore.Authorization;


namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageTutorProfilesController : ControllerBase
    {
        // --- RENAMED this field for clarity ---
        private readonly IManageTutorProfileService _manageTutorProfileService;
        private readonly ILogger<ManageTutorProfilesController> _logger;

        // --- ADDED these two fields ---
        private readonly CurrentUserService _currentUserService;
        private readonly EduMatchContext _eduMatch; // For transactions

        public ManageTutorProfilesController(
            IManageTutorProfileService service,
            ILogger<ManageTutorProfilesController> logger,
            // --- ADD these to your constructor ---
            CurrentUserService currentUserService,
            EduMatchContext eduMatch)
        {
            _manageTutorProfileService = service; // --- Fixed name assignment ---
            _logger = logger;
            _currentUserService = currentUserService; // --- Added ---
            _eduMatch = eduMatch; // --- Added ---
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


        //[Authorize]
        // --- CHANGED: Added email parameter to the route ---
        [HttpPut("update-profile/{email}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<TutorProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)] // Added NotFound
                                                                                           // --- CHANGED: Added email parameter to the method ---
        public async Task<IActionResult> UpdateTutorProfile([FromRoute] string email, [FromForm] UpdateTutorProfileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid request.", ModelState));

            // --- CHANGED: Use the email from the route parameter ---
            var userEmail = email;

            // Validate the provided email (optional but good practice)
            if (string.IsNullOrWhiteSpace(userEmail))
                return BadRequest(ApiResponse<string>.Fail("Email parameter cannot be empty."));

            // --- You might want to compare the route email with the logged-in user for security ---
            // var loggedInUserEmail = _currentUserService.Email;
            // if (!string.Equals(userEmail, loggedInUserEmail, StringComparison.OrdinalIgnoreCase))
            // {
            //     return Forbid("You are not authorized to update this profile."); // Or Unauthorized
            // }

            var existingProfile = await _manageTutorProfileService.GetByEmailAsync(userEmail);
            if (existingProfile == null)
                return NotFound(ApiResponse<string>.Fail($"Tutor profile for email '{userEmail}' not found."));

            await using var tx = await _eduMatch.Database.BeginTransactionAsync();
            try
            {
                var updatedProfile = await _manageTutorProfileService.UpdateTutorProfileAsync(existingProfile.Id, request);

                await tx.CommitAsync();

                return Ok(ApiResponse<object>.Ok(new
                {
                    profile = updatedProfile
                }, "Tutor profile updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(
                    "Failed to update tutor profile.",
                    new { exception = ex.Message, innerException = ex.InnerException?.Message }
                ));
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