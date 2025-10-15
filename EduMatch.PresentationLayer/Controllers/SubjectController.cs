using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SubjectController : ControllerBase
	{
		private readonly ISubjectService _subjectService;

		public SubjectController(ISubjectService subjectService)
		{
			_subjectService = subjectService;
		}

		[HttpGet("get-all-subject")]
		[ProducesResponseType(typeof(ApiResponse<List<SubjectDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetAllSubject()
		{
			try
			{
				var data = await _subjectService.GetAllAsync();

				if (data == null || !data.Any())
				{
					return Ok(ApiResponse<List<SubjectDto>>.Ok(
						new List<SubjectDto>(),
						"No subjects found in the system."
					));
				}

				return Ok(ApiResponse<List<SubjectDto>>.Ok(
					data.ToList(),
					"Successfully retrieved the list of subjects."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while retrieving the list of subjects.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}
	}
}
