using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
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
	public class EducationInstitutionService : IEducationInstitutionService
	{
		private readonly IEducationInstitutionRepository _repository;
		private readonly IMapper _mapper;

		public EducationInstitutionService(IEducationInstitutionRepository repository, IMapper mapper)
		{
			_repository = repository;
			_mapper = mapper;
		}

		public async Task<EducationInstitutionDto?> GetByIdAsync(int id)
		{
			try
			{
				var entity = await _repository.GetByIdAsync(id);
				return entity != null ? _mapper.Map<EducationInstitutionDto>(entity) : null;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to get education institution by ID: {ex.Message}", ex);
			}
		}

		public async Task<EducationInstitutionDto?> GetByCodeAsync(string code)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(code))
					throw new ArgumentException("Code cannot be null or empty");

				var entity = await _repository.GetByCodeAsync(code);
				return entity != null ? _mapper.Map<EducationInstitutionDto>(entity) : null;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to get education institution by code: {ex.Message}", ex);
			}
		}

		public async Task<IReadOnlyList<EducationInstitutionDto>> GetAllAsync()
		{
			try
			{
				var entities = await _repository.GetAllAsync();
				return _mapper.Map<IReadOnlyList<EducationInstitutionDto>>(entities);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to get all education institutions: {ex.Message}", ex);
			}
		}

		public async Task<IReadOnlyList<EducationInstitutionDto>> GetByNameAsync(string name)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(name))
					throw new ArgumentException("Name cannot be null or empty");

				var entities = await _repository.GetByNameAsync(name);
				return _mapper.Map<IReadOnlyList<EducationInstitutionDto>>(entities);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to get education institutions by name: {ex.Message}", ex);
			}
		}

		public async Task<IReadOnlyList<EducationInstitutionDto>> GetByInstitutionTypeAsync(InstitutionType institutionType)
		{
			try
			{
				var entities = await _repository.GetByInstitutionTypeAsync(institutionType);
				return _mapper.Map<IReadOnlyList<EducationInstitutionDto>>(entities);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to get education institutions by type: {ex.Message}", ex);
			}
		}

		public async Task<EducationInstitutionDto> CreateAsync(string code, string name, InstitutionType? institutionType = null)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(code))
					throw new ArgumentException("Code is required");
				if (string.IsNullOrWhiteSpace(name))
					throw new ArgumentException("Name is required");

				// Check if code already exists
				var existingByCode = await _repository.GetByCodeAsync(code);
				if (existingByCode != null)
				{
					throw new ArgumentException($"Education institution with code '{code}' already exists");
				}

				var entity = new EducationInstitution
				{
					Code = code,
					Name = name,
					InstitutionType = institutionType,
					CreatedAt = DateTime.UtcNow
				};

				await _repository.AddAsync(entity);
				return _mapper.Map<EducationInstitutionDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create education institution: {ex.Message}", ex);
			}
		}

		public async Task<EducationInstitutionDto> UpdateAsync(int id, string code, string name, InstitutionType? institutionType = null)
		{
			try
			{
				if (id <= 0)
					throw new ArgumentException("ID must be greater than 0");
				if (string.IsNullOrWhiteSpace(code))
					throw new ArgumentException("Code is required");
				if (string.IsNullOrWhiteSpace(name))
					throw new ArgumentException("Name is required");

				// Check if entity exists
				var existingEntity = await _repository.GetByIdAsync(id);
				if (existingEntity == null)
				{
					throw new ArgumentException($"Education institution with ID {id} not found");
				}

				// Check if code already exists (excluding current entity)
				var existingByCode = await _repository.GetByCodeAsync(code);
				if (existingByCode != null && existingByCode.Id != id)
				{
					throw new ArgumentException($"Education institution with code '{code}' already exists");
				}

				// Update only provided fields
				if (!string.IsNullOrWhiteSpace(code))
					existingEntity.Code = code;
				if (!string.IsNullOrWhiteSpace(name))
					existingEntity.Name = name;
				if (institutionType.HasValue)
					existingEntity.InstitutionType = institutionType;

				await _repository.UpdateAsync(existingEntity);
				return _mapper.Map<EducationInstitutionDto>(existingEntity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update education institution: {ex.Message}", ex);
			}
		}

		public async Task DeleteAsync(int id)
		{
			try
			{
				if (id <= 0)
					throw new ArgumentException("ID must be greater than 0");

				// Check if entity exists
				var existingEntity = await _repository.GetByIdAsync(id);
				if (existingEntity == null)
				{
					throw new ArgumentException($"Education institution with ID {id} not found");
				}

				await _repository.RemoveByIdAsync(id);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to delete education institution: {ex.Message}", ex);
			}
		}
	}
}
