using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.SystemFee;
using EduMatch.PresentationLayer.Common;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SystemFeeController : ControllerBase
	{
		private readonly ISystemFeeService _systemFeeService;

		/// <summary>
		/// SystemFee APIs: get all (paging & no paging), create, update
		/// </summary>
		public SystemFeeController(ISystemFeeService systemFeeService)
		{
			_systemFeeService = systemFeeService;
		}

		/// <summary>
		/// Get all SystemFees with paging
		/// </summary>
		[HttpGet("get-all-paging")]
		[Authorize]
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
		/// Get all SystemFees (no paging)
		/// </summary>
		[HttpGet("get-all-no-paging")]
		[Authorize]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<SystemFeeDto>>), StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<IEnumerable<SystemFeeDto>>>> GetAllNoPaging()
		{
			var items = await _systemFeeService.GetAllNoPagingAsync();
			return Ok(ApiResponse<IEnumerable<SystemFeeDto>>.Ok(items));
		}

		/// <summary>
		/// Create a new SystemFee
		/// </summary>
		[HttpPost("create-systemfee")]
		[Authorize(Roles = "BusseAdmin")]
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
		/// Update an existing SystemFee
		/// </summary>
		[HttpPut("update-systemfee")]
		[Authorize(Roles = "BusseAdmin")]
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
