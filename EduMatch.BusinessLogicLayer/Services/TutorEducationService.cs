using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EduMatch.BusinessLogicLayer.Services
{
	public class TutorEducationService : ITutorEducationService
	{
		private readonly ITutorEducationRepository _repository;
		private readonly IEducationInstitutionRepository _educationInstitutionRepository;
		private readonly ITutorProfileRepository _tutorProfileRepository;
		private readonly IMapper _mapper;
		private readonly ICloudMediaService _cloudMedia;
		private readonly CurrentUserService _currentUserService;

		public TutorEducationService(
			ITutorEducationRepository repository,
			IMapper mapper, ICloudMediaService cloudMedia,
			CurrentUserService currentUserService,
			ITutorProfileRepository tutorProfileRepository,
			IEducationInstitutionRepository educationInstitutionRepository,
			ICloudMediaService cloudMediaService
			)
		{
			_repository = repository;
			_mapper = mapper;
			_cloudMedia = cloudMedia; 
			_currentUserService = currentUserService;
			_tutorProfileRepository = tutorProfileRepository;
			_educationInstitutionRepository = educationInstitutionRepository;
			_cloudMedia = cloudMediaService;
		}

		public async Task<TutorEducationDto?> GetByIdFullAsync(int id)
		{
			var entity = await _repository.GetByIdFullAsync(id);
			return entity != null ? _mapper.Map<TutorEducationDto>(entity) : null;
		}

		public async Task<TutorEducationDto?> GetByTutorIdFullAsync(int tutorId)
		{
			var entity = await _repository.GetByTutorIdFullAsync(tutorId);
			return entity != null ? _mapper.Map<TutorEducationDto>(entity) : null;
		}

		public async Task<IReadOnlyList<TutorEducationDto>> GetByTutorIdAsync(int tutorId)
		{
			var entities = await _repository.GetByTutorIdAsync(tutorId);
			return _mapper.Map<IReadOnlyList<TutorEducationDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorEducationDto>> GetByInstitutionIdAsync(int institutionId)
		{
			var entities = await _repository.GetByInstitutionIdAsync(institutionId);
			return _mapper.Map<IReadOnlyList<TutorEducationDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorEducationDto>> GetByVerifiedStatusAsync(VerifyStatus verified)
		{
			var entities = await _repository.GetByVerifiedStatusAsync(verified);
			return _mapper.Map<IReadOnlyList<TutorEducationDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorEducationDto>> GetPendingVerificationsAsync()
		{
			var entities = await _repository.GetPendingVerificationsAsync();
			return _mapper.Map<IReadOnlyList<TutorEducationDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorEducationDto>> GetAllFullAsync()
		{
			var entities = await _repository.GetAllFullAsync();
			return _mapper.Map<IReadOnlyList<TutorEducationDto>>(entities);
		}

		public async Task<TutorEducationDto> CreateAsync(TutorEducationCreateRequest request)
		{
			try
			{
				
				var tutor = await _tutorProfileRepository.GetByIdFullAsync(request.TutorId);
				if (tutor is null)
					throw new ArgumentException($"Tutor with ID {request.TutorId} not found.");

				
				var institution = await _educationInstitutionRepository.GetByIdAsync(request.InstitutionId);
				if (institution is null)
					throw new ArgumentException($"Education institution with ID {request.InstitutionId} not found.");

				if (_currentUserService.Email is null)
					throw new ArgumentException("Current user email not found.");

				//  UploadToCloudRequest

				string? certUrl = null;
				string? certPublicId = null;
				var hasFile = request.CertificateEducation != null && request.CertificateEducation.Length > 0 && !string.IsNullOrWhiteSpace(request.CertificateEducation.FileName);
				if (hasFile)
				{
					using var stream = request.CertificateEducation!.OpenReadStream();
					var uploadRequest = new UploadToCloudRequest(
						Content: stream,
						FileName: request.CertificateEducation!.FileName,
						ContentType: request.CertificateEducation!.ContentType ?? "application/octet-stream",
						LengthBytes: request.CertificateEducation!.Length,
						OwnerEmail: _currentUserService.Email!,
						MediaType: MediaType.Image
					);
					var uploadResult = await _cloudMedia.UploadAsync(uploadRequest);
					if (!uploadResult.Ok || string.IsNullOrEmpty(uploadResult.SecureUrl))
						throw new InvalidOperationException($"Failed to upload file: {uploadResult.ErrorMessage}");
					certUrl = uploadResult.SecureUrl;
					certPublicId = uploadResult.PublicId;
				}

				// MAP  -> ENTITY
				
				var entity = new TutorEducation
				{
					TutorId = request.TutorId,
					InstitutionId = request.InstitutionId,
					IssueDate = request.IssueDate,
					CertificateUrl = certUrl,
					CertificatePublicId = certPublicId,
					CreatedAt = DateTime.UtcNow,
					Verified = VerifyStatus.Pending,
					RejectReason = null
				};

				await _repository.AddAsync(entity);

				

				return _mapper.Map<TutorEducationDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create tutor education: {ex.Message}", ex);
			}
		}


		public async Task<TutorEducationDto> UpdateAsync(TutorEducationUpdateRequest request)
		{
			try
			{
				

				// Check if entity exists
				var existingEntity = await _repository.GetByIdFullAsync(request.Id);
				if (existingEntity == null)
				{
					throw new ArgumentException($"Tutor education with ID {request.Id} not found");
				}

				var oldPublicId = existingEntity.CertificatePublicId;
				var hasNewFile = request.CertificateEducation != null && request.CertificateEducation.Length > 0 && !string.IsNullOrWhiteSpace(request.CertificateEducation.FileName);
				if (hasNewFile)
				{
					using var stream = request.CertificateEducation!.OpenReadStream();
					var uploadRequest = new UploadToCloudRequest(
						Content: stream,
						FileName: request.CertificateEducation!.FileName,
						ContentType: request.CertificateEducation!.ContentType ?? "application/octet-stream",
						LengthBytes: request.CertificateEducation!.Length,
						OwnerEmail: _currentUserService.Email!,
						MediaType: MediaType.Image
					);
					var uploadResult = await _cloudMedia.UploadAsync(uploadRequest);
					if (!uploadResult.Ok || string.IsNullOrEmpty(uploadResult.SecureUrl))
						throw new InvalidOperationException($"Failed to upload file: {uploadResult.ErrorMessage}");
					existingEntity.CertificateUrl = uploadResult.SecureUrl;
					existingEntity.CertificatePublicId = uploadResult.PublicId;
				}
				existingEntity.TutorId = request.TutorId;
				existingEntity.InstitutionId = request.InstitutionId;

				
				if (request.IssueDate.HasValue)
					existingEntity.IssueDate = request.IssueDate.Value;

				
				var targetVerified = request.Verified ?? existingEntity.Verified;

				// Quản lý RejectReason theo trạng thái đích
				if (targetVerified == VerifyStatus.Rejected)
				{
					// Nếu chuyển/đích là Rejected thì cần RejectReason:
					var reason = request.RejectReason ?? existingEntity.RejectReason;
					if (string.IsNullOrWhiteSpace(reason))
						throw new ArgumentException("Reject reason is required when verification status is Rejected.");
					existingEntity.RejectReason = reason.Trim();
				}
				else
				{
					
					if (request.Verified.HasValue)
					{
						existingEntity.RejectReason = null; 
					}
					
				}

				
				existingEntity.Verified = targetVerified;

				await _repository.UpdateAsync(existingEntity);

				if (hasNewFile && !string.IsNullOrWhiteSpace(oldPublicId))
				{
					_ = _cloudMedia.DeleteByPublicIdAsync(oldPublicId, MediaType.Image)
						.ContinueWith(t =>
						{
							if (t.IsCompletedSuccessfully)
							{
								Console.WriteLine($" Xóa ảnh {oldPublicId} thành công.");
							}
							else if (t.IsFaulted)
							{
								Console.WriteLine($" Xóa ảnh {oldPublicId} thất bại: {t.Exception?.GetBaseException().Message}");
							}
						});
				}


				return _mapper.Map<TutorEducationDto>(existingEntity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update tutor education: {ex.Message}", ex);
			}
		}

		public async Task<List<TutorEducationDto>> CreateBulkAsync(List<TutorEducationCreateRequest> requests)
		{
			try
			{
				var results = new List<TutorEducationDto>();
				foreach (var request in requests)
				{
					var result = await CreateAsync(request);
					results.Add(result);
				}
				return results;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create bulk tutor educations: {ex.Message}", ex);
			}
		}

		public async Task DeleteAsync(int id)
		{
			await _repository.RemoveByIdAsync(id);
		}

		public async Task DeleteByTutorIdAsync(int tutorId)
		{
			await _repository.RemoveByTutorIdAsync(tutorId);
		}
	}
}
