using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Authorization;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.Schedule;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.PresentationLayer.Common;
using System.ComponentModel.DataAnnotations;
using EduMatch.BusinessLogicLayer.Constants;
using EduMatch.BusinessLogicLayer.Services;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ScheduleController : ControllerBase
	{
		private readonly IScheduleService _scheduleService;
		private readonly IBookingService _bookingService;
		private readonly IScheduleCompletionService _scheduleCompletionService;
		private readonly CurrentUserService _currentUserService;
		private readonly EduMatchContext _context;

		/// <summary>
		/// API Schedule: lấy danh sách (có/không phân trang), lấy theo Id/AvailabilityId, tạo, cập nhật, hủy theo BookingId
		/// </summary>
		public ScheduleController(
			IScheduleService scheduleService,
			IBookingService bookingService,
			IScheduleCompletionService scheduleCompletionService,
			CurrentUserService currentUserService,
			EduMatchContext context)
		{
			_scheduleService = scheduleService;
			_bookingService = bookingService;
			_scheduleCompletionService = scheduleCompletionService;
			_currentUserService = currentUserService;
			_context = context;
        }

        /// <summary>
        /// Learner confirms a schedule as finished and triggers payout immediately.
        /// </summary>
        [Authorize(Roles = Roles.Learner)]
        [HttpPost("{id:int}/finish")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<object>>> FinishSchedule(int id)
		{
			try
			{
				var updated = await _scheduleCompletionService.FinishAndPayAsync(id, _currentUserService.Email, adminAction: false);
				return Ok(ApiResponse<object>.Ok(new { updated }));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// Learner or tutor cancels the schedule completion (no payout).
        /// </summary>
        [Authorize]
        [HttpPost("{id:int}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> CancelScheduleCompletion(int id)
        {
            try
            {
                var updated = await _scheduleCompletionService.CancelAsync(id);
                return Ok(ApiResponse<object>.Ok(new { updated }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// Mark schedule as reported/on-hold (ties to an existing report).
        /// </summary>
        [Authorize(Roles = Roles.Learner)]
        [HttpPost("{id:int}/report/{reportId:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> ReportSchedule(int id, int reportId)
        {
            try
            {
				var updated = await _scheduleCompletionService.MarkReportedAsync(id, reportId, _currentUserService.Email);
				return Ok(ApiResponse<object>.Ok(new { updated }));
			}
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// Admin finishes a schedule (bypasses learner ownership) and triggers payout.
        /// </summary>
        [Authorize(Roles = Roles.SystemAdmin)]
        [HttpPost("{id:int}/admin/finish")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> AdminFinishSchedule(int id)
        {
            try
            {
                var updated = await _scheduleCompletionService.FinishAndPayAsync(id, null, adminAction: true);
                return Ok(ApiResponse<object>.Ok(new { updated }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// Admin cancels a schedule completion (no payout).
        /// </summary>
        [Authorize(Roles = Roles.SystemAdmin)]
        [HttpPost("{id:int}/admin/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> AdminCancelSchedule(int id)
        {
            try
            {
                var updated = await _scheduleCompletionService.CancelAsync(id, null, adminAction: true);
                return Ok(ApiResponse<object>.Ok(new { updated }));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

		/// <summary>
		/// Admin resolution: release payout or cancel after review.
		/// </summary>
		[Authorize(Roles = Roles.SystemAdmin)]
		[HttpPost("{id:int}/resolve-report")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<object>>> ResolveReport(int id, [FromQuery] bool releaseToTutor = true)
		{
			try
			{
				var updated = await _scheduleCompletionService.ResolveReportAsync(id, releaseToTutor);
				return Ok(ApiResponse<object>.Ok(new { updated }));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Lấy Schedule theo Id
		/// </summary>
		[Authorize]
		[HttpGet("get-by-id/{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<ScheduleDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<ScheduleDto>>> GetById(int id)
		{
			try
			{
				var item = await _scheduleService.GetByIdAsync(id);
				if (item == null)
					return NotFound(ApiResponse<object>.Fail("Không tìm thấy Schedule"));
				return Ok(ApiResponse<ScheduleDto>.Ok(item));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Lấy Schedule theo AvailabilityId
		/// </summary>
		[Authorize]
		[HttpGet("get-by-availability-id/{availabilitiId:int}")]
		[ProducesResponseType(typeof(ApiResponse<ScheduleDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<ScheduleDto>>> GetByAvailabilityId(int availabilitiId)
		{
			try
			{
				var item = await _scheduleService.GetByAvailabilityIdAsync(availabilitiId);
				if (item == null)
					return NotFound(ApiResponse<object>.Fail("Không tìm thấy Schedule cho AvailabilityId"));
				return Ok(ApiResponse<ScheduleDto>.Ok(item));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Lấy danh sách Schedule theo BookingId và Status có phân trang
		/// </summary>
		[Authorize]
		[HttpGet("get-all-paging")]
		[ProducesResponseType(typeof(ApiResponse<PagedResult<ScheduleDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<PagedResult<ScheduleDto>>>> GetAllPaging(
			[FromQuery] int bookingId,
			[FromQuery] ScheduleStatus? status = null,
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10)
		{
			try
			{
				if (bookingId <= 0)
				{
					return BadRequest(ApiResponse<object>.Fail("BookingId phải lớn hơn 0"));
				}
				if (status.HasValue && !Enum.IsDefined(typeof(ScheduleStatus), status.Value))
				{
					return BadRequest(ApiResponse<object>.Fail("Status không hợp lệ"));
				}
                var items = await _scheduleService.GetAllByBookingIdAndStatusAsync(bookingId, status, page, pageSize);
                var total = await _scheduleService.CountByBookingIdAndStatusAsync(bookingId, status);
                var result = new PagedResult<ScheduleDto>
                {
                    Items = items,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalCount = total
                };
                return Ok(ApiResponse<PagedResult<ScheduleDto>>.Ok(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        /// <summary>
        /// Get attendance summary counts for a booking (studied / not studied yet / cancelled)
        /// </summary>
        [Authorize]
        [HttpGet("{bookingId:int}/attendance-summary")]
        [ProducesResponseType(typeof(ApiResponse<ScheduleAttendanceSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ScheduleAttendanceSummaryDto>>> GetAttendanceSummary(int bookingId)
        {
            try
            {
                var summary = await _scheduleService.GetAttendanceSummaryByBookingAsync(bookingId);
                return Ok(ApiResponse<ScheduleAttendanceSummaryDto>.Ok(summary));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

		/// <summary>
		/// Lấy danh sách Schedule theo BookingId và Status (không phân trang)
		/// </summary>
		[Authorize]
		[HttpGet("get-all-no-paging")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<ScheduleDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<IEnumerable<ScheduleDto>>>> GetAllNoPaging(
			[FromQuery] int bookingId,
			[FromQuery] ScheduleStatus? status = null)
		{
			try
			{
				if (bookingId <= 0)
				{
					return BadRequest(ApiResponse<object>.Fail("BookingId phải lớn hơn 0"));
				}
				if (status.HasValue && !Enum.IsDefined(typeof(ScheduleStatus), status.Value))
				{
					return BadRequest(ApiResponse<object>.Fail("Status không hợp lệ"));
				}
				var items = await _scheduleService.GetAllByBookingIdAndStatusNoPagingAsync(bookingId, status);
				return Ok(ApiResponse<IEnumerable<ScheduleDto>>.Ok(items));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Tạo Schedule mới và cập nhật TutorAvailability status sang Booked. 
		/// Nếu là online thì tạo MeetingSession tự động
		/// Optional vì booking là tạo luôn booking và schedule và meeting session(nếu là online) cùng lúc
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin)]
		[HttpPost("create-schedule")]
		[ProducesResponseType(typeof(ApiResponse<ScheduleDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<ScheduleDto>>> Create([FromBody] ScheduleCreateRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", ModelState));
				}
				var created = await _scheduleService.CreateAsync(request);
				return Ok(ApiResponse<ScheduleDto>.Ok(created, "Tạo Schedule thành công"));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Tạo danh sách Schedule cho một Booking. Tổng số Schedule sau khi tạo phải bằng TotalSessions của Booking
		/// Optional vì booking là tạo luôn booking và schedule và meeting session(nếu là online) cùng lúc
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin + "," + Roles.Tutor)]
		[HttpPost("create-schedule-list")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<ScheduleDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<IEnumerable<ScheduleDto>>>> CreateList([FromBody] List<ScheduleCreateRequest> requests)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", ModelState));
			}

			await using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var created = await _scheduleService.CreateListAsync(requests);
				await transaction.CommitAsync();
				return Ok(ApiResponse<IEnumerable<ScheduleDto>>.Ok(created, "Tạo danh sách Schedule thành công"));
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Cập nhật Schedule. Nếu AvailabilitiId thay đổi thì cập nhật MeetingSession và trạng thái Availability
		/// Mặc định là online, nếu không truyền IsOnline thì coi như online
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpPut("update-schedule")]
		[ProducesResponseType(typeof(ApiResponse<ScheduleDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<ScheduleDto>>> Update([FromBody] ScheduleUpdateRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", ModelState));
				}
				if (request.Status.HasValue && !Enum.IsDefined(typeof(ScheduleStatus), request.Status.Value))
				{
					return BadRequest(ApiResponse<object>.Fail("Status không hợp lệ"));
				}
				var updated = await _scheduleService.UpdateAsync(request);
				return Ok(ApiResponse<ScheduleDto>.Ok(updated, "Cập nhật Schedule thành công"));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Hủy toàn bộ Schedule theo BookingId: set Status=Cancelled, xóa MeetingSession (bao gồm Google Calendar event), trả Availability về Available
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpPost("cancel-all-by-booking/{bookingId:int}")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<ScheduleDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<IEnumerable<ScheduleDto>>>> CancelAllByBooking(int bookingId)
		{
			try
			{
				if (bookingId <= 0)
				{
					return BadRequest(ApiResponse<object>.Fail("BookingId phải lớn hơn 0"));
				}
				// Kiểm tra Booking tồn tại qua service
				var booking = await _bookingService.GetByIdAsync(bookingId);
				if (booking == null)
				{
					return NotFound(ApiResponse<object>.Fail("Không tìm thấy Booking"));
				}

				var cancelled = await _scheduleService.CancelAllByBookingAsync(bookingId);
				return Ok(ApiResponse<IEnumerable<ScheduleDto>>.Ok(cancelled, "Hủy toàn bộ Schedule thành công"));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Lấy tất cả lịch học theo LearnerEmail; có thể lọc theo khoảng thời gian và Status (StartDate <= EndDate)
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpGet("get-all-by-learner-email")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<ScheduleDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<IEnumerable<ScheduleDto>>>> GetAllByLearnerEmail(
			[FromQuery] string learnerEmail,
			[FromQuery] DateTime? startDate = null,
			[FromQuery] DateTime? endDate = null,
			[FromQuery] ScheduleStatus? status = null)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(learnerEmail))
				{
					return BadRequest(ApiResponse<object>.Fail("LearnerEmail không được để trống"));
				}
				if (!new EmailAddressAttribute().IsValid(learnerEmail))
				{
					return BadRequest(ApiResponse<object>.Fail("LearnerEmail không đúng định dạng"));
				}

				if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
				{
					return BadRequest(ApiResponse<object>.Fail("StartDate không được lớn hơn EndDate"));
				}
				if (status.HasValue && !Enum.IsDefined(typeof(ScheduleStatus), status.Value))
				{
					return BadRequest(ApiResponse<object>.Fail("Status không hợp lệ"));
				}

				var items = await _scheduleService.GetAllByLearnerEmailAsync(learnerEmail, startDate, endDate, status);
				return Ok(ApiResponse<IEnumerable<ScheduleDto>>.Ok(items));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Lấy tất cả lịch dạy theo TutorEmail Optional StartDate và EndDate (có thể lọc theo khoảng thời gian StartDate <= EndDate)
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpGet("get-all-by-tutor-email")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<ScheduleDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<IEnumerable<ScheduleDto>>>> GetAllByTutorEmail(
			[FromQuery] string tutorEmail,
			[FromQuery] DateTime? startDate = null,
			[FromQuery] DateTime? endDate = null,
			[FromQuery] ScheduleStatus? status = null)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(tutorEmail))
				{
					return BadRequest(ApiResponse<object>.Fail("TutorEmail không được để trống"));
				}
				if (!new EmailAddressAttribute().IsValid(tutorEmail))
				{
					return BadRequest(ApiResponse<object>.Fail("TutorEmail không đúng định dạng"));
				}

				if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
				{
					return BadRequest(ApiResponse<object>.Fail("StartDate không được lớn hơn EndDate"));
				}

				if (status.HasValue && !Enum.IsDefined(typeof(ScheduleStatus), status.Value))
				{
					return BadRequest(ApiResponse<object>.Fail("Status không hợp lệ"));
				}

				var items = await _scheduleService.GetAllByTutorEmailAsync(tutorEmail, startDate, endDate, status);
				return Ok(ApiResponse<IEnumerable<ScheduleDto>>.Ok(items));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Lấy một số buổi dạy của Tutor theo email và status, mặc định lấy 1  và status là Upcoming
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpGet("get-by-tutor-email-and-status")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<ScheduleDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<IEnumerable<ScheduleDto>>>> GetByTutorEmailAndStatus(
			[FromQuery] string tutorEmail,
			[FromQuery] int bookingId,
			[FromQuery] ScheduleStatus status = ScheduleStatus.Upcoming,
			[FromQuery] int take = 1)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(tutorEmail))
				{
					return BadRequest(ApiResponse<object>.Fail("TutorEmail không được để trống"));
				}
				if (!new EmailAddressAttribute().IsValid(tutorEmail))
				{
					return BadRequest(ApiResponse<object>.Fail("TutorEmail không đúng định dạng"));
				}

				if (bookingId <= 0)
				{
					return BadRequest(ApiResponse<object>.Fail("BookingId phải lớn hơn 0"));
				}

				if (take <= 0)
				{
					take = 1;
				}

				if (!Enum.IsDefined(typeof(ScheduleStatus), status))
				{
					return BadRequest(ApiResponse<object>.Fail("Status không hợp lệ"));
				}

				var items = await _scheduleService.GetByTutorEmailAndStatusAsync(tutorEmail, status, bookingId, take);
				return Ok(ApiResponse<IEnumerable<ScheduleDto>>.Ok(items));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Cập nhật Status của Schedule (chỉ cho phép update tiến dần)
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor)]
		[HttpPut("update-status/{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<ScheduleDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<ScheduleDto>>> UpdateStatus(
			int id,
			[FromQuery] ScheduleStatus status)
		{
			try
			{
				if (id <= 0)
				{
					return BadRequest(ApiResponse<object>.Fail("Id phải lớn hơn 0"));
				}

				if (!Enum.IsDefined(typeof(ScheduleStatus), status))
				{
					return BadRequest(ApiResponse<object>.Fail("Status không hợp lệ"));
				}

				var updated = await _scheduleService.UpdateStatusAsync(id, status);
				return Ok(ApiResponse<ScheduleDto>.Ok(updated, "Cập nhật Status thành công"));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}


	}
}
