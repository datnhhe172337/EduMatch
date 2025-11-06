using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.TutorEducation;
using EduMatch.BusinessLogicLayer.Requests.EducationInstitution;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.BusinessLogicLayer.Services;
using Microsoft.AspNetCore.Authorization;
using EduMatch.BusinessLogicLayer.Constants;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EducationController : ControllerBase
	{
		private readonly IEducationInstitutionService _educationInstitutionService;
		private readonly ITutorEducationService _tutorEducationService;
		private readonly CurrentUserService _currentUserService;

		public EducationController(IEducationInstitutionService educationInstitutionService, ITutorEducationService tutorEducationService, CurrentUserService currentUserService)
		{
			_educationInstitutionService = educationInstitutionService;
			_tutorEducationService = tutorEducationService;
			_currentUserService = currentUserService;
		}

		/// <summary>
		/// Lấy danh sách tất cả các cơ sở giáo dục có sẵn trong hệ thống
		/// </summary>
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

		/// <summary>
		/// Tạo mới cơ sở giáo dục
		/// </summary>
		
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Learner + "," + Roles.Tutor)]
		[HttpPost("create-education-institution")]
		[ProducesResponseType(typeof(ApiResponse<EducationInstitutionDto>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> CreateEducationInstitution([FromBody] EducationInstitutionCreateRequest request)
		{
			try
			{
				if (request == null)
				{
					return BadRequest(ApiResponse<string>.Fail("Request body cannot be null."));
				}

				if (!ModelState.IsValid)
				{
					return BadRequest(ApiResponse<string>.Fail("Validation failed"));
				}

				// Create the education institution
				var result = await _educationInstitutionService.CreateAsync(request);

				return Ok(
					ApiResponse<EducationInstitutionDto>.Ok(
						result,
						"Education institution created successfully with Pending status."
					)
				);
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while creating the education institution.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		/// <summary>
		/// Lấy danh sách các bằng cấp học vấn của một gia sư
		/// </summary>
		// Get tutor education list by tutor ID
		[HttpGet("get-{tutorId}-list-education")]
		[ProducesResponseType(typeof(ApiResponse<List<TutorEducationDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetTutorEducationList(int tutorId)
		{
			try
			{
				if (tutorId <= 0)
				{
					return BadRequest(ApiResponse<string>.Fail("Invalid tutor ID. Tutor ID must be greater than 0."));
				}

				var data = await _tutorEducationService.GetByTutorIdAsync(tutorId);

				if (data == null || !data.Any())
				{
					return Ok(ApiResponse<List<TutorEducationDto>>.Ok(
						new List<TutorEducationDto>(),
						"No education records found for this tutor."
					));
				}

				return Ok(ApiResponse<List<TutorEducationDto>>.Ok(
					data.ToList(),
					"Successfully retrieved the tutor's education records."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while retrieving the tutor's education records.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		/// <summary>
		/// Thêm bằng cấp học vấn mới cho gia sư
		/// </summary>
		// Create tutor education
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Learner + "," + Roles.Tutor)]
		[HttpPost("create-{tutorId}-education")]
		[ProducesResponseType(typeof(ApiResponse<TutorEducationDto>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> CreateTutorEducation(int tutorId, [FromBody] TutorEducationCreateRequest request)
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


				var result = await _tutorEducationService.CreateAsync(request);

				return CreatedAtAction(
					nameof(GetTutorEducationList),
					new { tutorId = tutorId },
					ApiResponse<TutorEducationDto>.Ok(
						result,
						"Tutor education record created successfully."
					)
				);
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while creating the tutor education record.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		/// <summary>
		/// Cập nhật thông tin bằng cấp học vấn của gia sư
		/// </summary>
		// Update tutor education
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor)]
		[HttpPut("update-{tutorId}-education")]
		[ProducesResponseType(typeof(ApiResponse<TutorEducationDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> UpdateTutorEducation(int tutorId, [FromBody] TutorEducationUpdateRequest request)
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


				var result = await _tutorEducationService.UpdateAsync(request);

				return Ok(ApiResponse<TutorEducationDto>.Ok(
					result,
					"Tutor education record updated successfully."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while updating the tutor education record.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		/// <summary>
		/// Xóa bằng cấp học vấn của gia sư (có thể xóa một bằng cụ thể)
		/// </summary>
		// Delete tutor education
		[Authorize(Roles = Roles.BusinessAdmin + ","  + Roles.Tutor)]
		[HttpDelete("delete-{tutorId}-education")]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> DeleteTutorEducation(int tutorId, [FromQuery] int? educationId = null)
		{
			try
			{
				if (tutorId <= 0)
				{
					return BadRequest(ApiResponse<string>.Fail("Invalid tutor ID. Tutor ID must be greater than 0."));
				}

				if (educationId.HasValue && educationId <= 0)
				{
					return BadRequest(ApiResponse<string>.Fail("Invalid education ID. Education ID must be greater than 0."));
				}

				if (educationId.HasValue)
				{
					// Delete specific education record
					await _tutorEducationService.DeleteAsync(educationId.Value);
					return Ok(ApiResponse<string>.Ok(
						"Tutor education record deleted successfully."
					));
				}
				else
				{
					// Delete all education records for the tutor
					await _tutorEducationService.DeleteByTutorIdAsync(tutorId);
					return Ok(ApiResponse<string>.Ok(
						"All tutor education records deleted successfully."
					));
				}
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while deleting the tutor education record(s).",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		/// <summary>
		/// Lấy danh sách các cơ sở giáo dục theo trạng thái verify
		/// </summary>
			
		[HttpGet("get-education-institutions-by-verify-status/{verifyStatus}")]
		[ProducesResponseType(typeof(ApiResponse<List<EducationInstitutionDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetEducationInstitutionsByVerifyStatus(VerifyStatus verifyStatus)
		{
			try
			{
				var allInstitutions = await _educationInstitutionService.GetAllAsync();
				var filteredInstitutions = allInstitutions?.Where(ei => ei.Verified == verifyStatus).ToList() ?? new List<EducationInstitutionDto>();

				if (!filteredInstitutions.Any())
				{
					return Ok(ApiResponse<List<EducationInstitutionDto>>.Ok(
						new List<EducationInstitutionDto>(),
						$"No education institutions found with {verifyStatus} status."
					));
				}

				return Ok(ApiResponse<List<EducationInstitutionDto>>.Ok(
					filteredInstitutions,
					$"Successfully retrieved education institutions with {verifyStatus} status."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while retrieving education institutions by verify status.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		/// <summary>
		/// Verify cơ sở giáo dục từ Pending sang Verified
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin )]
		[HttpPut("verify-education-institution/{educationInstitutionId}")]
		[ProducesResponseType(typeof(ApiResponse<EducationInstitutionDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> VerifyEducationInstitution(int educationInstitutionId)
		{
			try
			{
				if (educationInstitutionId <= 0)
				{
					return BadRequest(ApiResponse<string>.Fail("Invalid education institution ID. Education institution ID must be greater than 0."));
				}

				// Get current user email for verification
				var currentUserEmail = _currentUserService.Email ?? "System";

				// Get the education institution to check current status
				var educationInstitution = await _educationInstitutionService.GetByIdAsync(educationInstitutionId);

				if (educationInstitution == null)
				{
					return NotFound(ApiResponse<string>.Fail($"Education institution with ID {educationInstitutionId} not found."));
				}

				if (educationInstitution.Verified != VerifyStatus.Pending)
				{
					return BadRequest(ApiResponse<string>.Fail($"Education institution with ID {educationInstitutionId} is not in Pending status for verification."));
				}

				// Verify the education institution
				var result = await _educationInstitutionService.VerifyAsync(educationInstitutionId, currentUserEmail);

				return Ok(ApiResponse<EducationInstitutionDto>.Ok(
					result,
					$"Education institution verified successfully by {currentUserEmail}."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while verifying the education institution.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}




	}
}
