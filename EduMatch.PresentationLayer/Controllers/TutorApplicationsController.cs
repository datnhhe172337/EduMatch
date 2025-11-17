using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.BusinessLogicLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sprache;
using System.Security.Claims;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TutorApplicationsController : ControllerBase
    {
        private readonly ITutorApplicationService _service;

        public TutorApplicationsController(ITutorApplicationService service) 
        {
            _service = service; 
        }

        /// <summary>
        /// Gia sư ứng tuyến vào 1 class request -> Status = 0: Đang ứng tuyển 
        /// </summary>
        /// <param name="request">Bao gồm ClassRequestId và Message</param>
        /// <returns></returns>
        [HttpPost("apply")]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> TutorApplyAsync([FromBody] TutorApplyRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (userEmail == null)
                    return Unauthorized("Tutor ID not found in token.");

                await _service.TutorApplyAsync(request.ClassRequestId, userEmail, request.Message);
                return Ok(new { message = "Application submitted successfully" });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }

        }

        /// <summary>
        /// Lấy ra các gia sư đang ứng tuyển theo từng class request
        /// </summary>
        /// <param name="classRequestId"></param>
        /// <returns></returns>
        [HttpGet("class-request/{classRequestId}")]
        public async Task<IActionResult> GetTutorApplicationsByClassRequestIdAsync(int classRequestId)
        {
            var learnerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            //if (learnerEmail == null)
            //    return Unauthorized(new { message = "Invalid learner token" });

            var result = await _service.GetTutorApplicationsByClassRequestAsync(classRequestId, learnerEmail);
            return Ok(result);
        }

        /// <summary>
        /// Lấy ra các yêu cầu tạo lớp mà gia sư đang ứng tuyển(Lấy theo TutorEmail) -> Status = 0 -> Đang ứng tuyển
        /// </summary>
        /// <returns></returns>
        [HttpGet("tutor/applied")]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> GetTutorAppliedByTutorEmailAsync()
        {
            try
            {
                var tutorEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (tutorEmail == null)
                    return Unauthorized("Tutor email not found in token");

                var apps = await _service.GetTutorApplicationsByTutorAsync(tutorEmail);

                return Ok(apps);

            } catch (Exception ex) { return  BadRequest(ex); }
           
        }

        /// <summary>
        /// Lấy ra các ứng tuyển đã hủy bởi tutor (Status = 1 -> Đã hủy)
        /// </summary>
        /// <returns></returns>
        [HttpGet("tutor/canceled")]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> GetCanceledApplicationByTutorEmailAsync()
        {
            try
            {
                var tutorEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (tutorEmail == null)
                    return Unauthorized("Tutor email not found in token");

                var apps = await _service.GetCanceledApplicationsByTutorAsync(tutorEmail);

                return Ok(apps);

            }
            catch (Exception ex) { return BadRequest(ex); }

        }

        [HttpPut("edit")]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> EditApplication([FromBody] TutorApplicationEditRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var tutorEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (tutorEmail == null)
                    return Unauthorized("Tutor not found in token");

                await _service.EditTutorApplicationAsync(tutorEmail, request);
                return Ok(new { message = "Application updated successfully" });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>
        /// Gia sư Hủy yêu cầu ứng tuyển  -> Stauts = 1 : Đã hủy
        /// </summary>
        /// <param name="id">TutorApplicationId</param>
        /// <returns></returns>
        [HttpPut("cancel/{id}")]
        [Authorize(Roles = "Tutor")]
        public async Task<IActionResult> CancelApplicationAsync(int id)
        {
            var tutorEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (tutorEmail == null)
                return Unauthorized("Invalid token");

            try
            {
                await _service.CancelApplicationAsync(tutorEmail, id);
                return Ok(new { message = "Cancel application successfully" });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }


    }
}
