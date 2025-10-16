using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
		private readonly EmailService _emailService;

		public TutorsController(
			ITutorSubjectService tutorSubjectService,
			ISubjectService subjectService,
			ILevelService levelService,
			CurrentUserService currentUserService,
			ITutorAvailabilityService tutorAvailabilityService,
			ITutorProfileService tutorProfileService,
			ITutorCertificateService tutorCertificateService,
			EduMatchContext eduMatch,
			ITutorEducationService tutorEducationService,
			EmailService emailService

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
			_emailService = emailService;

		}


		// beacme tutor
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

			var userEmail = _currentUserService.Email;
			if (string.IsNullOrWhiteSpace(userEmail))
				return Unauthorized(ApiResponse<string>.Fail("User email not found."));

			// 🔒 Đảm bảo các danh sách không bị null
			request.Educations ??= new List<TutorEducationCreateRequest>();
			request.Certificates ??= new List<TutorCertificateCreateRequest>();
			request.Subjects ??= new List<TutorSubjectCreateRequest>();
			request.Availabilities ??= new TutorAvailabilityMixedRequest();

			await using var tx = await _eduMatch.Database.BeginTransactionAsync();
			try
			{
				// Tạo profile
				var profileDto = await _tutorProfileService.CreateAsync(request.TutorProfile);
				var tutorId = profileDto.Id;

				// Bulk insert (nếu có)
				if (request.Educations.Any())
				{
					foreach (var e in request.Educations) e.TutorId = tutorId;
					await _tutorEducationService.CreateBulkAsync(request.Educations);
				}

				if (request.Certificates.Any())
				{
					foreach (var c in request.Certificates) c.TutorId = tutorId;
					await _tutorCertificateService.CreateBulkAsync(request.Certificates);
				}

				if (request.Subjects.Any())
				{
					foreach (var s in request.Subjects) s.TutorId = tutorId;
					await _tutorSubjectService.CreateBulkAsync(request.Subjects);
				}

				if (request.Availabilities is not null)
				{
					request.Availabilities.TutorId = tutorId;
					await _tutorAvailabilityService.CreateMixedAvailabilitiesAsync(request.Availabilities);
				}

				await _emailService.SendBecomeTutorWelcomeAsync(userEmail);
				await tx.CommitAsync();

				var fullProfile = await _tutorProfileService.GetByIdFullAsync(tutorId);

				return Ok(ApiResponse<object>.Ok(new
				{
					profile = fullProfile
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






		// Update tutor education (partial)
		[Authorize]
		[HttpPut("update-education/{id}")]
		[Consumes("multipart/form-data")]
		[ProducesResponseType(typeof(ApiResponse<TutorEducationDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> UpdateEducation([FromRoute] int id, [FromForm] TutorEducationUpdateRequest request)
		{
			try
			{
				var existing = await _tutorEducationService.GetByIdFullAsync(id);
				if (existing == null)
					return NotFound(ApiResponse<string>.Fail("Not found", $"Education #{id} not found"));

				var merged = new TutorEducationUpdateRequest
				{
					Id = id,
					TutorId = request.TutorId != 0 ? request.TutorId : existing.TutorId,
					InstitutionId = request.InstitutionId != 0 ? request.InstitutionId : existing.InstitutionId,
					IssueDate = request.IssueDate ?? existing.IssueDate,
					CertificateEducation = request.CertificateEducation,
					Verified = request.Verified != 0 ? request.Verified : existing.Verified,
					RejectReason = string.IsNullOrWhiteSpace(request.RejectReason) ? existing.RejectReason : request.RejectReason
				};

				var updated = await _tutorEducationService.UpdateAsync(merged);
				return Ok(ApiResponse<TutorEducationDto>.Ok(updated, "Updated"));
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Bad request", ex.Message));
			}
		}

		// Update tutor certificate (partial)
		[Authorize]
		[HttpPut("update-certificate/{id}")]
		[Consumes("multipart/form-data")]
		[ProducesResponseType(typeof(ApiResponse<TutorCertificateDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> UpdateCertificate([FromRoute] int id, [FromForm] TutorCertificateUpdateRequest request)
		{
			try
			{
				var existing = await _tutorCertificateService.GetByIdFullAsync(id);
				if (existing == null)
					return NotFound(ApiResponse<string>.Fail("Not found", $"Certificate #{id} not found"));

				var merged = new TutorCertificateUpdateRequest
				{
					Id = id,
					TutorId = request.TutorId != 0 ? request.TutorId : existing.TutorId,
					CertificateTypeId = request.CertificateTypeId != 0 ? request.CertificateTypeId : existing.CertificateTypeId,
					IssueDate = request.IssueDate ?? existing.IssueDate,
					ExpiryDate = request.ExpiryDate ?? existing.ExpiryDate,
					Certificate = request.Certificate,
					Verified = request.Verified != 0 ? request.Verified : existing.Verified,
					RejectReason = string.IsNullOrWhiteSpace(request.RejectReason) ? existing.RejectReason : request.RejectReason
				};

				var updated = await _tutorCertificateService.UpdateAsync(merged);
				return Ok(ApiResponse<TutorCertificateDto>.Ok(updated, "Updated"));
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Bad request", ex.Message));
			}
		}

		// Update tutor subject (partial)
		[Authorize]
		[HttpPut("update-tutor-subject/{id}")]
		[ProducesResponseType(typeof(ApiResponse<TutorSubjectDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> UpdateSubject([FromRoute] int id, [FromBody] TutorSubjectUpdateRequest request)
		{
			try
			{
				var existing = await _tutorSubjectService.GetByIdFullAsync(id);
				if (existing == null)
					return NotFound(ApiResponse<string>.Fail("Not found", $"TutorSubject #{id} not found"));

				var merged = new TutorSubjectUpdateRequest
				{
					Id = id,
					TutorId = request.TutorId != 0 ? request.TutorId : existing.TutorId,
					SubjectId = request.SubjectId != 0 ? request.SubjectId : existing.SubjectId,
					HourlyRate = request.HourlyRate ?? existing.HourlyRate,
					LevelId = request.LevelId ?? existing.LevelId
				};

				var updated = await _tutorSubjectService.UpdateAsync(merged);
				return Ok(ApiResponse<TutorSubjectDto>.Ok(updated, "Updated"));
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Bad request", ex.Message));
			}
		}

		

		// Batch verify: education
		[Authorize]
		[HttpPut("update-education-list/{tutorId}/verify-batch")]
		[ProducesResponseType(typeof(ApiResponse<List<TutorEducationDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> VerifyEducationBatch([FromRoute] int tutorId, [FromBody] List<VerifyUpdateRequest> updates)
		{
			if (updates == null || updates.Count == 0)
				return BadRequest(ApiResponse<string>.Fail("No updates provided."));

			var results = new List<TutorEducationDto>();
			foreach (var u in updates)
			{
				var existing = await _tutorEducationService.GetByIdFullAsync(u.Id);
				if (existing == null || existing.TutorId != tutorId) continue;
				var req = new TutorEducationUpdateRequest
				{
					Id = u.Id,
					TutorId = existing.TutorId,
					InstitutionId = existing.InstitutionId,
					IssueDate = existing.IssueDate,
					Verified = u.Verified,
					RejectReason = u.RejectReason
				};
				results.Add(await _tutorEducationService.UpdateAsync(req));
			}
			return Ok(ApiResponse<List<TutorEducationDto>>.Ok(results, "Batch verification applied"));
		}

		// Batch verify: certificate
		[Authorize]
		[HttpPut("update-certificate-list/{tutorId}/verify-batch")]
		[ProducesResponseType(typeof(ApiResponse<List<TutorCertificateDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> VerifyCertificateBatch([FromRoute] int tutorId, [FromBody] List<VerifyUpdateRequest> updates)
		{
			if (updates == null || updates.Count == 0)
				return BadRequest(ApiResponse<string>.Fail("No updates provided."));

			var results = new List<TutorCertificateDto>();
			foreach (var u in updates)
			{
				var existing = await _tutorCertificateService.GetByIdFullAsync(u.Id);
				if (existing == null || existing.TutorId != tutorId) continue;
				var req = new TutorCertificateUpdateRequest
				{
					Id = u.Id,
					TutorId = existing.TutorId,
					CertificateTypeId = existing.CertificateTypeId,
					IssueDate = existing.IssueDate,
					ExpiryDate = existing.ExpiryDate,
					Verified = u.Verified,
					RejectReason = u.RejectReason
				};
				results.Add(await _tutorCertificateService.UpdateAsync(req));
			}
			return Ok(ApiResponse<List<TutorCertificateDto>>.Ok(results, "Batch verification applied"));
		}

		// Get all tutors by status
		[Authorize]
		[HttpGet("get-all-tutor-by-status")]
		[ProducesResponseType(typeof(ApiResponse<List<TutorProfileDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetTutorsByStatus([FromQuery] TutorStatus status)
		{
			try
			{
				var all = await _tutorProfileService.GetAllFullAsync();
				var filtered = all.Where(t => t.Status == status).ToList();
				return Ok(ApiResponse<List<TutorProfileDto>>.Ok(filtered));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Failed to get tutors", ex.Message));
			}
		}

		// Get all certificates and educations of a tutor filtered by verify status
		[Authorize]
		[HttpGet("get-tutor/{tutorId}/verifications")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetTutorVerifications([FromRoute] int tutorId, [FromQuery] VerifyStatus status)
		{
			try
			{
				var certs = await _tutorCertificateService.GetByTutorIdAsync(tutorId);
				var edus = await _tutorEducationService.GetByTutorIdAsync(tutorId);
				var certsFiltered = certs.Where(c => c.Verified == status).ToList();
				var edusFiltered = edus.Where(e => e.Verified == status).ToList();
				return Ok(ApiResponse<object>.Ok(new { certificates = certsFiltered, educations = edusFiltered }));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Failed to get tutor verifications", ex.Message));
			}
		}

	}
}
