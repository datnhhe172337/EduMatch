using EduMatch.BusinessLogicLayer.Constants;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.SystemFee;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SystemFeeController : ControllerBase
	{
		private readonly ISystemFeeService _systemFeeService;

		/// <summary>
		/// API SystemFee: lấy tất cả (có/không phân trang), tạo, cập nhật
		/// </summary>
		public SystemFeeController(ISystemFeeService systemFeeService)
		{
			_systemFeeService = systemFeeService;
		}

		/// <summary>
		/// Lấy tất cả SystemFee có phân trang
		/// </summary>
		[HttpGet("get-all-paging")]
		[ProducesResponseType(typeof(ApiResponse<PagedResult<SystemFeeDto>>), StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<PagedResult<SystemFeeDto>>>> GetAllPaging([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
		{
			var items = await _systemFeeService.GetAllAsync(page, pageSize);
			var total = await _systemFeeService.CountAsync();
			var result = new PagedResult<SystemFeeDto>
			{
				Items = items,
				PageNumber = page,
				PageSize = pageSize,
				TotalCount = total
			};
			return Ok(ApiResponse<PagedResult<SystemFeeDto>>.Ok(result));
		}

		/// <summary>
		/// Lấy tất cả SystemFee (không phân trang)
		/// </summary>
		[HttpGet("get-all-no-paging")]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<SystemFeeDto>>), StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<IEnumerable<SystemFeeDto>>>> GetAllNoPaging()
		{
			var items = await _systemFeeService.GetAllNoPagingAsync();
			return Ok(ApiResponse<IEnumerable<SystemFeeDto>>.Ok(items));
		}

		/// <summary>
		/// Tạo mới SystemFee
		/// </summary>
		[HttpPost("create-systemfee")]
		[Authorize(Roles = Roles.BusinessAdmin)]
		[ProducesResponseType(typeof(ApiResponse<SystemFeeDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<SystemFeeDto>>> Create([FromBody] SystemFeeCreateRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", ModelState));
			}
			var created = await _systemFeeService.CreateAsync(request);
			return Ok(ApiResponse<SystemFeeDto>.Ok(created, "Tạo thành công"));
		}

		/// <summary>
		/// Cập nhật SystemFee
		/// </summary>
		[HttpPut("update-systemfee")]
		[Authorize(Roles = Roles.BusinessAdmin)]
		[ProducesResponseType(typeof(ApiResponse<SystemFeeDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<SystemFeeDto>>> Update([FromBody] SystemFeeUpdateRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", ModelState));
			}
			var updated = await _systemFeeService.UpdateAsync(request);
			return Ok(ApiResponse<SystemFeeDto>.Ok(updated, "Cập nhật thành công"));
		}
	}
}
