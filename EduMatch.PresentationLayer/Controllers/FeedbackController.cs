using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.PresentationLayer.Common;
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
        private readonly ITutorFeedbackService _feedbackService;
        private readonly ITutorRatingSummaryService _summaryService;

        public FeedbackController(ITutorFeedbackService feedbackService, ITutorRatingSummaryService summaryService)
        {
            _feedbackService = feedbackService;
            _summaryService = summaryService;
        }

        [Authorize(Roles = "Learner")]
        [HttpPost("Create-Feedback")]
        public async Task<IActionResult> CreateFeedback(CreateTutorFeedbackRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var learnerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(learnerEmail))
                    return Unauthorized(ApiResponse<string>.Fail("Không lấy được email người dùng"));

                var result = await _feedbackService.CreateFeedbackAsync(request, learnerEmail);
                return Ok(ApiResponse<TutorFeedbackDto>.Ok(result, "Gửi feedback gia sư thành công."));
            }
            catch (InvalidOperationException ex)
            {
                // Những lỗi nghiệp vụ như chưa đủ 80% số buổi
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                // Lỗi hệ thống
                return StatusCode(500, ApiResponse<string>.Fail("Đã xảy ra lỗi hệ thống: " + ex.Message));
            }
        }

        [HttpGet("Get-Feedback-By-Id/{feedbackId}")]
        public async Task<IActionResult> GetFeedbackById(int feedbackId)
        {
            try
            {
                var result = await _feedbackService.GetFeedbackByIdAsync(feedbackId);
                return Ok(ApiResponse<TutorFeedbackDto>.Ok(result, "Lấy feedback thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail("Đã xảy ra lỗi hệ thống: " + ex.Message));
            }
        }

        [HttpGet("Get-Feedback-By-Learner")]
        public async Task<IActionResult> GetFeedbackByLearner(string learnerEmail)
        {
            if (string.IsNullOrEmpty(learnerEmail))
                return Unauthorized(new { Message = "User authentication failed." });

            var feedbacks = await _feedbackService.GetFeedbackByLearnerEmailAsync(learnerEmail);
            return Ok(ApiResponse<List<TutorFeedbackDto>>.Ok(feedbacks, "Lấy feedback thành công"));
        }

        [HttpGet("Get-Feedback-By-Tutor")]
        public async Task<IActionResult> GetFeedbackByTutor(int tutorId)
        {
            if (tutorId < 0)
                return NotFound(new {Message = "Tutor not found"});
            var feedbacks = await _feedbackService.GetFeedbackByTutorIdAsync(tutorId);
            return Ok(ApiResponse<List<TutorFeedbackDto>>.Ok(feedbacks, "Lấy feedback thành công"));
        }

        [HttpGet("Get-All-Criteria")]
        public async Task<IActionResult> GetAllCriteria()
        {
            var criteria = await _feedbackService.GetAllCriteriaAsync();
            return Ok(ApiResponse<List<FeedbackCriterion>>.Ok(criteria, "Lấy danh sách tiêu chí thành công"));
        }

        [HttpGet("Get-All-Feedbacks")]
        public async Task<IActionResult> GetAllFeedbacks()
        {
            var feedbacks = await _feedbackService.GetAllFeedbacksAsync();
            return Ok(ApiResponse<List<TutorFeedbackDto>>.Ok(feedbacks, "Lấy tất cả feedback thành công"));
        }

        [Authorize(Roles = "Learner")]
        [HttpPut("Update-Feedback")]
        public async Task<IActionResult> UpdateFeedback(UpdateTutorFeedbackRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var learnerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(learnerEmail))
                    return Unauthorized(ApiResponse<string>.Fail("Không lấy được email người dùng"));

                var result = await _feedbackService.UpdateFeedbackAsync(request, learnerEmail);

                return Ok(ApiResponse<TutorFeedbackDto>.Ok(result, "Cập nhật feedback thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail("Đã xảy ra lỗi hệ thống: " + ex.Message));
            }
        }

        [HttpGet("summary/{tutorId}")]
        public async Task<IActionResult> GetTutorRatingSummary(int tutorId)
        {
            var summary = await _summaryService.GetByTutorIdAsync(tutorId);
            if (summary == null) return NotFound("Tutor rating summary not found");
            return Ok(summary);
        }
    }
}
