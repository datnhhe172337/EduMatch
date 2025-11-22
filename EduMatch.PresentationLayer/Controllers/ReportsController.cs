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

        /// <summary>
        /// Creates a report from the current Learner or Tutor.
        /// </summary>
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

        /// <summary>
        /// Returns all reports submitted by the current learner.
        /// </summary>
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

        /// <summary>
        /// Allows a learner to update their pending report.
        /// </summary>
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

        /// <summary>
        /// Allows a learner to cancel their pending report.
        /// </summary>
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

        /// <summary>
        /// Returns reports where the current tutor is accused.
        /// </summary>
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

        /// <summary>
        /// Gets detailed report information. Restricted to admins or involved users.
        /// </summary>
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

        /// <summary>
        /// Attaches evidence to a report (reporter, reported tutor, or admin).
        /// </summary>
        [Authorize(Roles = Roles.Learner + "," + Roles.Tutor + "," + Roles.BusinessAdmin + "," + Roles.SystemAdmin)]
        [HttpPost("{id:int}/evidence")]
        public async Task<IActionResult> AddEvidenceAsync(int id, [FromBody] ReportEvidenceCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid request payload.", ModelState));

            var userEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(userEmail))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            var isAdmin = User.IsInRole(Roles.BusinessAdmin) || User.IsInRole(Roles.SystemAdmin);

            try
            {
                var result = await _reportService.AddEvidenceAsync(id, request, userEmail, isAdmin);
                return Ok(ApiResponse<ReportEvidenceDto>.Ok(result, "Evidence added successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
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

        /// <summary>
        /// Lists evidence for a report (admins or involved users).
        /// </summary>
        [Authorize(Roles = Roles.Learner + "," + Roles.Tutor + "," + Roles.BusinessAdmin + "," + Roles.SystemAdmin)]
        [HttpGet("{id:int}/evidence")]
        public async Task<IActionResult> GetEvidenceAsync(int id)
        {
            var userEmail = _currentUserService.Email;
            var isAdmin = User.IsInRole(Roles.BusinessAdmin) || User.IsInRole(Roles.SystemAdmin);

            try
            {
                var result = await _reportService.GetEvidenceByReportIdAsync(id, userEmail, isAdmin);
                return Ok(ApiResponse<IReadOnlyList<ReportEvidenceDto>>.Ok(result, "Evidence retrieved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Updates an evidence item (owner, reporter, accused tutor, or admin).
        /// </summary>
        [Authorize(Roles = Roles.Learner + "," + Roles.Tutor + "," + Roles.BusinessAdmin + "," + Roles.SystemAdmin)]
        [HttpPut("{id:int}/evidence/{evidenceId:int}")]
        public async Task<IActionResult> UpdateEvidenceAsync(int id, int evidenceId, [FromBody] ReportEvidenceUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid request payload.", ModelState));

            var userEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(userEmail))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            var isAdmin = User.IsInRole(Roles.BusinessAdmin) || User.IsInRole(Roles.SystemAdmin);

            try
            {
                var result = await _reportService.UpdateEvidenceAsync(id, evidenceId, request, userEmail, isAdmin);
                return Ok(ApiResponse<ReportEvidenceDto>.Ok(result, "Evidence updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
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

        /// <summary>
        /// Deletes an evidence item (owner, reporter, accused tutor, or admin).
        /// </summary>
        [Authorize(Roles = Roles.Learner + "," + Roles.Tutor + "," + Roles.BusinessAdmin + "," + Roles.SystemAdmin)]
        [HttpDelete("{id:int}/evidence/{evidenceId:int}")]
        public async Task<IActionResult> DeleteEvidenceAsync(int id, int evidenceId)
        {
            var userEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(userEmail))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            var isAdmin = User.IsInRole(Roles.BusinessAdmin) || User.IsInRole(Roles.SystemAdmin);

            try
            {
                await _reportService.DeleteEvidenceAsync(id, evidenceId, userEmail, isAdmin);
                return Ok(ApiResponse<string>.Ok(null, "Evidence deleted successfully."));
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

        /// <summary>
        /// Allows admin to update report status and notes.
        /// </summary>
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

        /// <summary>
        /// Tutor submits defense/complaint for a report.
        /// </summary>
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

        /// <summary>
        /// Tutor or admin adds a defense with optional evidences.
        /// </summary>
        [Authorize(Roles = Roles.Tutor + "," + Roles.BusinessAdmin + "," + Roles.SystemAdmin)]
        [HttpPost("{id:int}/defenses")]
        public async Task<IActionResult> AddDefenseAsync(int id, [FromBody] ReportDefenseCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid request payload.", ModelState));

            var userEmail = _currentUserService.Email;
            if (string.IsNullOrWhiteSpace(userEmail))
                return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

            var isAdmin = User.IsInRole(Roles.BusinessAdmin) || User.IsInRole(Roles.SystemAdmin);

            try
            {
                var result = await _reportService.AddDefenseAsync(id, request, userEmail, isAdmin);
                return Ok(ApiResponse<ReportDefenseDto>.Ok(result, "Defense added successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
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

        /// <summary>
        /// Lists defenses for a report (admin or involved users).
        /// </summary>
        [Authorize(Roles = Roles.Learner + "," + Roles.Tutor + "," + Roles.BusinessAdmin + "," + Roles.SystemAdmin)]
        [HttpGet("{id:int}/defenses")]
        public async Task<IActionResult> GetDefensesAsync(int id)
        {
            var userEmail = _currentUserService.Email;
            var isAdmin = User.IsInRole(Roles.BusinessAdmin) || User.IsInRole(Roles.SystemAdmin);

            try
            {
                var result = await _reportService.GetDefensesAsync(id, userEmail, isAdmin);
                return Ok(ApiResponse<IReadOnlyList<ReportDefenseDto>>.Ok(result, "Defenses retrieved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Gets full report detail including defenses and evidences.
        /// </summary>
        [Authorize]
        [HttpGet("{id:int}/full")]
        public async Task<IActionResult> GetFullReportAsync(int id)
        {
            var requesterEmail = _currentUserService.Email;
            var isAdmin = User.IsInRole(Roles.BusinessAdmin) || User.IsInRole(Roles.SystemAdmin);

            try
            {
                var report = await _reportService.GetFullReportDetailAsync(id, requesterEmail, isAdmin);
                if (report == null)
                    return NotFound(ApiResponse<string>.Fail("Report not found."));

                return Ok(ApiResponse<ReportFullDetailDto>.Ok(report));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        ///// <summary>
        ///// Permanently deletes a report (admin only).
        ///// </summary>
        //[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.SystemAdmin)]
        //[HttpDelete("{id:int}")]
        //public async Task<IActionResult> DeleteReportAsync(int id)
        //{
        //    try
        //    {
        //        await _reportService.DeleteReportAsync(id);
        //        return Ok(ApiResponse<string>.Ok(null, "Report deleted successfully."));
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(ApiResponse<string>.Fail(ex.Message));
        //    }
        //}
    }
}
