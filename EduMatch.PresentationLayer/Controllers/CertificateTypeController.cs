using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.CertificateType;
using EduMatch.PresentationLayer.Common;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.BusinessLogicLayer.Services;
using Microsoft.AspNetCore.Authorization;
using EduMatch.BusinessLogicLayer.Constants;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CertificateTypeController : ControllerBase
	{
		private readonly ICertificateTypeService _certificateTypeService;
		private readonly ISubjectService _subjectService;
		private readonly CurrentUserService _currentUserService;

		public CertificateTypeController(ICertificateTypeService certificateTypeService, ISubjectService subjectService, CurrentUserService currentUserService)
		{
			_certificateTypeService = certificateTypeService;
			_subjectService = subjectService;
			_currentUserService = currentUserService;
		}

		/// <summary>
		/// Lấy danh sách tất cả các loại chứng chỉ
		/// </summary>
		[HttpGet("get-all-certificate-types")]
		[ProducesResponseType(typeof(ApiResponse<List<CertificateTypeDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetAllCertificateTypes()
		{
			try
			{
				var data = await _certificateTypeService.GetAllAsync();

				if (data == null || !data.Any())
				{
					return Ok(ApiResponse<List<CertificateTypeDto>>.Ok(
						new List<CertificateTypeDto>(),
						"No certificate types found in the system."
					));
				}

				return Ok(ApiResponse<List<CertificateTypeDto>>.Ok(
					data.ToList(),
					"Successfully retrieved the list of certificate types."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while retrieving the list of certificate types.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		/// <summary>
		/// Lấy danh sách các loại chứng chỉ theo trạng thái verify
		/// </summary>
		[HttpGet("get-certificate-types-by-verify-status/{verifyStatus}")]
		[ProducesResponseType(typeof(ApiResponse<List<CertificateTypeDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetCertificateTypesByVerifyStatus(VerifyStatus verifyStatus)
		{
			try
			{
				var allTypes = await _certificateTypeService.GetAllAsync();
				var filteredTypes = allTypes?.Where(ct => ct.Verified == verifyStatus).ToList() ?? new List<CertificateTypeDto>();

				if (!filteredTypes.Any())
				{
					return Ok(ApiResponse<List<CertificateTypeDto>>.Ok(
						new List<CertificateTypeDto>(),
						$"No certificate types found with {verifyStatus} status."
					));
				}

				return Ok(ApiResponse<List<CertificateTypeDto>>.Ok(
					filteredTypes,
					$"Successfully retrieved certificate types with {verifyStatus} status."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while retrieving certificate types by verify status.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		/// <summary>
		/// Tạo loại chứng chỉ mới với trạng thái Pending
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Learner + "," + Roles.Tutor)]
		[HttpPost("create-certificate-type")]
		[ProducesResponseType(typeof(ApiResponse<CertificateTypeDto>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> CreateCertificateType([FromBody] CertificateTypeCreateRequest request)
		{
			try
			{
				if (request == null)
				{
					return BadRequest(ApiResponse<string>.Fail("Request body cannot be null."));
				}

				var result = await _certificateTypeService.CreateAsync(request);

				return CreatedAtAction(
					nameof(GetAllCertificateTypes),
					ApiResponse<CertificateTypeDto>.Ok(
						result,
						"Certificate type created successfully with Pending status."
					)
				);
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while creating the certificate type.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		/// <summary>
		/// Thêm các môn học vào loại chứng chỉ
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Learner + "," + Roles.Tutor)]
		[HttpPost("add-subjects-to-certificate-type/{certificateTypeId}")]
		[ProducesResponseType(typeof(ApiResponse<CertificateTypeDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> AddSubjectsToCertificateType(int certificateTypeId, [FromBody] List<int> subjectIds)
		{
			try
			{
				if (certificateTypeId <= 0)
				{
					return BadRequest(ApiResponse<string>.Fail("Invalid certificate type ID. Certificate type ID must be greater than 0."));
				}

				if (subjectIds == null || !subjectIds.Any())
				{
					return BadRequest(ApiResponse<string>.Fail("Subject IDs list cannot be null or empty."));
				}

				// Check if certificate type exists
				var certificateType = await _certificateTypeService.GetByIdAsync(certificateTypeId);
				if (certificateType == null)
				{
					return NotFound(ApiResponse<string>.Fail($"Certificate type with ID {certificateTypeId} not found."));
				}

				// Validate all subject IDs exist
				var allSubjects = await _subjectService.GetAllAsync();
				var existingSubjectIds = allSubjects.Select(s => s.Id).ToList();
				var invalidSubjectIds = subjectIds.Where(id => !existingSubjectIds.Contains(id)).ToList();

				if (invalidSubjectIds.Any())
				{
					return BadRequest(ApiResponse<string>.Fail($"Invalid subject IDs: {string.Join(", ", invalidSubjectIds)}"));
				}

				// Add subjects to certificate type
				var result = await _certificateTypeService.AddSubjectsToCertificateTypeAsync(certificateTypeId, subjectIds);
				
				return Ok(ApiResponse<CertificateTypeDto>.Ok(
					result,
					$"Successfully added subjects to certificate type {certificateTypeId}."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while adding subjects to certificate type.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		
		/// <summary>
		/// Verify loại chứng chỉ từ Pending sang Verified
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin)]
		[HttpPut("verify-certificate-type/{certificateTypeId}")]
		[ProducesResponseType(typeof(ApiResponse<CertificateTypeDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> VerifyCertificateType(int certificateTypeId)
		{
			try
			{
				if (certificateTypeId <= 0)
				{
					return BadRequest(ApiResponse<string>.Fail("Invalid certificate type ID. Certificate type ID must be greater than 0."));
				}

				// Get current user email for verification
				var currentUserEmail = _currentUserService.Email ?? "System";

				// Get the certificate type to check current status
				var certificateType = await _certificateTypeService.GetByIdAsync(certificateTypeId);

				if (certificateType == null)
				{
					return NotFound(ApiResponse<string>.Fail($"Certificate type with ID {certificateTypeId} not found."));
				}

				if (certificateType.Verified != VerifyStatus.Pending)
				{
					return BadRequest(ApiResponse<string>.Fail($"Certificate type with ID {certificateTypeId} is not in Pending status for verification."));
				}

				// Verify the certificate type
				var result = await _certificateTypeService.VerifyAsync(certificateTypeId, currentUserEmail);

				return Ok(ApiResponse<CertificateTypeDto>.Ok(
					result,
					$"Certificate type verified successfully by {currentUserEmail}."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while verifying the certificate type.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		/// <summary>
		/// Xóa loại chứng chỉ
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin)]
		[HttpDelete("delete-certificate-type/{certificateTypeId}")]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> DeleteCertificateType(int certificateTypeId)
		{
			try
			{
				if (certificateTypeId <= 0)
				{
					return BadRequest(ApiResponse<string>.Fail("Invalid certificate type ID. Certificate type ID must be greater than 0."));
				}

				await _certificateTypeService.DeleteAsync(certificateTypeId);

				return Ok(ApiResponse<string>.Ok(
					"Certificate type deleted successfully."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while deleting the certificate type.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

	}
}
