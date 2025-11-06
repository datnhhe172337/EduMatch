using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.CertificateType;
using EduMatch.BusinessLogicLayer.Requests.TutorCertificate;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.BusinessLogicLayer.Services;
using Microsoft.AspNetCore.Authorization;
using EduMatch.BusinessLogicLayer.Constants;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CertificateController : ControllerBase
	{
		private readonly ICertificateTypeService _certificateTypeService;
		private readonly ITutorCertificateService _tutorCertificateService;
		private readonly CurrentUserService _currentUserService;

		public CertificateController(ICertificateTypeService certificateTypeService, ITutorCertificateService tutorCertificateService, CurrentUserService currentUserService)
		{
			_certificateTypeService = certificateTypeService;
			_tutorCertificateService = tutorCertificateService;
			_currentUserService = currentUserService;
		}



		/// <summary>
		/// Lấy danh sách tất cả các loại chứng chỉ kèm theo môn học
		/// </summary>
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

		/// <summary>
		/// Lấy danh sách các chứng chỉ của một gia sư
		/// </summary>
		// Get tutor certificate list by tutor ID
		[HttpGet("get-{tutorId}-list-certificate")]
		[ProducesResponseType(typeof(ApiResponse<List<TutorCertificateDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetTutorCertificateList(int tutorId)
		{
			try
			{
				if (tutorId <= 0)
				{
					return BadRequest(ApiResponse<string>.Fail("Invalid tutor ID. Tutor ID must be greater than 0."));
				}

				var data = await _tutorCertificateService.GetByTutorIdAsync(tutorId);

				if (data == null || !data.Any())
				{
					return Ok(ApiResponse<List<TutorCertificateDto>>.Ok(
						new List<TutorCertificateDto>(),
						"No certificate records found for this tutor."
					));
				}

				return Ok(ApiResponse<List<TutorCertificateDto>>.Ok(
					data.ToList(),
					"Successfully retrieved the tutor's certificate records."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while retrieving the tutor's certificate records.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		/// <summary>
		/// Thêm chứng chỉ mới cho gia sư
		/// </summary>
		// Create tutor certificate
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Learner + "," + Roles.Tutor)]
		[HttpPost("create-{tutorId}-certificate")]
		[ProducesResponseType(typeof(ApiResponse<TutorCertificateDto>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> CreateTutorCertificate(int tutorId, [FromBody] TutorCertificateCreateRequest request)
		{
			try
			{
				if (tutorId <= 0)
				{
					return BadRequest(ApiResponse<string>.Fail("Invalid tutor ID. Tutor ID must be greater than 0."));
				}

				if (request == null)
				{
					return BadRequest(ApiResponse<string>.Fail("Request body cannot be null."));
				}

				// Set the tutor ID from the route parameter
				request.TutorId = tutorId;


				var result = await _tutorCertificateService.CreateAsync(request);

				return CreatedAtAction(
					nameof(GetTutorCertificateList),
					new { tutorId = tutorId },
					ApiResponse<TutorCertificateDto>.Ok(
						result,
						"Tutor certificate record created successfully."
					)
				);
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while creating the tutor certificate record.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		/// <summary>
		/// Cập nhật thông tin chứng chỉ của gia sư
		/// </summary>
		// Update tutor certificate
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor)]
		[HttpPut("update-{tutorId}-certificate")]
		[ProducesResponseType(typeof(ApiResponse<TutorCertificateDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> UpdateTutorCertificate(int tutorId, [FromBody] TutorCertificateUpdateRequest request)
		{
			try
			{
				if (tutorId <= 0)
				{
					return BadRequest(ApiResponse<string>.Fail("Invalid tutor ID. Tutor ID must be greater than 0."));
				}

				if (request == null)
				{
					return BadRequest(ApiResponse<string>.Fail("Request body cannot be null."));
				}

				// Set the tutor ID from the route parameter
				request.TutorId = tutorId;


				var result = await _tutorCertificateService.UpdateAsync(request);

				return Ok(ApiResponse<TutorCertificateDto>.Ok(
					result,
					"Tutor certificate record updated successfully."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while updating the tutor certificate record.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		/// <summary>
		/// Xóa chứng chỉ của gia sư (có thể xóa một chứng chỉ cụ)
		/// </summary>
		// Delete tutor certificate
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor)]
		[HttpDelete("delete-{tutorId}-certificate")]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> DeleteTutorCertificate(int tutorId, [FromQuery] int? certificateId = null)
		{
			try
			{
				if (tutorId <= 0)
				{
					return BadRequest(ApiResponse<string>.Fail("Invalid tutor ID. Tutor ID must be greater than 0."));
				}

				if (certificateId.HasValue && certificateId <= 0)
				{
					return BadRequest(ApiResponse<string>.Fail("Invalid certificate ID. Certificate ID must be greater than 0."));
				}

				if (certificateId.HasValue)
				{
					// Delete specific certificate record
					await _tutorCertificateService.DeleteAsync(certificateId.Value);
					return Ok(ApiResponse<string>.Ok(
						"Tutor certificate record deleted successfully."
					));
				}
				else
				{
					// Delete all certificate records for the tutor
					await _tutorCertificateService.DeleteByTutorIdAsync(tutorId);
					return Ok(ApiResponse<string>.Ok(
						"All tutor certificate records deleted successfully."
					));
				}
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while deleting the tutor certificate record(s).",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

	}
}

