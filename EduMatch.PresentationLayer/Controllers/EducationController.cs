using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EducationController : ControllerBase
	{
		private readonly IEducationInstitutionService _educationInstitutionService;

		public EducationController(IEducationInstitutionService educationInstitutionService)
		{
			_educationInstitutionService = educationInstitutionService;
		}

		// get all education institutions
		[HttpGet("get-all-education-institution")]
		[ProducesResponseType(typeof(ApiResponse<List<EducationInstitutionDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetAllEducationInstitution()
		{
			try
			{
				var data = await _educationInstitutionService.GetAllAsync();

				if (data == null || !data.Any())
				{
					return Ok(ApiResponse<List<EducationInstitutionDto>>.Ok(
						new List<EducationInstitutionDto>(),
						"No education institutions found in the system."
					));
				}

				return Ok(ApiResponse<List<EducationInstitutionDto>>.Ok(
					data.ToList(),
					"Successfully retrieved the list of education institutions."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while retrieving the list of education institutions.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}



	}
}
