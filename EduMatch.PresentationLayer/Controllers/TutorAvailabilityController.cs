using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TutorAvailability;
using EduMatch.PresentationLayer.Common;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Entities;
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
		[HttpPost("tutor-availability-create-list")]
		public async Task<IActionResult> TutorAvailabilityCreateList([FromBody] List<TutorAvailabilityCreateRequest> requests)
		{
			await using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var results = new List<TutorAvailabilityDto>();
				foreach (var request in requests)
				{
					var result = await _tutorAvailabilityService.CreateAsync(request);
					results.Add(result);
				}
				
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
		/// Cập nhật nhiều lịch trình cùng lúc
		/// </summary>
		[HttpPut("tutor-availability-update-list")]
		public async Task<IActionResult> TutorAvailabilityUpdateList([FromBody] List<TutorAvailabilityUpdateRequest> requests)
		{
			await using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var results = new List<TutorAvailabilityDto>();
				foreach (var request in requests)
				{
					var result = await _tutorAvailabilityService.UpdateAsync(request);
					results.Add(result);
				}
				
				await transaction.CommitAsync();
				return Ok(ApiResponse<List<TutorAvailabilityDto>>.Ok(results, $"Cập nhật {results.Count} lịch trình thành công"));
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
		/// Lấy tất cả lịch trình của gia sư với những ngày chưa bắt đầu
		/// </summary>
		[HttpGet("tutor-availability-get-all")]
		public async Task<IActionResult> TutorAvailabilityGetAll()
		{
			try
			{
				var allAvailabilities = await _tutorAvailabilityService.GetAllFullAsync();
				
				// Lọc những ngày chưa bắt đầu (StartDate > DateTime.Now)
				var futureAvailabilities = allAvailabilities
					.Where(ta => ta.StartDate > DateTime.Now)
					.ToList();

				return Ok(ApiResponse<IReadOnlyList<TutorAvailabilityDto>>.Ok(
					futureAvailabilities, 
					$"Lấy danh sách lịch trình thành công. Tìm thấy {futureAvailabilities.Count} lịch trình chưa bắt đầu"));
			}
			catch (Exception ex)
			{
				return StatusCode(500, ApiResponse<object>.Fail("Lỗi hệ thống", ex.Message));
			}
		}

		/// <summary>
		/// Lấy lịch trình theo trạng thái với những ngày chưa bắt đầu
		/// </summary>
		[HttpGet("tutor-availability-get-list-by-status/{status}")]
		public async Task<IActionResult> TutorAvailabilityGetListByStatus(TutorAvailabilityStatus status)
		{
			try
			{
				var allAvailabilities = await _tutorAvailabilityService.GetAllFullAsync();
				
				// Lọc những ngày chưa bắt đầu (StartDate > DateTime.Now) và theo trạng thái
				var filteredAvailabilities = allAvailabilities
					.Where(ta => ta.StartDate > DateTime.Now && ta.Status == status)
					.ToList();

				return Ok(ApiResponse<IReadOnlyList<TutorAvailabilityDto>>.Ok(
					filteredAvailabilities, 
					$"Lấy danh sách lịch trình theo trạng thái {status} thành công. Tìm thấy {filteredAvailabilities.Count} lịch trình chưa bắt đầu"));
			}
			catch (Exception ex)
			{
				return StatusCode(500, ApiResponse<object>.Fail("Lỗi hệ thống", ex.Message));
			}
		}
	}
}
