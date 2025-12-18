using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.TutorProfile;
using EduMatch.BusinessLogicLayer.Requests.TutorCertificate;
using EduMatch.BusinessLogicLayer.Requests.TutorEducation;
using EduMatch.BusinessLogicLayer.Requests.TutorSubject;
using EduMatch.BusinessLogicLayer.Requests.TutorAvailability;
using EduMatch.BusinessLogicLayer.Requests.User;
using EduMatch.BusinessLogicLayer.Requests.TutorVerificationRequest;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.PresentationLayer.Common;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using EduMatch.BusinessLogicLayer.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
		private readonly IUserService _userService;
		private readonly ITutorVerificationRequestService _tutorVerificationRequestService;
		private readonly ITutorRatingSummaryService _summaryService;
		private readonly INotificationService _notificationService;
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
			EmailService emailService,
			IUserService userService,
			ITutorVerificationRequestService tutorVerificationRequestService,
			ITutorRatingSummaryService summaryService,
			INotificationService notificationService
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
			_userService = userService;
			_tutorVerificationRequestService = tutorVerificationRequestService;
			_summaryService = summaryService;
			_notificationService = notificationService;
		}


		
		/// <summary>
		/// Đăng ký trở thành gia sư với đầy đủ thông tin profile, education, certificate, subject và availability
		/// </summary>
		[HttpPost("become-tutor")]
		[ProducesResponseType(typeof(ApiResponse<TutorProfileDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> BecomeTutor([FromBody] BecomeTutorRequest request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ApiResponse<string>.Fail("Invalid request."));

			if (request.TutorProfile == null)
				return BadRequest(ApiResponse<string>.Fail("Tutor profile is required."));

			var userEmail = request.TutorProfile.UserEmail;
			if (string.IsNullOrWhiteSpace(userEmail))
				return BadRequest(ApiResponse<string>.Fail("User email is required in request."));

				request.Educations ??= new List<TutorEducationCreateRequest>();
				request.Certificates ??= new List<TutorCertificateCreateRequest>();
				request.Subjects ??= new List<TutorSubjectCreateRequest>();
				request.Availabilities ??= new List<TutorAvailabilityCreateRequest>();

				await using var tx = await _eduMatch.Database.BeginTransactionAsync();
				try
				{
					// Tạo profile (sử dụng transaction từ controller)
					var profileDto = await _tutorProfileService.CreateAsync(request.TutorProfile);
					var tutorId = profileDto.Id;

					await _summaryService.AddRatingSummary(tutorId);
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

					if (request.Availabilities.Any())
					{
						foreach (var s in request.Availabilities) s.TutorId = tutorId;
						await _tutorAvailabilityService.CreateBulkAsync(request.Availabilities);
					}

					await _emailService.SendBecomeTutorWelcomeAsync(userEmail);

					// Cấp role Tutor ngay khi đăng ký become-tutor
					await _userService.UpdateRoleUserAsync(userEmail, 2);

					// Gửi thông báo đăng ký trở thành gia sư thành công
					await _notificationService.CreateNotificationAsync(
						userEmail,
						"Đăng ký trở thành gia sư thành công! Hồ sơ của bạn đang chờ được phê duyệt.",
						$"/tutor/profile/{tutorId}"
					);

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
	


        /// <summary>
        /// Xác thực hàng loạt các bằng cấp học vấn của gia sư
        /// </summary>
        [Authorize(Roles = Roles.BusinessAdmin)]
		[HttpPut("verify-list-education/{tutorId}")]
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

		/// <summary>
		/// Xác thực hàng loạt các chứng chỉ của gia sư
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin )]
		[HttpPut("verify-list-certificate/{tutorId}")]
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





		
		/// <summary>
		/// Lấy danh sách gia sư theo trạng thái (Pending, Approved, Rejected)
		/// </summary>
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

		/// <summary>
		/// Lấy danh sách tất cả gia sư trong hệ thống
		/// </summary>
		[HttpGet("get-all-tutor")]
		[ProducesResponseType(typeof(ApiResponse<List<TutorProfileDto>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetTutorsAll()
		{
			try
			{
				var all = await _tutorProfileService.GetAllFullAsync();
				var allList = all.ToList();
				return Ok(ApiResponse<List<TutorProfileDto>>.Ok(allList));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Failed to get tutors", ex.Message));
			}
		}

		/// <summary>
		/// Lấy thông tin gia sư theo Email. Trả lỗi nếu không tìm thấy hoặc email chưa đăng ký gia sư
		/// </summary>
		[HttpGet("get-tutor-by-email")]
		[ProducesResponseType(typeof(ApiResponse<TutorProfileDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetTutorByEmail([FromQuery] string email)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(email))
					return BadRequest(ApiResponse<string>.Fail("Email không hợp lệ."));
				if (!new EmailAddressAttribute().IsValid(email))
					return BadRequest(ApiResponse<string>.Fail("Email không đúng định dạng."));

				var tutor = await _tutorProfileService.GetByEmailFullAsync(email);
				if (tutor == null)
					return NotFound(ApiResponse<string>.Fail("Không tìm thấy gia sư hoặc bạn chưa phải là gia sư."));

				return Ok(ApiResponse<TutorProfileDto>.Ok(tutor));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Lỗi khi lấy thông tin gia sư.", ex.Message));
			}
		}

		/// <summary>
		/// Lấy tất cả chứng chỉ và bằng cấp học vấn của một gia sư
		/// </summary>
		[HttpGet("get-all-tutor-certificate-education/{tutorId}")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetTutorVerifications([FromRoute] int tutorId)
		{
			try
			{
				var certs = await _tutorCertificateService.GetByTutorIdAsync(tutorId);
				var edus = await _tutorEducationService.GetByTutorIdAsync(tutorId);
				var certsFiltered = certs.ToList();
				var edusFiltered = edus.ToList();
				return Ok(ApiResponse<object>.Ok(new { certificates = certsFiltered, educations = edusFiltered }));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Failed to get tutor verifications", ex.Message));
			}
		}



		
		/// <summary>
		/// Lấy thông tin chi tiết của một gia sư theo ID
		/// </summary>
		[HttpGet("get-tutor-by-id/{tutorId}")]
		[ProducesResponseType(typeof(ApiResponse<TutorProfileDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> GetTutorById([FromRoute] int tutorId)
		{
			try
			{
				if (tutorId <= 0)
					return BadRequest(ApiResponse<string>.Fail("Invalid tutor ID."));

				var tutor = await _tutorProfileService.GetByIdFullAsync(tutorId);
				if (tutor == null)
					return NotFound(ApiResponse<string>.Fail($"Tutor with ID {tutorId} not found."));

				return Ok(ApiResponse<TutorProfileDto>.Ok(tutor));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Failed to get tutor profile.", ex.Message));
			}
		}


        /// <summary>
		/// Cập nhật thông cơ bản (không có chứng chỉ, bằng cấp học vấn, status) tin gia sư
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor + "," + Roles.Learner)]
		[HttpPut("update-tutor-profile")]
		[ProducesResponseType(typeof(ApiResponse<TutorProfileDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> UpdateTutorProfile([FromBody] TutorProfileUpdateRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(ApiResponse<string>.Fail("Invalid request."));

				var updatedProfile = await _tutorProfileService.UpdateAsync(request);
				return Ok(ApiResponse<TutorProfileDto>.Ok(updatedProfile, "Tutor profile updated successfully."));
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ApiResponse<string>.Fail(ex.Message));
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ApiResponse<string>.Fail(ex.Message));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Failed to update tutor profile.", ex.Message));
			}
		}

		/// <summary>
		/// Phê duyệt gia sư và xác thực tất cả chứng chỉ, bằng cấp của gia sư
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin )]
		[HttpPut("approve-and-verify-all/{tutorId}")]
		[ProducesResponseType(typeof(ApiResponse<TutorProfileDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> ApproveAndVerifyAll([FromRoute] int tutorId)
		{
			if (tutorId <= 0)
				return BadRequest(ApiResponse<string>.Fail("Invalid tutor ID."));

			// Check if tutor exists using service layer
			var existingProfile = await _tutorProfileService.GetByIdFullAsync(tutorId);
			if (existingProfile == null)
				return NotFound(ApiResponse<string>.Fail($"Tutor with ID {tutorId} not found."));

			// Sử dụng transaction cho toàn bộ hàm
			using var dbTransaction = await _eduMatch.Database.BeginTransactionAsync();
			try
			{
				var currentUserEmail = _currentUserService.Email ?? "System";

				// Đầu tiên: Update TutorVerificationRequest status thành Approved
				var verificationRequests = await _tutorVerificationRequestService.GetAllByEmailOrTutorIdAsync(
					email: null, 
					tutorId: tutorId, 
					status: null);
				
				if (verificationRequests != null && verificationRequests.Any())
				{
					// Lấy request mới nhất (Pending) để approve
					var pendingRequest = verificationRequests
						.Where(r => r.Status == TutorVerificationRequestStatus.Pending)
						.OrderByDescending(r => r.CreatedAt)
						.FirstOrDefault();
					
					if (pendingRequest != null)
					{
						await _tutorVerificationRequestService.UpdateStatusAsync(
							pendingRequest.Id, 
							TutorVerificationRequestStatus.Approved);
					}
				}

				// Verify all certificates using service layer
				var certs = await _tutorCertificateService.GetByTutorIdAsync(tutorId);
				foreach (var c in certs)
				{
					await _tutorCertificateService.UpdateAsync(new TutorCertificateUpdateRequest
					{
						Id = c.Id,
						TutorId = c.TutorId,
						CertificateTypeId = c.CertificateTypeId,
						IssueDate = c.IssueDate,
						ExpiryDate = c.ExpiryDate,
						Verified = VerifyStatus.Verified,
						RejectReason = null
					});
				}

				// Verify all educations using service layer
				var edus = await _tutorEducationService.GetByTutorIdAsync(tutorId);
				foreach (var e in edus)
				{
					await _tutorEducationService.UpdateAsync(new TutorEducationUpdateRequest
					{
						Id = e.Id,
						TutorId = e.TutorId,
						InstitutionId = e.InstitutionId,
						IssueDate = e.IssueDate,
						Verified = VerifyStatus.Verified,
						RejectReason = null
					});
				}

				// Commit transaction
				await dbTransaction.CommitAsync();

				// Gửi thông báo phê duyệt gia sư thành công
				await _notificationService.CreateNotificationAsync(
					existingProfile.UserEmail,
					"Chúc mừng! Hồ sơ gia sư của bạn đã được phê duyệt thành công. Tất cả chứng chỉ và bằng cấp đã được xác thực.",
					$"/tutor/profile/{tutorId}"
				);

				// Gửi email thông báo phê duyệt đơn đăng ký gia sư
				
					try
					{
						await _emailService.SendTutorApplicationResultAsync(
							existingProfile.UserEmail,
							isApproved: true);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[TutorsController] Error sending email: {ex.Message}");
					}
				

				var fullProfile = await _tutorProfileService.GetByIdFullAsync(tutorId);
				return Ok(ApiResponse<TutorProfileDto>.Ok(fullProfile!, $"Tutor approved and all certificates/educations verified by {currentUserEmail}."));
			}
			catch (Exception ex)
			{
				// Rollback transaction nếu có lỗi
				await dbTransaction.RollbackAsync();
				return BadRequest(ApiResponse<string>.Fail("Failed to approve and verify all."));

			}
		}

		/// <summary>
		/// Từ chối gia sư và reject tất cả chứng chỉ, bằng cấp của gia sư
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin)]
		[HttpPut("reject-all/{tutorId}")]
		[ProducesResponseType(typeof(ApiResponse<TutorProfileDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> RejectAll([FromRoute] int tutorId, [FromBody] RejectTutorRequest request)
		{
			if (tutorId <= 0)
				return BadRequest(ApiResponse<string>.Fail("Invalid tutor ID."));

			if (request == null || string.IsNullOrWhiteSpace(request.Reason))
				return BadRequest(ApiResponse<string>.Fail("Reason is required."));

			// Check if tutor exists using service layer
			var existingProfile = await _tutorProfileService.GetByIdFullAsync(tutorId);
			if (existingProfile == null)
				return NotFound(ApiResponse<string>.Fail($"Tutor with ID {tutorId} not found."));

			// Sử dụng transaction cho toàn bộ hàm
			using var dbTransaction = await _eduMatch.Database.BeginTransactionAsync();
			try
			{
				var currentUserEmail = _currentUserService.Email ?? "System";

				// Đầu tiên: Update TutorVerificationRequest status thành Rejected với reason
				var verificationRequests = await _tutorVerificationRequestService.GetAllByEmailOrTutorIdAsync(
					email: null,
					tutorId: tutorId,
					status: null);

				if (verificationRequests != null && verificationRequests.Any())
				{
					// Lấy request mới nhất (Pending) để reject
					var pendingRequest = verificationRequests
						.Where(r => r.Status == TutorVerificationRequestStatus.Pending)
						.OrderByDescending(r => r.CreatedAt)
						.FirstOrDefault();

					if (pendingRequest != null)
					{
						// Update status và admin note (reason)
						await _tutorVerificationRequestService.UpdateAsync(new TutorVerificationRequestUpdateRequest
						{
							Id = pendingRequest.Id,
							AdminNote = request.Reason
						});
						await _tutorVerificationRequestService.UpdateStatusAsync(
							pendingRequest.Id,
							TutorVerificationRequestStatus.Rejected);
					}
				}

				// Reject all certificates using service layer
				var certs = await _tutorCertificateService.GetByTutorIdAsync(tutorId);
				foreach (var c in certs)
				{
					await _tutorCertificateService.UpdateAsync(new TutorCertificateUpdateRequest
					{
						Id = c.Id,
						TutorId = c.TutorId,
						CertificateTypeId = c.CertificateTypeId,
						IssueDate = c.IssueDate,
						ExpiryDate = c.ExpiryDate,
						Verified = VerifyStatus.Rejected,
						RejectReason = request.Reason
					});
				}

				// Reject all educations using service layer
				var edus = await _tutorEducationService.GetByTutorIdAsync(tutorId);
				foreach (var e in edus)
				{
					await _tutorEducationService.UpdateAsync(new TutorEducationUpdateRequest
					{
						Id = e.Id,
						TutorId = e.TutorId,
						InstitutionId = e.InstitutionId,
						IssueDate = e.IssueDate,
						Verified = VerifyStatus.Rejected,
						RejectReason = request.Reason
					});
				}

				// Commit transaction
				await dbTransaction.CommitAsync();

				// Gửi thông báo từ chối gia sư
				await _notificationService.CreateNotificationAsync(
					existingProfile.UserEmail,
					$"Rất tiếc, hồ sơ gia sư của bạn đã bị từ chối. Lý do: {request.Reason}. Vui lòng kiểm tra và cập nhật hồ sơ để đăng ký lại.",
					$"/tutor/profile/{tutorId}"
				);

				// Gửi email thông báo từ chối đơn đăng ký gia sư
				
					try
					{
						await _emailService.SendTutorApplicationResultAsync(
							existingProfile.UserEmail,
							isApproved: false,
							rejectionReason: request.Reason);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[TutorsController] Error sending email: {ex.Message}");
					}
				

				var fullProfile = await _tutorProfileService.GetByIdFullAsync(tutorId);
				return Ok(ApiResponse<TutorProfileDto>.Ok(fullProfile!, $"Tutor rejected and all certificates/educations rejected by {currentUserEmail}. Reason: {request.Reason}"));
			}
			catch (Exception ex)
			{
				// Rollback transaction nếu có lỗi
				await dbTransaction.RollbackAsync();
				return BadRequest(ApiResponse<string>.Fail("Failed to reject tutor.", ex.Message));
			}
		}

		/// <summary>
		/// Cập nhật trạng thái của gia sư (chỉ cho phép từ Approved sang Suspended hoặc Deactivated)
		/// </summary>
		[Authorize(Roles = Roles.BusinessAdmin + "," + Roles.Tutor)]
		[HttpPut("update-tutor-status/{tutorId}")]
		[ProducesResponseType(typeof(ApiResponse<TutorProfileDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> UpdateTutorStatus([FromRoute] int tutorId, [FromBody] UpdateTutorStatusRequest request)
		{
			if (tutorId <= 0)
				return BadRequest(ApiResponse<string>.Fail("Invalid tutor ID."));

			// Check if tutor exists using service layer
			var existingProfile = await _tutorProfileService.GetByIdFullAsync(tutorId);
			if (existingProfile == null)
				return NotFound(ApiResponse<string>.Fail($"Tutor with ID {tutorId} not found."));

			try
			{
				// Chỉ cho phép update từ Approved sang Suspended hoặc Deactivated
				// Approved và Rejected chỉ được thay đổi khi được xác nhận thành gia sư hoặc từ chối (qua TutorVerificationRequest)
				var updatedProfile = await _tutorProfileService.UpdateStatusAsync(tutorId, request.Status);
				return Ok(ApiResponse<TutorProfileDto>.Ok(updatedProfile, $"Trạng thái gia sư đã được cập nhật sang {request.Status}."));
			}
			catch (Exception ex)
			{
				return BadRequest(ApiResponse<string>.Fail("Không thể cập nhật trạng thái gia sư.", ex.Message));
			}
		}








	
	}
}