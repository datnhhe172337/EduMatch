using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.BusinessLogicLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassRequestsController : ControllerBase
    {
        private readonly IClassRequestService _service;

        public ClassRequestsController(IClassRequestService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Learner")]
        [HttpPost("Create")]
        public async Task<IActionResult> CreateClassRequestAsync([FromBody] ClassCreateRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var learnerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(learnerEmail))
                return Unauthorized(new { Message = "User authentication failed." });

            var result = await _service.CreateRequestToOpenClassAsync(dto, learnerEmail);
            if (result == null)
                return StatusCode(500, new { message = "Failed to create class request." });

            var response = new
            {
                id = result.Id,
                learnerEmail = result.LearnerEmail,
                title = result.Title,
                subjectId = result.SubjectId,
                levelId = result.LevelId,
                status = result.Status.ToString(),
                createdAt = result.CreatedAt
            };

            return CreatedAtRoute("GetById", new { id = result.Id }, response);
        }


        /// <summary>
        /// Lấy ra thông tin chi tiết yêu cầu tạo lớp đó theo id
        /// </summary>
        /// <param name="id"> Id của ClassRequest</param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetById")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var request = await _service.GetDetailsClassRequestByIdAsync(id);
            if (request == null)
                return NotFound(new { message = "Class request not found" });

            return Ok(request);
        }


        /// <summary>
        /// Lấy ra các yêu cầu tạo lớp với trạng thái là Pending cho Business Admin để BA duyệt/tự chối
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Business Admin")]
        [HttpGet("ListPending")]
        public async Task<IActionResult> GetPendingClassRequestsAsync()
        {
            try
            {
                var requests = await _service.GetPendingClassRequestsAsync();
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        /// <summary>
        /// Lấy ra tất cả các yêu cầu tạo lớp đã được duyệt (Status = "Open") và sắp xếp theo ngày tạo mới nhất
        /// </summary>
        [HttpGet("ListOpen")]
        public async Task<IActionResult> GetOpenClassRequestsAsync()
        {
            try
            {
                var requests = await _service.GetOpenClassRequestsAsync();
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Lấy ra các yêu cầu tạo lớp của Learner với Status = "Pending"
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Learner")]
        [HttpGet("ListPendingByLearnerEmail")]
        public async Task<IActionResult> GetPendingClassRequestsByLearnerEmailAsync()
        {
            try
            {
                var learnerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(learnerEmail))
                    return Unauthorized(new { message = "Invalid user token" });

                var requests = await _service.GetPendingClassRequestsByLearnerEmail(learnerEmail);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        [Authorize(Roles = "Learner")]
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateClassRequestAsync(int id, [FromBody] UpdateClassRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var learnerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(learnerEmail))
                return Unauthorized(new { message = "User authentication failed." });

            try
            {
                await _service.UpdateClassRequestAsync(id, request, learnerEmail);
                return Ok(new { message = "Class request updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        /// <summary>
        /// Nếu yêu cầu tạo lớp đó chưa được BA duyệt (tức là trạng thái của yêu cầu đó đang là "Pending" -> thì xóa cứng luôn
        /// </summary>
        /// <param name="id"> id của class request</param>
        /// <returns></returns>
        [Authorize(Roles = "Learner")]
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteClassRequestAsync(int id)
        {
            var learnerEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(learnerEmail))
                return Unauthorized(new { Message = "User authentication failed." });

            try
            {
                await _service.DeleteClassRequestAsync(id, learnerEmail);
                return Ok(new { message = "Delete successful." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Business Admin duyệt (true) hoặc từ chối (false) yêu cầu tạo lớp từ người học (Nếu từ chối thì phải ghi thêm lý do từ chối)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Authorize(Roles = "Business Admin")]
        [HttpPut("ApproveOrReject/{id}")]
        public async Task<IActionResult> ApproveOrRejectClassRequestAsync(int id, [FromBody] IsApprovedClassRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var businessAdmin = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(businessAdmin))
                return Unauthorized(new { message = "User authentication failed." });

            try
            {
                await _service.ApproveOrRejectClassRequestAsync(id, businessAdmin, dto);
                string message = dto.IsApproved
                    ? "Class request approved successfully."
                    : "Class request rejected successfully.";
                return Ok(new 
                { 
                    message = message,
                    approvedAt = DateTime.Now,
                    approvedBy = businessAdmin,
                    rejectionReason = dto.RejectionReason
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy ra các yêu cầu tạo lớp của Learner với Status = "Open"
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Learner")]
        [HttpGet("ListOpenByLearnerEmail")]
        public async Task<IActionResult> GetOpenClassRequestsByLearnerEmailAsync()
        {
            try
            {
                var learnerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(learnerEmail))
                    return Unauthorized(new { message = "Invalid user token" });

                var requests = await _service.GetOpenClassRequestsByLearnerEmail(learnerEmail);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Lấy ra các yêu cầu tạo lớp của Learner với Status = "Expired" -> yêu cầu tạo lớp đó đã quá hạn (hạn là 14 ngày kể từ ngày BA duyệt)
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Learner")]
        [HttpGet("ListExpiredByLearnerEmail")]
        public async Task<IActionResult> GetExpiredClassRequestsByLearnerEmailAsync()
        {
            try
            {
                var learnerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(learnerEmail))
                    return Unauthorized(new { message = "Invalid user token" });

                var requests = await _service.GetExpiredClassRequestsByLearnerEmail(learnerEmail);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Lấy ra các yêu cầu tạo lớp của Learner với Status = "Rejected" -> các yêu cầu đã bị BA từ chối
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Learner")]
        [HttpGet("ListRejectedByLearnerEmail")]
        public async Task<IActionResult> GetRejectedClassRequestsByLearnerEmailAsync()
        {
            try
            {
                var learnerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(learnerEmail))
                    return Unauthorized(new { message = "Invalid user token" });

                var requests = await _service.GetRejectedClassRequestsByLearnerEmail(learnerEmail);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [Authorize(Roles = "Learner")]
        [HttpPut("Cancel/{id}")]
        public async Task<IActionResult> CancelClassRequest(int id, [FromBody] CancelClassRequestDto dto)
        {
            try
            {
                var learnerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (learnerEmail == null)
                    return Unauthorized(new { message = "User not authenticated" });

                await _service.CancelClassRequestAsync(id, learnerEmail, dto);
                return Ok(new { message = "Class request cancelled successfully" });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        /// <summary>
        /// Lấy ra các yêu cầu tạo lớp của Learner với Status = "Canceled" -> các yêu cầu tạo lớp đã hủy bởi learner
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Learner")]
        [HttpGet("ListCanceledByLearnerEmail")]
        public async Task<IActionResult> GetCanceledClassRequestsByLearnerEmailAsync()
        {
            try
            {
                var learnerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(learnerEmail))
                    return Unauthorized(new { message = "Invalid user token" });

                var requests = await _service.GetCanceledClassRequestsByLearnerEmail(learnerEmail);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [Authorize(Roles = "Learner")]
        [HttpGet("listAllClassRequestByLearnerEmail")]
        public async Task<IActionResult> GetListAllClassRequestsByLearnerEmailAsync()
        {
            try
            {
                var learnerEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(learnerEmail))
                    return Unauthorized(new { message = "Invalid user token" });

                var requests = await _service.GetListAllClassRequestsByLearnerEmail(learnerEmail);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }



    }
}
