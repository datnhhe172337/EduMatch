using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EduMatch.PresentationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FindTutorController : ControllerBase
    {
        private readonly IFindTutorService _findTutorService;

        public FindTutorController(IFindTutorService findTutorService)
        {
            _findTutorService = findTutorService;
        }

        /// <summary>
        /// Get all tutors
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllTutors()
        {
            var tutors = await _findTutorService.GetAllTutorsAsync();
            return Ok(tutors);
        }

        /// <summary>
        /// Search and filter tutors
        /// </summary>
        [HttpPost("search")]
        public async Task<IActionResult> SearchTutors([FromBody] TutorFilterDto filter)
        {
            var tutors = await _findTutorService.SearchTutorsAsync(filter);
            return Ok(tutors);
        }
    }
}
