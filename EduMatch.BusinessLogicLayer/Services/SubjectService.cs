using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EduMatch.BusinessLogicLayer.Services
{
	public class SubjectService : ISubjectService
	{
		private readonly ISubjectRepository _repository;
		private readonly IMapper _mapper;

		public SubjectService(ISubjectRepository repository, IMapper mapper)
		{
			_repository = repository;
			_mapper = mapper;
		}

		public async Task<SubjectDto?> GetByIdAsync(int id)
		{
			var entity = await _repository.GetByIdAsync(id);
			return entity != null ? _mapper.Map<SubjectDto>(entity) : null;
		}

		public async Task<IReadOnlyList<SubjectDto>> GetAllAsync()
		{
			var entities = await _repository.GetAllAsync();
			return _mapper.Map<IReadOnlyList<SubjectDto>>(entities);
		}

		public async Task<IReadOnlyList<SubjectDto>> GetByNameAsync(string name)
		{
			var entities = await _repository.GetByNameAsync(name);
			return _mapper.Map<IReadOnlyList<SubjectDto>>(entities);
		}

		public async Task<SubjectDto> CreateAsync(SubjectCreateRequest request)
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

				var entity = _mapper.Map<Subject>(request);
				await _repository.AddAsync(entity);
				return _mapper.Map<SubjectDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create subject: {ex.Message}", ex);
			}
		}

		public async Task<SubjectDto> UpdateAsync(SubjectUpdateRequest request)
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
				var existingEntity = await _repository.GetByIdAsync(request.Id);
				if (existingEntity == null)
				{
					throw new ArgumentException($"Subject with ID {request.Id} not found");
				}

				var entity = _mapper.Map<Subject>(request);
				await _repository.UpdateAsync(entity);
				return _mapper.Map<SubjectDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update subject: {ex.Message}", ex);
			}
		}

		public async Task DeleteAsync(int id)
		{
			await _repository.RemoveByIdAsync(id);
		}
	}
}
