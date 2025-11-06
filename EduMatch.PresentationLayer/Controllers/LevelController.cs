using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LevelController : ControllerBase
	{

		private readonly ILevelService _levelService;

		public LevelController(ILevelService levelService)
		{
			_levelService = levelService;
		}

		/// <summary>
		/// Lấy danh sách tất cả các cấp độ học tập có sẵn trong hệ thống
		/// </summary>
		// get all levels
		[HttpGet("get-all-level")]
		[ProducesResponseType(typeof(ApiResponse<List<LevelDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetAllLevel()
		{
			try
			{
				var data = await _levelService.GetAllAsync();

				if (data == null || !data.Any())
				{
					return Ok(ApiResponse<List<LevelDto>>.Ok(
						new List<LevelDto>(),
						"No levels found in the system."
					));
				}

				return Ok(ApiResponse<List<LevelDto>>.Ok(
					data.ToList(),
					"Successfully retrieved the list of levels."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while retrieving the list of levels.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

	}
}
