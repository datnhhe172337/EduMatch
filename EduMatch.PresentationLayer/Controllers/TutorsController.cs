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
		[ProducesResponseType(typeof(ApiResponse<TutorProfileDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> BecomeTutor([FromForm] BecomeTutorRequest request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ApiResponse<string>.Fail("Invalid request."));

			// 1) Xác thực
			var userEmail = _currentUserService.Email;
			if (string.IsNullOrWhiteSpace(userEmail))
				return Unauthorized(ApiResponse<string>.Fail("User email not found."));

			// đảm bảo profile request có email đúng user hiện tại (nếu bạn lưu trường này)
			request.TutorProfile.UserEmail = userEmail;

			// 2) Transaction cho toàn bộ quy trình
			await using var tx = await _eduMatch.Database.BeginTransactionAsync();
			try
			{
				// 3) Tạo TutorProfile (trả về Id để gán cho các entity con)
				var profileDto = await _tutorProfileService.CreateAsync(request.TutorProfile);
				var tutorId = profileDto.Id;

				// 4) Gắn TutorId cho các request con (nếu chưa có)
				if (request.Educations != null && request.Educations.Count > 0)
				{
					foreach (var e in request.Educations) e.TutorId = tutorId;
					await _tutorEducationService.CreateBulkAsync(request.Educations);
				}

				if (request.Certificates != null && request.Certificates.Count > 0)
				{
					foreach (var c in request.Certificates) c.TutorId = tutorId;
					await _tutorCertificateService.CreateBulkAsync(request.Certificates);
				}

				if (request.Subjects != null && request.Subjects.Count > 0)
				{
					foreach (var s in request.Subjects) s.TutorId = tutorId;
					await _tutorSubjectService.CreateBulkAsync(request.Subjects);
				}

				if (request.Availabilities != null)
				{
					request.Availabilities.TutorId = tutorId;
					await _tutorAvailabilityService.CreateMixedAvailabilitiesAsync(request.Availabilities);
				}

				// 5) Commit
				await tx.CommitAsync();

				// 6) Re-load full profile để trả ra UI (đồng bộ trạng thái DB)
				//    Service nên include: Educations, Certificates, Subjects, Availabilities
				var fullProfile = await _tutorProfileService.GetByIdFullAsync(tutorId);

				// 7) Trả về full profile + trạng thái
				return Ok(ApiResponse<object>.Ok(new
				{
					profile = fullProfile,
					status = "Pending"
				}, "Tutor profile created successfully and pending approval."));
			}
			catch (Exception ex)
			{
				await tx.RollbackAsync();
				return BadRequest(ApiResponse<string>.Fail(
					"Failed to create tutor profile.",
					new { exception = ex.Message }
				));
			}
		}



	}
}
