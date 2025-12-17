using System.Linq;
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
using EduMatch.BusinessLogicLayer.Services;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BookingController : ControllerBase
	{
		private readonly IBookingService _bookingService;
		private readonly IScheduleService _scheduleService;
		private readonly EduMatchContext _context;
		private readonly CurrentUserService _currentUserService;
		private readonly INotificationService _notificationService;
		private readonly ITutorSubjectService _tutorSubjectService;
		private readonly EmailService _emailService;

		/// <summary>
		/// API Booking: lấy danh sách theo LearnerEmail/TutorId (có/không phân trang), lấy theo Id, tạo, cập nhật, cập nhật Status/PaymentStatus
		/// </summary>
		public BookingController(IBookingService bookingService, IScheduleService scheduleService, EduMatchContext context, CurrentUserService currentUserService, INotificationService notificationService, ITutorSubjectService tutorSubjectService, EmailService emailService)
		{
			_bookingService = bookingService;
			_scheduleService = scheduleService;
			_context = context;
			_currentUserService = currentUserService;
			_notificationService = notificationService;
			_tutorSubjectService = tutorSubjectService;
			_emailService = emailService;
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
		/// Thanh toán booking: khóa tiền từ ví học viên.
		/// </summary>
		[Authorize(Roles = Roles.Learner)]
		[HttpPost("{id:int}/pay")]
		[ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> PayForBooking(int id)
		{
			try
			{
				var learnerEmail = _currentUserService.Email;
				if (string.IsNullOrWhiteSpace(learnerEmail))
					return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));

				var booking = await _bookingService.PayForBookingAsync(id, learnerEmail);
				return Ok(ApiResponse<BookingDto>.Ok(booking, "Booking payment processed successfully."));
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Hủy booking bởi học viên, hoàn lại toàn bộ số tiền còn lại và hủy lịch.
		/// </summary>
		[Authorize(Roles = Roles.Learner)]
		[HttpPost("{id:int}/learner-cancel")]
		[ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> CancelByLearner(int id)
		{
			try
			{
				var learnerEmail = _currentUserService.Email;
				if (string.IsNullOrWhiteSpace(learnerEmail))
					return Unauthorized(ApiResponse<string>.Fail("User email not found in token."));
				var booking = await _bookingService.CancelByLearnerAsync(id, learnerEmail);
				return Ok(ApiResponse<BookingDto>.Ok(booking, "Hủy booking thành công và hoàn lại toàn bộ số tiền còn lại."));
			}
			catch (UnauthorizedAccessException)
			{
				return Forbid();
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Xem trước số buổi chưa học và số tiền dự kiến hoàn lại nếu hủy booking.
		/// </summary>
		[Authorize(Roles = Roles.Learner)]
		[HttpGet("{id:int}/cancel-preview")]
		[ProducesResponseType(typeof(ApiResponse<BookingCancelPreviewDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetCancelPreview(int id)
		{
			try
			{
				var preview = await _bookingService.GetCancelPreviewAsync(id);
				return Ok(ApiResponse<BookingCancelPreviewDto>.Ok(preview, "Xem trước thông tin hủy booking."));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail(ex.Message));
			}
		}

		/// <summary>
		/// Lấy danh sách Booking theo LearnerEmail có phân trang và lọc theo Status, TutorSubjectId
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin + "," + Roles.Learner)]
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
		[Authorize (Roles = Roles.BusinessAdmin + "," +  Roles.Learner)]
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
		/// Tạo Booking mới và tính phí hệ thống theo SystemFee đang hoạt động. Tự động lấy SystemFee có Id nhỏ nhất đang hoạt động.
		/// Sau khi tạo Booking thành công, lấy ID của Booking để tạo Schedule, sau đó tiến hành thanh toán luôn cho Booking đó.
		/// BookingId trong request Schedule có thể truyền bất kỳ số nào > 0, sẽ được ghi đè bằng ID của Booking vừa tạo.
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin + ","  + Roles.Learner)]
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

				// Lấy email học viên từ token để dùng cho bước thanh toán
				var learnerEmail = _currentUserService.Email;
				if (string.IsNullOrWhiteSpace(learnerEmail))
				{
					return Unauthorized(ApiResponse<object>.Fail("User email not found in token."));
				}

				// Tạo Booking
				var createdBooking = await _bookingService.CreateAsync(request.Booking);
				
				// Kiểm tra Booking đã tạo thành công
				if (createdBooking == null || createdBooking.Id <= 0)
				{
					return BadRequest(ApiResponse<object>.Fail("Tạo Booking thất bại"));
				}

				// Tạo danh sách Schedule nếu có (sử dụng ID của Booking vừa tạo)
				if (request.Schedules != null && request.Schedules.Count > 0)
				{
					foreach (var s in request.Schedules)
					{
						s.BookingId = createdBooking.Id;
					}
					await _scheduleService.CreateListAsync(request.Schedules);
				}

				// Nếu là đơn học thử (tổng tiền = 0) thì không cần thanh toán ví,
				// có thể cập nhật PaymentStatus sang Paid cho rõ ràng.
				if (createdBooking.TotalAmount <= 0)
				{
					var trialBooking = await _bookingService.UpdatePaymentStatusAsync(createdBooking.Id, PaymentStatus.Paid);
					await transaction.CommitAsync();
					
					// Gửi notification cho tutor về đơn yêu cầu dạy học mới
					var tutorSubject = await _tutorSubjectService.GetByIdFullAsync(createdBooking.TutorSubjectId);
					if (!string.IsNullOrWhiteSpace(tutorSubject?.TutorEmail))
					{
						await _notificationService.CreateNotificationAsync(
							tutorSubject.TutorEmail,
							$"Bạn có đơn yêu cầu dạy học thử với học viên: {learnerEmail} - Môn: {tutorSubject.Subject.SubjectName} - {tutorSubject.Level.Name}. Vui lòng xác nhận đơn hàng.",
							"/bookings");

					}
					
					// Gửi notification cho learner khi booking tạo thành công
					if (!string.IsNullOrWhiteSpace(learnerEmail) && tutorSubject != null)
					{
						
						await _notificationService.CreateNotificationAsync(
							learnerEmail,
							$"Bạn đã tạo đơn học thử thành công với gia sư: {tutorSubject.TutorEmail} - Môn: {tutorSubject.Subject.SubjectName} - {tutorSubject.Level.Name}. Đang chờ gia sư xác nhận.",
							"/bookings");


						// Gửi email thông báo booking tạo thành công cho learner
						try
						{
							await _emailService.SendBookingCreatedNotificationAsync(
								learnerEmail,
								tutorSubject.Subject?.SubjectName ?? "N/A",
								tutorSubject.Level?.Name ?? "N/A",
								trialBooking.TotalAmount,
								tutorSubject.TutorEmail);
						}
						catch (Exception ex)
						{
							Console.WriteLine($"[BookingController] Error sending email: {ex.Message}");
						}

					}

					
					return Ok(ApiResponse<BookingDto>.Ok(trialBooking, "Tạo Booking học thử (miễn phí) và cập nhật thanh toán thành công"));
				}

				// Thanh toán luôn cho booking vừa tạo (khóa tiền từ ví học viên)
				var paidBooking = await _bookingService.PayForBookingAsync(createdBooking.Id, learnerEmail);

				await transaction.CommitAsync();
				
				// Gửi notification cho tutor về đơn yêu cầu dạy học mới
				var tutorSubjectForPaid = await _tutorSubjectService.GetByIdFullAsync(paidBooking.TutorSubjectId);
				if (!string.IsNullOrWhiteSpace(tutorSubjectForPaid?.TutorEmail))
				{
					await _notificationService.CreateNotificationAsync(
						tutorSubjectForPaid.TutorEmail,
						$"Bạn có đơn yêu cầu dạy với học viên: {learnerEmail} - Môn: {tutorSubjectForPaid.Subject.SubjectName} - {tutorSubjectForPaid.Level.Name}. Vui lòng xác nhận đơn hàng.",
						"/bookings");
				}
				
				// Gửi notification cho learner khi booking tạo thành công
				if (!string.IsNullOrWhiteSpace(learnerEmail) && tutorSubjectForPaid != null)
				{
					await _notificationService.CreateNotificationAsync(
						learnerEmail,
						$"Bạn đã tạo đơn đặt lịch thành công với gia sư: {tutorSubjectForPaid.TutorEmail} - Môn: {tutorSubjectForPaid.Subject.SubjectName} - {tutorSubjectForPaid.Level.Name}. Đang chờ gia sư xác nhận.",
						"/bookings");
					// Gửi email thông báo booking tạo thành công cho learner

					try
					{

						await _emailService.SendBookingCreatedNotificationAsync(
							learnerEmail,
							tutorSubjectForPaid.Subject?.SubjectName ?? "N/A",
							tutorSubjectForPaid.Level?.Name ?? "N/A",
							paidBooking.TotalAmount,
							tutorSubjectForPaid.TutorEmail);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[BookingController] Error sending email: {ex.Message}");
					}
				}
				
				return Ok(ApiResponse<BookingDto>.Ok(paidBooking, "Tạo Booking, danh sách Schedule và thanh toán thành công"));
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}

		// /// <summary>
		// /// Cập nhật Booking và tính lại các giá trị liên quan khi thay đổi (TotalSessions, TutorSubjectId). Nếu thay đổi TotalSessions sẽ tính lại SystemFeeAmount
		// /// </summary>
		// [Authorize (Roles = Roles.BusinessAdmin +  "," + Roles.Learner)]
		// [HttpPut("update-booking")]
		// [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
		// [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		// public async Task<ActionResult<ApiResponse<BookingDto>>> Update([FromBody] BookingUpdateRequest request)
		// {
		// 	try
		// 	{
		// 		if (!ModelState.IsValid)
		// 		{
		// 			return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", ModelState));
		// 		}
		// 		var updated = await _bookingService.UpdateAsync(request);
		// 		return Ok(ApiResponse<BookingDto>.Ok(updated, "Cập nhật Booking thành công"));
		// 	}
		// 	catch (Exception ex)
		// 	{
		// 		return BadRequest(ApiResponse<object>.Fail(ex.Message));
		// 	}
		// }

		// /// <summary>
		// /// Cập nhật PaymentStatus của Booking theo Id
		// /// </summary>
		// [Authorize (Roles = Roles.BusinessAdmin )]
		// [HttpPut("update-payment-status/{id:int}")]
		// [ProducesResponseType(typeof(ApiResponse<BookingDto>), StatusCodes.Status200OK)]
		// [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		// public async Task<ActionResult<ApiResponse<BookingDto>>> UpdatePaymentStatus(
		// 	int id,
		// 	[FromBody] PaymentStatus paymentStatus)
		// {
		// 	try
		// 	{
		// 		if (id <= 0)
		// 		{
		// 			return BadRequest(ApiResponse<object>.Fail("Id phải lớn hơn 0"));
		// 		}
		// 		if (!Enum.IsDefined(typeof(PaymentStatus), paymentStatus))
		// 		{
		// 			return BadRequest(ApiResponse<object>.Fail("PaymentStatus không hợp lệ"));
		// 		}
		// 		var updated = await _bookingService.UpdatePaymentStatusAsync(id, paymentStatus);
		// 		return Ok(ApiResponse<BookingDto>.Ok(updated, "Cập nhật PaymentStatus thành công"));
		// 	}
		// 	catch (Exception ex)
		// 	{
		// 		return BadRequest(ApiResponse<object>.Fail(ex.Message));
		// 	}
		// }

		/// <summary>
		/// Cập nhật Status của Booking theo Id
		/// </summary>
		[Authorize (Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner )]
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
				
				// Lấy email của người đang gọi API
				var currentUserEmail = _currentUserService.Email;
				
				var updated = await _bookingService.UpdateStatusAsync(id, status);
				
				// Lấy thông tin tutor từ booking
				var tutorSubject = await _tutorSubjectService.GetByIdFullAsync(updated.TutorSubjectId);
				var tutorEmail = tutorSubject?.TutorEmail;
				
				// Xác định ai là người gọi và gửi notification cho người còn lại
				if (!string.IsNullOrWhiteSpace(currentUserEmail))
				{
					// So sánh email để xác định người gọi
					var isLearner = string.Equals(currentUserEmail, updated.LearnerEmail, StringComparison.OrdinalIgnoreCase);
					var isTutor = !string.IsNullOrWhiteSpace(tutorEmail) && 
					               string.Equals(currentUserEmail, tutorEmail, StringComparison.OrdinalIgnoreCase);
					
					if (isLearner && !string.IsNullOrWhiteSpace(tutorEmail))
					{
						// Learner gọi → gửi notification cho tutor
						await _notificationService.CreateNotificationAsync(
							tutorEmail,
							$"Trạng thái đơn hàng booking môn: {tutorSubject.Subject.SubjectName} - {tutorSubject.Level.Name}, Học viên: {updated.LearnerEmail} đã được cập nhật thành {status} bởi học viên.",
							"/bookings");
					}
					else if (isTutor && !string.IsNullOrWhiteSpace(updated.LearnerEmail))
					{
						// Tutor gọi → gửi notification cho learner
						await _notificationService.CreateNotificationAsync(
							updated.LearnerEmail,
							$"Trạng thái đơn hàng booking môn: {tutorSubject.Subject.SubjectName} - {tutorSubject.Level.Name}, Gia sư: {tutorEmail} đã được cập nhật thành {status} bởi gia sư.",
							"/bookings");
					}
				}
				
				return Ok(ApiResponse<BookingDto>.Ok(updated, "Cập nhật Status thành công"));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
		}
	}
}
