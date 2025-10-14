using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.PresentationLayer.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TutorsController : ControllerBase
	{
		private readonly EduMatchContext _eduMatch;
		private readonly ITutorSubjectService _tutorSubjectService;
		private readonly ISubjectService _subjectService;
		private readonly ILevelService _levelService;
		private readonly CurrentUserService _currentUserService;
		private readonly ITutorAvailabilityService _tutorAvailabilityService;
		private readonly ITutorProfileService _tutorProfileService;
		private readonly ITutorCertificateService _tutorCertificateService;
		private readonly ITutorEducationService _tutorEducationService;

		public TutorsController(
			ITutorSubjectService tutorSubjectService,
			ISubjectService subjectService,
			ILevelService levelService,
			CurrentUserService currentUserService,
			ITutorAvailabilityService tutorAvailabilityService,
			ITutorProfileService tutorProfileService,
			ITutorCertificateService tutorCertificateService,
			EduMatchContext eduMatch,
			ITutorEducationService tutorEducationService

			)
		{
			_tutorSubjectService = tutorSubjectService;
			_subjectService = subjectService;
			_levelService = levelService;
			_currentUserService = currentUserService;
			_tutorAvailabilityService = tutorAvailabilityService;
			_tutorProfileService = tutorProfileService;
			_tutorCertificateService = tutorCertificateService;
			_eduMatch = eduMatch;
			_tutorEducationService = tutorEducationService;

		}



		// became tutor
		[Authorize]
		[HttpPost("become-tutor")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> BecomeTutor([FromForm] BecomeTutorRequest request)
		{
			using var transaction = await _eduMatch.Database.BeginTransactionAsync();

			try
			{
				// Xác thực người dùng
				var userEmail = _currentUserService.Email;
				if (string.IsNullOrWhiteSpace(userEmail))
					return Unauthorized(ApiResponse<string>.Fail("User email not found."));


				//  Tạo hồ sơ TutorProfile



				var profile = await _tutorProfileService.CreateAsync(request.TutorProfile);
				

				
				// Education
				
					await _tutorEducationService.CreateBulkAsync(request.Educations);
			

			
				//  Certificate
				
				
					await _tutorCertificateService.CreateBulkAsync(request.Certificates);
			

				
				//  Subject
				
				
					await _tutorSubjectService.CreateBulkAsync(request.Subjects);
				

				
				// Availability (NonRecurring + Recurring)
				 await _tutorAvailabilityService.CreateMixedAvailabilitiesAsync(request.Availabilities);


				//  Commit transaction
				
				await transaction.CommitAsync();

				return Ok(ApiResponse<object>.Ok(new
				{
					profile,
					status = "Pending"
				}, "Tutor profile created successfully and pending approval."));
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				return BadRequest(ApiResponse<string>.Fail(
					"Failed to create tutor profile.",
					new { exception = ex.Message }
				));
			}
		}


	}
}
