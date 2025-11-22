using EduMatch.BusinessLogicLayer.Constants;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.BookingRefundRequest;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.PresentationLayer.Controllers
{
	/// <summary>
	/// API BookingRefundRequest: quản lý yêu cầu hoàn tiền đặt chỗ
	/// Role: BusinessAdmin cho các thao tác quản lý, Tutor và Learner cho các thao tác liên quan
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	public class BookingRefundRequestController : ControllerBase
	{
		private readonly IBookingRefundRequestService _bookingRefundRequestService;

		/// <summary>
		/// API BookingRefundRequest: lấy tất cả (có lọc theo Status), lấy theo Email, lấy theo ID, tạo, cập nhật trạng thái
		/// </summary>
		public BookingRefundRequestController(IBookingRefundRequestService bookingRefundRequestService)
		{
			_bookingRefundRequestService = bookingRefundRequestService;
		}

		/// <summary>
		/// Lấy tất cả BookingRefundRequest (có thể lọc theo Status)
		/// Role: BusinessAdmin
		/// </summary>
		[HttpGet("get-all")]
		[Authorize(Roles = Roles.BusinessAdmin)]
		[ProducesResponseType(typeof(ApiResponse<List<BookingRefundRequestDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<ApiResponse<List<BookingRefundRequestDto>>>> GetAll([FromQuery] BookingRefundRequestStatus? status = null)
		{
			try
			{
				var items = await _bookingRefundRequestService.GetAllAsync(status);
				return Ok(ApiResponse<List<BookingRefundRequestDto>>.Ok(items));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Lỗi khi lấy danh sách yêu cầu hoàn tiền", ex.Message));
			}
		}

		/// <summary>
		/// Lấy tất cả BookingRefundRequest theo Email (có thể lọc theo Status)
		/// Role: BusinessAdmin, Tutor
		/// </summary>
		[HttpGet("get-all-by-email")]
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Learner)]
		[ProducesResponseType(typeof(ApiResponse<List<BookingRefundRequestDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<ApiResponse<List<BookingRefundRequestDto>>>> GetAllByEmail(
			[FromQuery] string learnerEmail,
			[FromQuery] BookingRefundRequestStatus? status = null)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(learnerEmail))
					return BadRequest(ApiResponse<string>.Fail("LearnerEmail không được để trống"));

				var items = await _bookingRefundRequestService.GetAllByEmailAsync(learnerEmail, status);
				return Ok(ApiResponse<List<BookingRefundRequestDto>>.Ok(items));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Lỗi khi lấy danh sách yêu cầu hoàn tiền theo email", ex.Message));
			}
		}

		/// <summary>
		/// Lấy BookingRefundRequest theo ID
		/// Role: Authorize (tất cả role đã đăng nhập)
		/// </summary>
		[HttpGet("get-by-id/{id}")]
		[Authorize]
		[ProducesResponseType(typeof(ApiResponse<BookingRefundRequestDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<ApiResponse<BookingRefundRequestDto>>> GetById([FromRoute] int id)
		{
			try
			{
				if (id <= 0)
					return BadRequest(ApiResponse<string>.Fail("Id phải lớn hơn 0"));

				var item = await _bookingRefundRequestService.GetByIdAsync(id);
				if (item == null)
					return NotFound(ApiResponse<string>.Fail("Không tìm thấy yêu cầu hoàn tiền"));

				return Ok(ApiResponse<BookingRefundRequestDto>.Ok(item));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Lỗi khi lấy yêu cầu hoàn tiền", ex.Message));
			}
		}

		/// <summary>
		/// Tạo mới BookingRefundRequest
		/// Role: Learner
		/// </summary>
		[HttpPost("create")]
		[Authorize(Roles = Roles.Learner)]
		[ProducesResponseType(typeof(ApiResponse<BookingRefundRequestDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<ApiResponse<BookingRefundRequestDto>>> Create([FromBody] BookingRefundRequestCreateRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", ModelState));
				}

				var created = await _bookingRefundRequestService.CreateAsync(request);
				return Ok(ApiResponse<BookingRefundRequestDto>.Ok(created, "Tạo yêu cầu hoàn tiền thành công"));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Lỗi khi tạo yêu cầu hoàn tiền", ex.Message));
			}
		}

		/// <summary>
		/// Cập nhật trạng thái BookingRefundRequest
		/// Role: BusinessAdmin
		/// </summary>
		[HttpPut("update-status/{id}")]
		[Authorize(Roles = Roles.BusinessAdmin)]
		[ProducesResponseType(typeof(ApiResponse<BookingRefundRequestDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<ApiResponse<BookingRefundRequestDto>>> UpdateStatus(
			[FromRoute] int id,
			[FromQuery] BookingRefundRequestStatus status)
		{
			try
			{
				if (id <= 0)
					return BadRequest(ApiResponse<string>.Fail("Id phải lớn hơn 0"));

				var updated = await _bookingRefundRequestService.UpdateStatusAsync(id, status);
				return Ok(ApiResponse<BookingRefundRequestDto>.Ok(updated, $"Cập nhật trạng thái yêu cầu hoàn tiền thành công: {status}"));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Lỗi khi cập nhật trạng thái yêu cầu hoàn tiền", ex.Message));
			}
		}
	}
}

