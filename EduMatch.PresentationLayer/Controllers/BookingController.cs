using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.Booking;
using EduMatch.BusinessLogicLayer.Requests.Schedule;
using EduMatch.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using EduMatch.BusinessLogicLayer.Constants;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BookingController : ControllerBase
	{
		private readonly IBookingService _bookingService;
		private readonly IScheduleService _scheduleService;
		private readonly EduMatchContext _context;

		/// <summary>
		/// API Booking: lấy danh sách theo LearnerEmail/TutorId (có/không phân trang), lấy theo Id, tạo, cập nhật, cập nhật Status/PaymentStatus
		/// </summary>
		public BookingController(IBookingService bookingService, IScheduleService scheduleService, EduMatchContext context)
		{
			_bookingService = bookingService;
			_scheduleService = scheduleService;
			_context = context;
		}

		/// <summary>
		/// Lấy Booking theo Id
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpGet("get-by-id/{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<BookingDto>>> GetById(int id)
		{
			try
			{
				var item = await _bookingService.GetByIdAsync(id);
				if (item == null)
					return NotFound(ApiResponse<object>.Fail("Không tìm thấy Booking"));
				return Ok(ApiResponse<BookingDto>.Ok(item));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Lấy danh sách Booking theo LearnerEmail có phân trang và lọc theo Status, TutorSubjectId
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpGet("get-all-by-learner-email-paging")]
		[ProducesResponseType(typeof(ApiResponse<PagedResult<BookingDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<PagedResult<BookingDto>>>> GetAllByLearnerEmailPaging(
			[FromQuery] string email,
			[FromQuery] BookingStatus? status = null,
			[FromQuery] int? tutorSubjectId = null,
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(email))
				{
					return BadRequest(ApiResponse<object>.Fail("Email không được để trống"));
				}
				if (!new EmailAddressAttribute().IsValid(email))
				{
					return BadRequest(ApiResponse<object>.Fail("Email không đúng định dạng"));
				}
				if (status.HasValue && !Enum.IsDefined(typeof(BookingStatus), status.Value))
				{
					return BadRequest(ApiResponse<object>.Fail("Status không hợp lệ"));
				}
				var items = await _bookingService.GetAllByLearnerEmailAsync(email, status, tutorSubjectId, page, pageSize);
				var total = await _bookingService.CountByLearnerEmailAsync(email, status, tutorSubjectId);
				var result = new PagedResult<BookingDto>
				{
					Items = items,
					PageNumber = page,
					PageSize = pageSize,
					TotalCount = total
				};
				return Ok(ApiResponse<PagedResult<BookingDto>>.Ok(result));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Lấy danh sách Booking theo LearnerEmail (không phân trang) và lọc theo Status, TutorSubjectId
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpGet("get-all-by-learner-email-no-paging")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<BookingDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<IEnumerable<BookingDto>>>> GetAllByLearnerEmailNoPaging(
			[FromQuery] string email,
			[FromQuery] BookingStatus? status = null,
			[FromQuery] int? tutorSubjectId = null)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(email))
				{
					return BadRequest(ApiResponse<object>.Fail("Email không được để trống"));
				}
				if (!new EmailAddressAttribute().IsValid(email))
				{
					return BadRequest(ApiResponse<object>.Fail("Email không đúng định dạng"));
				}
				if (status.HasValue && !Enum.IsDefined(typeof(BookingStatus), status.Value))
				{
					return BadRequest(ApiResponse<object>.Fail("Status không hợp lệ"));
				}
				var items = await _bookingService.GetAllByLearnerEmailNoPagingAsync(email, status, tutorSubjectId);
				return Ok(ApiResponse<IEnumerable<BookingDto>>.Ok(items));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Lấy danh sách Booking theo TutorId có phân trang và lọc theo Status, TutorSubjectId
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin + "," + Roles.Tutor)]
		[HttpGet("get-all-by-tutor-id-paging")]
		[ProducesResponseType(typeof(ApiResponse<PagedResult<BookingDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<PagedResult<BookingDto>>>> GetAllByTutorIdPaging(
			[FromQuery] int tutorId,
			[FromQuery] BookingStatus? status = null,
			[FromQuery] int? tutorSubjectId = null,
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10)
		{
			try
			{
				if (tutorId <= 0)
				{
					return BadRequest(ApiResponse<object>.Fail("TutorId phải lớn hơn 0"));
				}
				if (status.HasValue && !Enum.IsDefined(typeof(BookingStatus), status.Value))
				{
					return BadRequest(ApiResponse<object>.Fail("Status không hợp lệ"));
				}
				var items = await _bookingService.GetAllByTutorIdAsync(tutorId, status, tutorSubjectId, page, pageSize);
				var total = await _bookingService.CountByTutorIdAsync(tutorId, status, tutorSubjectId);
				var result = new PagedResult<BookingDto>
				{
					Items = items,
					PageNumber = page,
					PageSize = pageSize,
					TotalCount = total
				};
				return Ok(ApiResponse<PagedResult<BookingDto>>.Ok(result));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Lấy danh sách Booking theo TutorId (không phân trang) và lọc theo Status, TutorSubjectId
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin + "," + Roles.Tutor)]
		[HttpGet("get-all-by-tutor-id-no-paging")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<BookingDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<IEnumerable<BookingDto>>>> GetAllByTutorIdNoPaging(
			[FromQuery] int tutorId,
			[FromQuery] BookingStatus? status = null,
			[FromQuery] int? tutorSubjectId = null)
		{
			try
			{
				if (tutorId <= 0)
				{
					return BadRequest(ApiResponse<object>.Fail("TutorId phải lớn hơn 0"));
				}
				if (status.HasValue && !Enum.IsDefined(typeof(BookingStatus), status.Value))
				{
					return BadRequest(ApiResponse<object>.Fail("Status không hợp lệ"));
				}
				var items = await _bookingService.GetAllByTutorIdNoPagingAsync(tutorId, status, tutorSubjectId);
				return Ok(ApiResponse<IEnumerable<BookingDto>>.Ok(items));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Tạo Booking mới và tính phí hệ thống theo SystemFee đang hoạt động. Tự động lấy SystemFee có Id nhỏ nhất đang hoạt động
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpPost("create-booking")]
		[ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<BookingDto>>> Create([FromBody] BookingWithSchedulesCreateRequest request)
		{
			await using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", ModelState));
				}

				// Tạo Booking
				var createdBooking = await _bookingService.CreateAsync(request.Booking);

				// Tạo danh sách Schedule nếu có
				if (request.Schedules != null && request.Schedules.Count > 0)
				{
					foreach (var s in request.Schedules)
					{
						s.BookingId = createdBooking.Id;
					}
					await _scheduleService.CreateListAsync(request.Schedules);
				}

				await transaction.CommitAsync();
				return Ok(ApiResponse<BookingDto>.Ok(createdBooking, "Tạo Booking và danh sách Schedule thành công"));
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Cập nhật Booking và tính lại các giá trị liên quan khi thay đổi (TotalSessions, TutorSubjectId). Nếu thay đổi TotalSessions sẽ tính lại SystemFeeAmount
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpPut("update-booking")]
		[ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<BookingDto>>> Update([FromBody] BookingUpdateRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", ModelState));
				}
				var updated = await _bookingService.UpdateAsync(request);
				return Ok(ApiResponse<BookingDto>.Ok(updated, "Cập nhật Booking thành công"));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Cập nhật PaymentStatus của Booking theo Id
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin )]
		[HttpPut("update-payment-status/{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<BookingDto>>> UpdatePaymentStatus(
			int id,
			[FromBody] PaymentStatus paymentStatus)
		{
			try
			{
				if (id <= 0)
				{
					return BadRequest(ApiResponse<object>.Fail("Id phải lớn hơn 0"));
				}
				if (!Enum.IsDefined(typeof(PaymentStatus), paymentStatus))
				{
					return BadRequest(ApiResponse<object>.Fail("PaymentStatus không hợp lệ"));
				}
				var updated = await _bookingService.UpdatePaymentStatusAsync(id, paymentStatus);
				return Ok(ApiResponse<BookingDto>.Ok(updated, "Cập nhật PaymentStatus thành công"));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Cập nhật Status của Booking theo Id
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin + "," + Roles.Tutor)]
		[HttpPut("update-status/{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<BookingDto>>> UpdateStatus(
			int id,
			[FromBody] BookingStatus status)
		{
			try
			{
				if (id <= 0)
				{
					return BadRequest(ApiResponse<object>.Fail("Id phải lớn hơn 0"));
				}
				if (!Enum.IsDefined(typeof(BookingStatus), status))
				{
					return BadRequest(ApiResponse<object>.Fail("Status không hợp lệ"));
				}
				var updated = await _bookingService.UpdateStatusAsync(id, status);
				return Ok(ApiResponse<BookingDto>.Ok(updated, "Cập nhật Status thành công"));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}
	}
}
