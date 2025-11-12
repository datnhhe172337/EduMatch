using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [Authorize(Roles = "Learner")]
        [HttpPost("Create-Feedback")]
        public async Task<IActionResult> CreateFeedback([FromBody] CreateTutorFeedbackRequest request)
        {
            string learnerEmail = User.FindFirst(ClaimTypes.Email)?.Value!;
            var result = await _feedbackService.CreateFeedbackAsync(request, learnerEmail);
            return Ok(result);
        }
    }
}
