using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TutorsController : ControllerBase
	{

		private readonly ITutorSubjectService _tutorSubjectService;
		private readonly ISubjectService _subjectService;
		private readonly ILevelService _levelService;
		private readonly CurrentUserService _currentUserService;

		public TutorsController(
			ITutorSubjectService tutorSubjectService,
			ISubjectService subjectService,
			ILevelService levelService,
			CurrentUserService currentUserService
			)
		{
			_tutorSubjectService = tutorSubjectService;
			_subjectService = subjectService;
			_levelService = levelService;
			_currentUserService = currentUserService;
		}

		// create bulk  tutor - subjects

		[Authorize]
		[HttpPost("add-tutor-subjects")]
		[ProducesResponseType(typeof(ApiResponse<List<TutorSubjectDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> AddTutorSubjects(
			[FromBody] List<TutorSubjectCreateRequest> requests)
		{
			try
			{
				if (requests == null || requests.Count == 0)
				{
					return BadRequest(ApiResponse<string>.Fail("Request list cannot be empty."));
				}

				var validRequests = new List<TutorSubjectCreateRequest>();

				foreach (var req in requests)
				{
					// Validate DataAnnotations
					var validationContext = new ValidationContext(req);
					var validationResults = new List<ValidationResult>();
					if (!Validator.TryValidateObject(req, validationContext, validationResults, true))
					{
						return BadRequest(ApiResponse<string>.Fail(
							$"Validation failed for TutorId={req.TutorId}: " +
							string.Join(", ", validationResults.Select(v => v.ErrorMessage))
						));
					}

					// Check Subject existence
					var subject = await _subjectService.GetByIdAsync(req.SubjectId);
					if (subject == null)
					{
						return BadRequest(ApiResponse<string>.Fail(
							$"Subject with ID {req.SubjectId} does not exist."
						));
					}

					// Check Level existence (if provided)
					if (req.LevelId.HasValue)
					{
						var level = await _levelService.GetByIdAsync(req.LevelId.Value);
						if (level == null)
						{
							return BadRequest(ApiResponse<string>.Fail(
								$"Level with ID {req.LevelId} does not exist."
							));
						}
					}

					validRequests.Add(req);
				}

				// ✅ Call BLL service to create bulk
				var created = await _tutorSubjectService.CreateBulkAsync(validRequests);

				return Ok(ApiResponse<List<TutorSubjectDto>>.Ok(
					created.ToList(),
					"Successfully created tutor-subject associations."
				));
			}
			catch (Exception ex)
			{
				return StatusCode(
					StatusCodes.Status500InternalServerError,
					ApiResponse<string>.Fail(
						"An error occurred while creating tutor-subject associations.",
						new { error = ex.Message, stackTrace = ex.StackTrace }
					)
				);
			}
		}

	}
}
