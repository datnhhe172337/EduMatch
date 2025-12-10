using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.ScheduleChangeRequest;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.PresentationLayer.Common;
using System.ComponentModel.DataAnnotations;
using EduMatch.BusinessLogicLayer.Constants;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ScheduleChangeRequestController : ControllerBase
	{
		private readonly IScheduleChangeRequestService _scheduleChangeRequestService;

		/// <summary>
		/// API ScheduleChangeRequest: lấy theo Id, tạo, cập nhật Status, lấy danh sách theo RequesterEmail/RequestedToEmail
		/// </summary>
		public ScheduleChangeRequestController(IScheduleChangeRequestService scheduleChangeRequestService)
		{
			_scheduleChangeRequestService = scheduleChangeRequestService;
		}

		/// <summary>
		/// Lấy ScheduleChangeRequest theo Id
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpGet("get-by-id/{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<ScheduleChangeRequestDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<ScheduleChangeRequestDto>>> GetById(int id)
		{
			try
			{
				if (id <= 0)
				{
					return BadRequest(ApiResponse<object>.Fail("Id phải lớn hơn 0"));
				}

				var item = await _scheduleChangeRequestService.GetByIdAsync(id);
				if (item == null)
					return NotFound(ApiResponse<object>.Fail("Không tìm thấy ScheduleChangeRequest"));
				return Ok(ApiResponse<ScheduleChangeRequestDto>.Ok(item));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Tạo ScheduleChangeRequest mới
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpPost("create")]
		[ProducesResponseType(typeof(ApiResponse<ScheduleChangeRequestDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<ScheduleChangeRequestDto>>> Create([FromBody] ScheduleChangeRequestCreateRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", ModelState));
				}

				var created = await _scheduleChangeRequestService.CreateAsync(request);
				return Ok(ApiResponse<ScheduleChangeRequestDto>.Ok(created, "Tạo ScheduleChangeRequest thành công"));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Cập nhật Status của ScheduleChangeRequest
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpPut("update-status/{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<ScheduleChangeRequestDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<ScheduleChangeRequestDto>>> UpdateStatus(
			int id,
			[FromQuery] ScheduleChangeRequestStatus status)
		{
			try
			{
				if (id <= 0)
				{
					return BadRequest(ApiResponse<object>.Fail("Id phải lớn hơn 0"));
				}

				if (!Enum.IsDefined(typeof(ScheduleChangeRequestStatus), status))
				{
					return BadRequest(ApiResponse<object>.Fail("Status không hợp lệ"));
				}

				var updated = await _scheduleChangeRequestService.UpdateStatusAsync(id, status);
				return Ok(ApiResponse<ScheduleChangeRequestDto>.Ok(updated, "Cập nhật Status thành công"));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Lấy danh sách ScheduleChangeRequest theo RequesterEmail
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpGet("get-all-by-requester-email")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<ScheduleChangeRequestDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<IEnumerable<ScheduleChangeRequestDto>>>> GetAllByRequesterEmail(
			[FromQuery] string requesterEmail,
			[FromQuery] ScheduleChangeRequestStatus? status = null)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(requesterEmail))
				{
					return BadRequest(ApiResponse<object>.Fail("RequesterEmail không được để trống"));
				}

				if (!new EmailAddressAttribute().IsValid(requesterEmail))
				{
					return BadRequest(ApiResponse<object>.Fail("RequesterEmail không đúng định dạng"));
				}

				if (status.HasValue && !Enum.IsDefined(typeof(ScheduleChangeRequestStatus), status.Value))
				{
					return BadRequest(ApiResponse<object>.Fail("Status không hợp lệ"));
				}

				var items = await _scheduleChangeRequestService.GetAllByRequesterEmailAsync(requesterEmail, status);
				return Ok(ApiResponse<IEnumerable<ScheduleChangeRequestDto>>.Ok(items));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Lấy danh sách ScheduleChangeRequest theo RequestedToEmail
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpGet("get-all-by-requested-to-email")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<ScheduleChangeRequestDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<IEnumerable<ScheduleChangeRequestDto>>>> GetAllByRequestedToEmail(
			[FromQuery] string requestedToEmail,
			[FromQuery] ScheduleChangeRequestStatus? status = null)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(requestedToEmail))
				{
					return BadRequest(ApiResponse<object>.Fail("RequestedToEmail không được để trống"));
				}

				if (!new EmailAddressAttribute().IsValid(requestedToEmail))
				{
					return BadRequest(ApiResponse<object>.Fail("RequestedToEmail không đúng định dạng"));
				}

				if (status.HasValue && !Enum.IsDefined(typeof(ScheduleChangeRequestStatus), status.Value))
				{
					return BadRequest(ApiResponse<object>.Fail("Status không hợp lệ"));
				}

				var items = await _scheduleChangeRequestService.GetAllByRequestedToEmailAsync(requestedToEmail, status);
				return Ok(ApiResponse<IEnumerable<ScheduleChangeRequestDto>>.Ok(items));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Lấy danh sách ScheduleChangeRequest theo scheduleId (có thể lọc thêm theo status)
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpGet("get-all-by-schedule-id/{scheduleId:int}")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<ScheduleChangeRequestDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<IEnumerable<ScheduleChangeRequestDto>>>> GetAllByScheduleId(
			int scheduleId,
			[FromQuery] ScheduleChangeRequestStatus? status = null)
		{
			try
			{
				if (scheduleId <= 0)
				{
					return BadRequest(ApiResponse<object>.Fail("scheduleId phải lớn hơn 0"));
				}

				if (status.HasValue && !Enum.IsDefined(typeof(ScheduleChangeRequestStatus), status.Value))
				{
					return BadRequest(ApiResponse<object>.Fail("Status không hợp lệ"));
				}

				var items = await _scheduleChangeRequestService.GetAllByScheduleIdAsync(scheduleId, status);
				return Ok(ApiResponse<IEnumerable<ScheduleChangeRequestDto>>.Ok(items));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}
	}
}

