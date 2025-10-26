using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TimeSlotsController : ControllerBase
	{
		private readonly ITimeSlotService _timeSlotService;
		public TimeSlotsController(ITimeSlotService timeSlotService)
		{
			_timeSlotService = timeSlotService;
		}

		/// <summary>
		/// Lấy danh sách tất cả các khung giờ học có sẵn trong hệ thống
		/// </summary>
		[HttpGet("get-all-time-slots")]
		[ProducesResponseType(typeof(ApiResponse<List<TimeSlotDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetAllTimeSlot()
		{
			try
			{
				var data = await _timeSlotService.GetAllAsync();

				if (data == null || !data.Any())
				{
					return Ok(ApiResponse<List<TimeSlotDto>>.Ok(
						new List<TimeSlotDto>(),
						"No time slots found in the system."
					));
				}

				return Ok(ApiResponse<List<TimeSlotDto>>.Ok(
					data.ToList(),
					"Successfully retrieved the list of time slots."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while retrieving the list of time slots.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}



	}
}
