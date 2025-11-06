using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MeetingSessionsController : ControllerBase
	{
		private readonly IMeetingSessionService _meetingSessionService;

		/// <summary>
		/// API MeetingSession: lấy theo Id và theo ScheduleId
		/// </summary>
		public MeetingSessionsController(IMeetingSessionService meetingSessionService)
		{
			_meetingSessionService = meetingSessionService;
		}

		/// <summary>
		/// Lấy MeetingSession theo Id
		/// </summary>
		[Authorize]
		[HttpGet("get-by-id/{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<MeetingSessionDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApiResponse<MeetingSessionDto>>> GetById(int id)
		{
			var item = await _meetingSessionService.GetByIdAsync(id);
			if (item == null)
				return NotFound(ApiResponse<object>.Fail("Không tìm thấy MeetingSession"));
			return Ok(ApiResponse<MeetingSessionDto>.Ok(item));
		}

		/// <summary>
		/// Lấy MeetingSession theo ScheduleId
		/// </summary>
		[Authorize]
		[HttpGet("get-by-schedule-id/{scheduleId:int}")]
		[ProducesResponseType(typeof(ApiResponse<MeetingSessionDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApiResponse<MeetingSessionDto>>> GetByScheduleIdAsync(int scheduleId)
		{
			var item = await _meetingSessionService.GetByScheduleIdAsync(scheduleId);
			if (item == null)
				return NotFound(ApiResponse<object>.Fail("Không tìm thấy MeetingSession cho ScheduleId"));
			return Ok(ApiResponse<MeetingSessionDto>.Ok(item));
		}
	}
}
