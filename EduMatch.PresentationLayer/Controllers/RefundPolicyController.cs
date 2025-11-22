using EduMatch.BusinessLogicLayer.Constants;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.RefundPolicy;
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
	/// API RefundPolicy: quản lý chính sách hoàn tiền
	/// Role: BusinessAdmin cho các thao tác tạo, cập nhật
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	public class RefundPolicyController : ControllerBase
	{
		private readonly IRefundPolicyService _refundPolicyService;

		/// <summary>
		/// API RefundPolicy: lấy tất cả (có lọc theo IsActive), lấy theo ID, tạo, cập nhật, cập nhật trạng thái
		/// </summary>
		public RefundPolicyController(IRefundPolicyService refundPolicyService)
		{
			_refundPolicyService = refundPolicyService;
		}

		/// <summary>
		/// Lấy tất cả RefundPolicy (có thể lọc theo IsActive)
		/// Role: Không yêu cầu
		/// </summary>
		[HttpGet("get-all")]
		[ProducesResponseType(typeof(ApiResponse<List<RefundPolicyDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<List<RefundPolicyDto>>>> GetAll([FromQuery] bool? isActive = null)
		{
			try
			{
				var items = await _refundPolicyService.GetAllAsync(isActive);
				return Ok(ApiResponse<List<RefundPolicyDto>>.Ok(items));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Lỗi khi lấy danh sách chính sách hoàn tiền", ex.Message));
			}
		}

		/// <summary>
		/// Lấy RefundPolicy theo ID
		/// Role: Không yêu cầu
		/// </summary>
		[HttpGet("get-by-id/{id}")]
		[ProducesResponseType(typeof(ApiResponse<RefundPolicyDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApiResponse<RefundPolicyDto>>> GetById([FromRoute] int id)
		{
			try
			{
				if (id <= 0)
					return BadRequest(ApiResponse<string>.Fail("Id phải lớn hơn 0"));

				var item = await _refundPolicyService.GetByIdAsync(id);
				if (item == null)
					return NotFound(ApiResponse<string>.Fail("Không tìm thấy chính sách hoàn tiền"));

				return Ok(ApiResponse<RefundPolicyDto>.Ok(item));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Lỗi khi lấy chính sách hoàn tiền", ex.Message));
			}
		}

		/// <summary>
		/// Tạo mới RefundPolicy
		/// Role: BusinessAdmin
		/// </summary>
		[HttpPost("create")]
		[Authorize(Roles = Roles.BusinessAdmin)]
		[ProducesResponseType(typeof(ApiResponse<RefundPolicyDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<ApiResponse<RefundPolicyDto>>> Create([FromBody] RefundPolicyCreateRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", ModelState));
				}

				var created = await _refundPolicyService.CreateAsync(request);
				return Ok(ApiResponse<RefundPolicyDto>.Ok(created, "Tạo chính sách hoàn tiền thành công"));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Lỗi khi tạo chính sách hoàn tiền", ex.Message));
			}
		}

		/// <summary>
		/// Cập nhật RefundPolicy
		/// Role: BusinessAdmin
		/// </summary>
		[HttpPut("update")]
		[Authorize(Roles = Roles.BusinessAdmin)]
		[ProducesResponseType(typeof(ApiResponse<RefundPolicyDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<ApiResponse<RefundPolicyDto>>> Update([FromBody] RefundPolicyUpdateRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", ModelState));
				}

				var updated = await _refundPolicyService.UpdateAsync(request);
				return Ok(ApiResponse<RefundPolicyDto>.Ok(updated, "Cập nhật chính sách hoàn tiền thành công"));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Lỗi khi cập nhật chính sách hoàn tiền", ex.Message));
			}
		}

		/// <summary>
		/// Cập nhật trạng thái IsActive của RefundPolicy
		/// Role: BusinessAdmin
		/// </summary>
		[HttpPut("update-is-active/{id}")]
		[Authorize(Roles = Roles.BusinessAdmin)]
		[ProducesResponseType(typeof(ApiResponse<RefundPolicyDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<ApiResponse<RefundPolicyDto>>> UpdateIsActive([FromRoute] int id, [FromQuery] bool isActive)
		{
			try
			{
				if (id <= 0)
					return BadRequest(ApiResponse<string>.Fail("Id phải lớn hơn 0"));

				var updated = await _refundPolicyService.UpdateIsActiveAsync(id, isActive);
				return Ok(ApiResponse<RefundPolicyDto>.Ok(updated, $"Cập nhật trạng thái chính sách hoàn tiền thành công: {(isActive ? "Kích hoạt" : "Vô hiệu hóa")}"));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Lỗi khi cập nhật trạng thái chính sách hoàn tiền", ex.Message));
			}
		}
	}
}

