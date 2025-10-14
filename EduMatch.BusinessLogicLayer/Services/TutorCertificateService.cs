using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace EduMatch.BusinessLogicLayer.Services
{
	public class TutorCertificateService : ITutorCertificateService
	{
		private readonly ITutorCertificateRepository _repository;
		private readonly IMapper _mapper;
		private readonly ICloudMediaService _cloudMedia;

		public TutorCertificateService(ITutorCertificateRepository repository, IMapper mapper, ICloudMediaService cloudMedia)
		{
			_repository = repository;
			_mapper = mapper;
			_cloudMedia = cloudMedia;
		}

		public async Task<TutorCertificateDto?> GetByIdFullAsync(int id)
		{
			var entity = await _repository.GetByIdFullAsync(id);
			return entity != null ? _mapper.Map<TutorCertificateDto>(entity) : null;
		}

		public async Task<TutorCertificateDto?> GetByTutorIdFullAsync(int tutorId)
		{
			var entity = await _repository.GetByTutorIdFullAsync(tutorId);
			return entity != null ? _mapper.Map<TutorCertificateDto>(entity) : null;
		}

		public async Task<IReadOnlyList<TutorCertificateDto>> GetByTutorIdAsync(int tutorId)
		{
			var entities = await _repository.GetByTutorIdAsync(tutorId);
			return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorCertificateDto>> GetByCertificateTypeAsync(int certificateTypeId)
		{
			var entities = await _repository.GetByCertificateTypeAsync(certificateTypeId);
			return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorCertificateDto>> GetByVerifiedStatusAsync(VerifyStatus verified)
		{
			var entities = await _repository.GetByVerifiedStatusAsync(verified);
			return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorCertificateDto>> GetExpiredCertificatesAsync()
		{
			var entities = await _repository.GetExpiredCertificatesAsync();
			return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorCertificateDto>> GetExpiringCertificatesAsync(DateTime beforeDate)
		{
			var entities = await _repository.GetExpiringCertificatesAsync(beforeDate);
			return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorCertificateDto>> GetAllFullAsync()
		{
			var entities = await _repository.GetAllFullAsync();
			return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
		}

		public async Task<TutorCertificateDto> CreateAsync(TutorCertificateCreateRequest request)
		{
			try
			{
				// Validate request
				var validationContext = new ValidationContext(request);
				var validationResults = new List<ValidationResult>();
				if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
				{
					throw new ArgumentException($"Validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}");
				}

				var entity = _mapper.Map<TutorCertificate>(request);
				await _repository.AddAsync(entity);
				return _mapper.Map<TutorCertificateDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create tutor certificate: {ex.Message}", ex);
			}
		}

		public async Task<TutorCertificateDto> UpdateAsync(TutorCertificateUpdateRequest request)
		{
			try
			{
				// Validate request
				var validationContext = new ValidationContext(request);
				var validationResults = new List<ValidationResult>();
				if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
				{
					throw new ArgumentException($"Validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}");
				}

				// Check if entity exists
				var existingEntity = await _repository.GetByIdFullAsync(request.Id);
				if (existingEntity == null)
				{
					throw new ArgumentException($"Tutor certificate with ID {request.Id} not found");
				}

				var oldPublicId = existingEntity.CertificatePublicId;
				var entity = _mapper.Map<TutorCertificate>(request);
				await _repository.UpdateAsync(entity);

				if (!string.IsNullOrWhiteSpace(oldPublicId))
				{
					_ = _cloudMedia.DeleteByPublicIdAsync(oldPublicId, MediaType.Image)
						.ContinueWith(t =>
						{
							if (t.IsCompletedSuccessfully)
							{
								Console.WriteLine($" Xóa ?nh {oldPublicId} thành công.");
							}
							else if (t.IsFaulted)
							{
								Console.WriteLine($" Xóa ?nh {oldPublicId} th?t b?i: {t.Exception?.GetBaseException().Message}");
							}
						});
				}

				return _mapper.Map<TutorCertificateDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update tutor certificate: {ex.Message}", ex);
			}
		}

		public async Task<List<TutorCertificateDto>> CreateBulkAsync(List<TutorCertificateCreateRequest> requests)
		{
			try
			{
				var results = new List<TutorCertificateDto>();
				foreach (var request in requests)
				{
					var result = await CreateAsync(request);
					results.Add(result);
				}
				return results;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create bulk tutor certificates: {ex.Message}", ex);
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
