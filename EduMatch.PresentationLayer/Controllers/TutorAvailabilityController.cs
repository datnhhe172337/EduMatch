using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.TutorAvailability;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using EduMatch.BusinessLogicLayer.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.PresentationLayer.Controllers
{
	/// <summary>
	/// Controller quản lý lịch trình của gia sư
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	public class TutorAvailabilityController : ControllerBase
	{
		private readonly ITutorAvailabilityService _tutorAvailabilityService;
		private readonly EduMatchContext _context;

		public TutorAvailabilityController(ITutorAvailabilityService tutorAvailabilityService, EduMatchContext context)
		{
			_tutorAvailabilityService = tutorAvailabilityService;
			_context = context;
		}

		/// <summary>
		/// Tạo nhiều lịch trình cùng lúc cho gia sư
		/// </summary>

		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor )]
		[HttpPost("tutor-availability-create-list")]
		public async Task<IActionResult> TutorAvailabilityCreateList([FromBody] List<TutorAvailabilityCreateRequest> requests)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", ModelState));
			}

			await using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var results = await _tutorAvailabilityService.CreateBulkAsync(requests);

				await transaction.CommitAsync();
				return Ok(ApiResponse<List<TutorAvailabilityDto>>.Ok(results, $"Tạo {results.Count} lịch trình thành công"));
			}
			catch (ArgumentException ex)
			{
				await transaction.RollbackAsync();
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
			catch (InvalidOperationException ex)
			{
				await transaction.RollbackAsync();
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return StatusCode(500, ApiResponse<object>.Fail("Lỗi hệ thống", ex.Message));
			}
		}

		/// <summary>
		/// Cập nhật 1 lịch trình
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor)]
		[HttpPut("tutor-availability-update")]
		public async Task<IActionResult> TutorAvailabilityUpdate([FromBody] TutorAvailabilityUpdateRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ", ModelState));
			}

			await using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var result = await _tutorAvailabilityService.UpdateAsync(request);

				await transaction.CommitAsync();
				return Ok(ApiResponse<TutorAvailabilityDto>.Ok(result, "Cập nhật lịch trình thành công"));
			}
			catch (ArgumentException ex)
			{
				await transaction.RollbackAsync();
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
			catch (InvalidOperationException ex)
			{
				await transaction.RollbackAsync();
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return StatusCode(500, ApiResponse<object>.Fail("Lỗi hệ thống", ex.Message));
			}
		}

		/// <summary>
		/// Xóa nhiều lịch trình cùng lúc (chỉ được xóa khi status là Available)
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor)]
		[HttpDelete("tutor-availability-delete-list")]
		public async Task<IActionResult> TutorAvailabilityDeleteList([FromBody] List<int> ids)
		{
			await using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var deletedIds = new List<int>();
				foreach (var id in ids)
				{
					// Kiểm tra status trước khi xóa
					var availability = await _tutorAvailabilityService.GetByIdFullAsync(id);
					if (availability == null)
					{
						throw new ArgumentException($"Không tìm thấy lịch trình với ID {id}");
					}

					if (availability.Status != TutorAvailabilityStatus.Available)
					{
						throw new ArgumentException($"Chỉ được xóa lịch trình có trạng thái Available. ID {id} có trạng thái: {availability.Status}");
					}

					await _tutorAvailabilityService.DeleteAsync(id);
					deletedIds.Add(id);
				}
				
				await transaction.CommitAsync();
				return Ok(ApiResponse<List<int>>.Ok(deletedIds, $"Xóa {deletedIds.Count} lịch trình thành công"));
			}
			catch (ArgumentException ex)
			{
				await transaction.RollbackAsync();
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return StatusCode(500, ApiResponse<object>.Fail("Lỗi hệ thống", ex.Message));
			}
		}

		/// <summary>
		/// Lấy tất cả lịch trình của gia sư (repository đã lọc những ngày còn tương lai)
		/// </summary>
		[HttpGet("tutor-availability-get-all/{tutorId}")]
		public async Task<IActionResult> TutorAvailabilityGetAll(int tutorId)
		{
			try
			{
				// Repository đã lọc StartDate >= now, không cần filter lại
				var availabilities = await _tutorAvailabilityService.GetByTutorIdFullAsync(tutorId);

				return Ok(ApiResponse<IReadOnlyList<TutorAvailabilityDto>>.Ok(
					availabilities, 
					$"Lấy danh sách lịch trình của gia sư {tutorId} thành công. Tìm thấy {availabilities.Count} lịch trình"));
			}
			catch (Exception ex)
			{
				return StatusCode(500, ApiResponse<object>.Fail("Lỗi hệ thống", ex.Message));
			}
		}


		/// <summary>
		/// Lấy lịch trình theo trạng thái (repository đã lọc những ngày còn tương lai)
		/// </summary>
		[HttpGet("tutor-availability-get-list-by-status/{tutorId}/{status}")]
		public async Task<IActionResult> TutorAvailabilityGetListByStatus(int tutorId, TutorAvailabilityStatus status)
		{
			try
			{
				var allAvailabilities = await _tutorAvailabilityService.GetByTutorIdFullAsync(tutorId);
				
				// filter theo status
				var filteredAvailabilities = allAvailabilities
					.Where(ta => ta.Status == status)
					.ToList();

				return Ok(ApiResponse<IReadOnlyList<TutorAvailabilityDto>>.Ok(
					filteredAvailabilities, 
					$"Lấy danh sách lịch trình của gia sư {tutorId} theo trạng thái {status} thành công. Tìm thấy {filteredAvailabilities.Count} lịch trình"));
			}
			catch (Exception ex)
			{
				return StatusCode(500, ApiResponse<object>.Fail("Lỗi hệ thống", ex.Message));
			}
		}

		/// <summary>
		/// Cập nhật trạng thái của một lịch trình
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor)]
		[HttpPut("tutor-availability-update-status/{id}")]
		public async Task<IActionResult> TutorAvailabilityUpdateStatus(int id, [FromQuery] TutorAvailabilityStatus status)
		{
			try
			{
				var result = await _tutorAvailabilityService.UpdateStatusAsync(id, status);
				return Ok(ApiResponse<TutorAvailabilityDto>.Ok(result, "Cập nhật trạng thái lịch trình thành công"));
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ApiResponse<object>.Fail(ex.Message));
			}
			catch (Exception ex)
			{
				return StatusCode(500, ApiResponse<object>.Fail("Lỗi hệ thống", ex.Message));
			}
		}
	}
}
