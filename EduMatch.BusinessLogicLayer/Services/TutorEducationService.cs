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
		private readonly IMapper _mapper;

		public TutorEducationService(ITutorEducationRepository repository, IMapper mapper)
		{
			_repository = repository;
			_mapper = mapper;
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
				// Validate request
				var validationContext = new ValidationContext(request);
				var validationResults = new List<ValidationResult>();
				if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
				{
					throw new ArgumentException($"Validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}");
				}

				var entity = _mapper.Map<TutorEducation>(request);
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
					throw new ArgumentException($"Tutor education with ID {request.Id} not found");
				}

				var entity = _mapper.Map<TutorEducation>(request);
				await _repository.UpdateAsync(entity);
				return _mapper.Map<TutorEducationDto>(entity);
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
