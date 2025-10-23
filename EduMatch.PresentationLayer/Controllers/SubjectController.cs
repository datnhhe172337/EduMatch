using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.TutorSubject;
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
		private readonly ITutorSubjectService _tutorSubjectService;

		public SubjectController(ISubjectService subjectService, ITutorSubjectService tutorSubjectService)
		{
			_subjectService = subjectService;
			_tutorSubjectService = tutorSubjectService;
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



		[HttpGet("get-subject-by-id/{id:int}")]
		[ProducesResponseType(typeof(ApiResponse<SubjectDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetSubjectById(int id)
		{
			try
			{
				var subject = await _subjectService.GetByIdAsync(id);

				if (subject == null)
				{
					return NotFound(ApiResponse<string>.Fail($"Subject with ID {id} was not found."));
				}

				return Ok(ApiResponse<SubjectDto>.Ok(
					subject,
					$"Successfully retrieved subject with ID {id}."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while retrieving the subject.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		// Get tutor subject list by tutor ID
		[HttpGet("get-{tutorId}-list-subject")]
		[ProducesResponseType(typeof(ApiResponse<List<TutorSubjectDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetTutorSubjectList(int tutorId)
		{
			try
			{
				if (tutorId <= 0)
				{
					return BadRequest(ApiResponse<string>.Fail("Invalid tutor ID. Tutor ID must be greater than 0."));
				}

				var data = await _tutorSubjectService.GetByTutorIdAsync(tutorId);

				if (data == null || !data.Any())
				{
					return Ok(ApiResponse<List<TutorSubjectDto>>.Ok(
						new List<TutorSubjectDto>(),
						"No subject records found for this tutor."
					));
				}

				return Ok(ApiResponse<List<TutorSubjectDto>>.Ok(
					data.ToList(),
					"Successfully retrieved the tutor's subject records."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while retrieving the tutor's subject records.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		// Create tutor subject
		[HttpPost("create-{tutorId}-subject")]
		[ProducesResponseType(typeof(ApiResponse<TutorSubjectDto>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> CreateTutorSubject(int tutorId, [FromBody] TutorSubjectCreateRequest request)
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


				var result = await _tutorSubjectService.CreateAsync(request);

				return CreatedAtAction(
					nameof(GetTutorSubjectList),
					new { tutorId = tutorId },
					ApiResponse<TutorSubjectDto>.Ok(
						result,
						"Tutor subject record created successfully."
					)
				);
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while creating the tutor subject record.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		// Update tutor subject
		[HttpPut("update-{tutorId}-subject")]
		[ProducesResponseType(typeof(ApiResponse<TutorSubjectDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> UpdateTutorSubject(int tutorId, [FromBody] TutorSubjectUpdateRequest request)
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


				var result = await _tutorSubjectService.UpdateAsync(request);

				return Ok(ApiResponse<TutorSubjectDto>.Ok(
					result,
					"Tutor subject record updated successfully."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while updating the tutor subject record.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

		// Delete tutor subject
		[HttpDelete("delete-{tutorId}-subject")]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> DeleteTutorSubject(int tutorId, [FromQuery] int? subjectId = null)
		{
			try
			{
				if (tutorId <= 0)
				{
					return BadRequest(ApiResponse<string>.Fail("Invalid tutor ID. Tutor ID must be greater than 0."));
				}

				if (subjectId.HasValue && subjectId <= 0)
				{
					return BadRequest(ApiResponse<string>.Fail("Invalid subject ID. Subject ID must be greater than 0."));
				}

				if (subjectId.HasValue)
				{
					// Delete specific subject record
					await _tutorSubjectService.DeleteAsync(subjectId.Value);
					return Ok(ApiResponse<string>.Ok(
						"Tutor subject record deleted successfully."
					));
				}
				else
				{
					// Delete all subject records for the tutor
					await _tutorSubjectService.DeleteByTutorIdAsync(tutorId);
					return Ok(ApiResponse<string>.Ok(
						"All tutor subject records deleted successfully."
					));
				}
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while deleting the tutor subject record(s).",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

	}
}
