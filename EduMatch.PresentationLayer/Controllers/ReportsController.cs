using EduMatch.BusinessLogicLayer.Constants;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Report;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly CurrentUserService _currentUserService;

        public ReportsController(IReportService reportService, CurrentUserService currentUserService)
        {
            _reportService = reportService;
            _currentUserService = currentUserService;
        }

        [Authorize(Roles = Roles.Learner + "," + Roles.Tutor)]
        [HttpPost]
        public async Task<IActionResult> CreateReportAsync([FromBody] ReportCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid request payload.", ModelState));

            var reporterEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(reporterEmail))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            try
            {
                var result = await _reportService.CreateReportAsync(request, reporterEmail);
                return Ok(ApiResponse<ReportDetailDto>.Ok(result, "Report created successfully."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = Roles.Learner)]
        [HttpGet("learner")]
        public async Task<IActionResult> GetReportsByLearnerAsync()
        {
            var learnerEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(learnerEmail))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            try
            {
                var reports = await _reportService.GetReportsByReporterAsync(learnerEmail);
                return Ok(ApiResponse<IReadOnlyList<ReportListItemDto>>.Ok(reports, "Reports retrieved successfully."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = Roles.Learner)]
        [HttpPut("{id:int}/learner")]
        public async Task<IActionResult> UpdateReportByLearnerAsync(int id, [FromBody] ReportUpdateByLearnerRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid request payload.", ModelState));

            var learnerEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(learnerEmail))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            try
            {
                var result = await _reportService.UpdateReportByLearnerAsync(id, request, learnerEmail);
                return Ok(ApiResponse<ReportDetailDto>.Ok(result, "Report updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = Roles.Learner)]
        [HttpDelete("{id:int}/learner")]
        public async Task<IActionResult> CancelReportByLearnerAsync(int id)
        {
            var learnerEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(learnerEmail))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            try
            {
                var result = await _reportService.CancelReportByLearnerAsync(id, learnerEmail);
                return Ok(ApiResponse<ReportDetailDto>.Ok(result, "Report canceled successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = Roles.Tutor)]
        [HttpGet("tutor")]
        public async Task<IActionResult> GetReportsByTutorAsync()
        {
            var tutorEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(tutorEmail))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            try
            {
                var reports = await _reportService.GetReportsByReportedUserAsync(tutorEmail);
                return Ok(ApiResponse<IReadOnlyList<ReportListItemDto>>.Ok(reports, "Reports retrieved successfully."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetReportDetailAsync(int id)
        {
            var requesterEmail = _currentUserService.Email;
            var isAdmin = User.IsInRole(Roles.BusinessAdmin) || User.IsInRole(Roles.SystemAdmin);

            try
            {
                var report = await _reportService.GetReportDetailAsync(id, requesterEmail, isAdmin);
                if (report == null)
                    return NotFound(ApiResponse<string>.Fail("Report not found."));

                return Ok(ApiResponse<ReportDetailDto>.Ok(report));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [Authorize(Roles = Roles.BusinessAdmin + "," + Roles.SystemAdmin)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateReportAsync(int id, [FromBody] ReportUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid request payload.", ModelState));

            var adminEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(adminEmail))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            try
            {
                var result = await _reportService.UpdateReportAsync(id, request, adminEmail);
                return Ok(ApiResponse<ReportDetailDto>.Ok(result, "Report updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = Roles.Tutor)]
        [HttpPut("{id:int}/complaint")]
        public async Task<IActionResult> SubmitTutorComplaintAsync(int id, [FromBody] TutorComplaintRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid request payload.", ModelState));

            var tutorEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(tutorEmail))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            try
            {
                var result = await _reportService.SubmitTutorComplaintAsync(id, request, tutorEmail);
                return Ok(ApiResponse<ReportDetailDto>.Ok(result, "Defense submitted successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [Authorize(Roles = Roles.BusinessAdmin + "," + Roles.SystemAdmin)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteReportAsync(int id)
        {
            try
            {
                await _reportService.DeleteReportAsync(id);
                return Ok(ApiResponse<string>.Ok(null, "Report deleted successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(ex.Message));
            }
        }
    }
}
