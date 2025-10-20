using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.CertificateType;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CertificateController : ControllerBase
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ICertificateTypeService _certificateTypeService;

		public CertificateController(IHttpContextAccessor httpContextAccessor, ICertificateTypeService certificateTypeService)
		{
			_httpContextAccessor = httpContextAccessor;
			_certificateTypeService = certificateTypeService;
		}



		// get all certificate types with subjects 
		[HttpGet("get-all-certificatetypes-with-subjects")]
		[ProducesResponseType(typeof(ApiResponse<List<CertificateTypeDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetAllCertificateTypesWithSubjects()
		{
			try
			{
				// Service BLL trả về CertificateTypeDto đã gồm Subjects (Id, SubjectName)
				var types = await _certificateTypeService.GetAllAsync();

				// Phòng trường hợp null -> trả list rỗng
				var data = types?.ToList() ?? new List<CertificateTypeDto>();

				return Ok(ApiResponse<List<CertificateTypeDto>>.Ok(data));
			}
			catch (Exception ex)
			{

				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail("Internal Server Error", ex.Message)
				);
			}
		}

		


	}
}

