using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.TrialLesson;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using EduMatch.BusinessLogicLayer.Constants;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrialLessonsController : ControllerBase
    {
        private readonly ILearnerTrialLessonService _trialLessonService;
        private readonly CurrentUserService _currentUserService;

        public TrialLessonsController(
            ILearnerTrialLessonService trialLessonService,
            CurrentUserService currentUserService)
        {
            _trialLessonService = trialLessonService;
            _currentUserService = currentUserService;
        }
        /// <summary>
        /// Ghi nhận rằng học viên hiện tại đã học thử môn này với gia sư chỉ định.
        /// </summary>
        [Authorize (Roles = Roles.Learner)]
		[HttpPost]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RecordTrialLesson([FromBody] TrialLessonCreateRequest request)
        {
            var learnerEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(learnerEmail))
            {
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));
            }

            var created = await _trialLessonService.RecordTrialAsync(learnerEmail, request.TutorId, request.SubjectId);
            if (!created)
            {
                return Conflict(ApiResponse<string>.Fail("You already have a trial lesson with this tutor for this subject."));
            }

            return Ok(ApiResponse<string>.Ok("Trial lesson recorded."));
        }

		/// <summary>
		/// Kiểm tra học viên hiện tại đã học thử môn này với gia sư chỉ định hay chưa.
		/// </summary>
        [Authorize (Roles = Roles.Learner)]
		[HttpGet("exists")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> HasTrialLesson([FromQuery] int tutorId, [FromQuery] int subjectId)
        {
            var learnerEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(learnerEmail))
            {
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));
            }

            var exists = await _trialLessonService.HasTrialedAsync(learnerEmail, tutorId, subjectId);
            return Ok(ApiResponse<bool>.Ok(exists));
        }
		/// <summary>
		/// Liệt kê các môn của gia sư và đánh dấu môn nào học viên hiện tại đã học thử.
		/// </summary>
    
        [Authorize (Roles = Roles.Learner)]
		[HttpGet("subjects")]
        [ProducesResponseType(typeof(ApiResponse<List<TrialLessonSubjectStatusDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSubjectTrialStatuses([FromQuery] int tutorId)
        {
            var learnerEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(learnerEmail))
            {
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));
            }

            var statuses = await _trialLessonService.GetSubjectTrialStatusesAsync(learnerEmail, tutorId);
            return Ok(ApiResponse<IReadOnlyList<TrialLessonSubjectStatusDto>>.Ok(statuses));
        }
    }
}
