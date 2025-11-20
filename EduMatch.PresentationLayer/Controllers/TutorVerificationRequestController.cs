using EduMatch.BusinessLogicLayer.Constants;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
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
	/// API TutorVerificationRequest: quản lý yêu cầu xác minh gia sư
	/// Role: BusinessAdmin cho các thao tác quản lý
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	public class TutorVerificationRequestController : ControllerBase
	{
		private readonly ITutorVerificationRequestService _tutorVerificationRequestService;

		/// <summary>
		/// API TutorVerificationRequest: lấy tất cả (có lọc theo Status), lấy theo Email/TutorId, lấy theo ID
		/// </summary>
		public TutorVerificationRequestController(ITutorVerificationRequestService tutorVerificationRequestService)
		{
			_tutorVerificationRequestService = tutorVerificationRequestService;
		}

		/// <summary>
		/// Lấy tất cả TutorVerificationRequest (có thể lọc theo Status)
		/// Role: BusinessAdmin
		/// </summary>
		[HttpGet("get-all")]
		[Authorize(Roles = Roles.BusinessAdmin)]
		[ProducesResponseType(typeof(ApiResponse<List<TutorVerificationRequestDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<ApiResponse<List<TutorVerificationRequestDto>>>> GetAll([FromQuery] TutorVerificationRequestStatus? status = null)
		{
			try
			{
				var items = await _tutorVerificationRequestService.GetAllAsync(status);
				return Ok(ApiResponse<List<TutorVerificationRequestDto>>.Ok(items));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Lỗi khi lấy danh sách yêu cầu xác minh gia sư", ex.Message));
			}
		}

		/// <summary>
		/// Lấy tất cả TutorVerificationRequest theo Email hoặc TutorId (có thể lọc theo Status)
		/// Role: BusinessAdmin, Tutor, Learner
		/// </summary>
		[HttpGet("get-by-email-or-tutor-id")]
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[ProducesResponseType(typeof(ApiResponse<List<TutorVerificationRequestDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<ApiResponse<List<TutorVerificationRequestDto>>>> GetByEmailOrTutorId(
			[FromQuery] string? email = null,
			[FromQuery] int? tutorId = null,
			[FromQuery] TutorVerificationRequestStatus? status = null)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(email) && !tutorId.HasValue)
					return BadRequest(ApiResponse<string>.Fail("Phải cung cấp ít nhất một trong hai: email hoặc tutorId"));

				if (tutorId.HasValue && tutorId.Value <= 0)
					return BadRequest(ApiResponse<string>.Fail("TutorId phải lớn hơn 0"));

				var items = await _tutorVerificationRequestService.GetAllByEmailOrTutorIdAsync(email, tutorId, status);
				return Ok(ApiResponse<List<TutorVerificationRequestDto>>.Ok(items));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Lỗi khi lấy danh sách yêu cầu xác minh gia sư", ex.Message));
			}
		}

		/// <summary>
		/// Lấy TutorVerificationRequest theo ID
		/// Role: BusinessAdmin
		/// </summary>
		[HttpGet("get-by-id/{id}")]
		[Authorize(Roles = Roles.BusinessAdmin)]
		[ProducesResponseType(typeof(ApiResponse<TutorVerificationRequestDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<ApiResponse<TutorVerificationRequestDto>>> GetById([FromRoute] int id)
		{
			try
			{
				if (id <= 0)
					return BadRequest(ApiResponse<string>.Fail("Id phải lớn hơn 0"));

				var item = await _tutorVerificationRequestService.GetByIdAsync(id);
				if (item == null)
					return NotFound(ApiResponse<string>.Fail("Không tìm thấy yêu cầu xác minh gia sư"));

				return Ok(ApiResponse<TutorVerificationRequestDto>.Ok(item));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Lỗi khi lấy yêu cầu xác minh gia sư", ex.Message));
			}
		}
	}
}

