using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.CertificateType;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EduMatch.BusinessLogicLayer.Services
{
	public class CertificateTypeService : ICertificateTypeService
	{
		private readonly ICertificateTypeRepository _repository;
		private readonly IMapper _mapper;

		public CertificateTypeService(ICertificateTypeRepository repository, IMapper mapper)
		{
			_repository = repository;
			_mapper = mapper;
		}

		public async Task<CertificateTypeDto?> GetByIdAsync(int id)
		{
			var entity = await _repository.GetByIdAsync(id);
			return entity != null ? _mapper.Map<CertificateTypeDto>(entity) : null;
		}

		public async Task<CertificateTypeDto?> GetByCodeAsync(string code)
		{
			var entity = await _repository.GetByCodeAsync(code);
			return entity != null ? _mapper.Map<CertificateTypeDto>(entity) : null;
		}

		public async Task<IReadOnlyList<CertificateTypeDto>> GetAllAsync()
		{
			var entities = await _repository.GetAllAsync();
			return _mapper.Map<IReadOnlyList<CertificateTypeDto>>(entities);
		}

		public async Task<IReadOnlyList<CertificateTypeDto>> GetByNameAsync(string name)
		{
			var entities = await _repository.GetByNameAsync(name);
			return _mapper.Map<IReadOnlyList<CertificateTypeDto>>(entities);
		}

		public async Task<CertificateTypeDto> CreateAsync(CertificateTypeCreateRequest request)
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

				// Check if code already exists
				var existingByCode = await _repository.GetByCodeAsync(request.Code);
				if (existingByCode != null)
				{
					throw new ArgumentException($"Certificate type with code '{request.Code}' already exists");
				}

				var entity = _mapper.Map<CertificateType>(request);
				await _repository.AddAsync(entity);
				return _mapper.Map<CertificateTypeDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create certificate type: {ex.Message}", ex);
			}
		}

		public async Task<CertificateTypeDto> UpdateAsync(CertificateTypeUpdateRequest request)
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
					throw new ArgumentException($"Certificate type with ID {request.Id} not found");
				}

				// Check if code already exists (excluding current entity)
				var existingByCode = await _repository.GetByCodeAsync(request.Code);
				if (existingByCode != null && existingByCode.Id != request.Id)
				{
					throw new ArgumentException($"Certificate type with code '{request.Code}' already exists");
				}

				var entity = _mapper.Map<CertificateType>(request);
				await _repository.UpdateAsync(entity);
				return _mapper.Map<CertificateTypeDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update certificate type: {ex.Message}", ex);
			}
		}

		public async Task DeleteAsync(int id)
		{
			await _repository.RemoveByIdAsync(id);
		}
	}
}
