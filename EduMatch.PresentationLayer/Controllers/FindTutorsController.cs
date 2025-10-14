using Microsoft.AspNetCore.Mvc;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.PresentationLayer.Common;

namespace EduMatch.PresentationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FindTutorController : ControllerBase
    {
        private readonly IFindTutorService _findTutorService;
        private readonly ILogger<FindTutorController> _logger;

        public FindTutorController(IFindTutorService findTutorService, ILogger<FindTutorController> logger)
        {
            _findTutorService = findTutorService;
            _logger = logger;
        }

        /// <summary>
        /// Get all tutors (for public browsing)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllTutors()
        {
            try
            {
                var tutors = await _findTutorService.GetAllTutorsAsync();

                if (tutors == null || !tutors.Any())
                    return Ok(ApiResponse<IEnumerable<TutorProfile>>.Ok([], "No tutors found."));

                return Ok(ApiResponse<IEnumerable<TutorProfile>>.Ok(tutors, "Tutors retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all tutors.");
                return StatusCode(500, ApiResponse<IEnumerable<TutorProfile>>.Fail("An error occurred while retrieving tutors.", ex.Message));
            }
        }

        /// <summary>
        /// Search and filter tutors using multiple criteria (keyword, city, gender, etc.)
        /// </summary>
        [HttpPost("search")]
        public async Task<IActionResult> SearchTutors([FromBody] TutorFilterDto filter)
        {
            if (filter == null)
                return BadRequest(ApiResponse<string>.Fail("Invalid search filter."));

            try
            {
                var tutors = await _findTutorService.SearchTutorsAsync(filter);

                if (tutors == null || !tutors.Any())
                    return Ok(ApiResponse<IEnumerable<TutorProfile>>.Ok([], "No tutors matched the filter criteria."));

                return Ok(ApiResponse<IEnumerable<TutorProfile>>.Ok(tutors, "Tutors retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching tutors with filter: {@Filter}", filter);
                return StatusCode(500, ApiResponse<IEnumerable<TutorProfile>>.Fail("An error occurred while searching tutors.", ex.Message));
            }
        }
    }
}
