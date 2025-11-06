using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.EducationInstitution;
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
		private readonly IEducationInstitutionRepository _educationInstitutionRepository;
		private readonly IMapper _mapper;

		public EducationInstitutionService(IEducationInstitutionRepository repository, IMapper mapper)
		{
			_educationInstitutionRepository = repository;
			_mapper = mapper;
		}

		/// <summary>
		/// Lấy EducationInstitution theo ID
		/// </summary>
		public async Task<EducationInstitutionDto?> GetByIdAsync(int id)
		{
			try
			{
			var entity = await _educationInstitutionRepository.GetByIdAsync(id);
				return entity != null ? _mapper.Map<EducationInstitutionDto>(entity) : null;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to get education institution by ID: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Lấy EducationInstitution theo Code
		/// </summary>
		public async Task<EducationInstitutionDto?> GetByCodeAsync(string code)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(code))
					throw new ArgumentException("Code cannot be null or empty");

			var entity = await _educationInstitutionRepository.GetByCodeAsync(code);
				return entity != null ? _mapper.Map<EducationInstitutionDto>(entity) : null;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to get education institution by code: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Lấy tất cả EducationInstitution
		/// </summary>
		public async Task<IReadOnlyList<EducationInstitutionDto>> GetAllAsync()
		{
			try
			{
			var entities = await _educationInstitutionRepository.GetAllAsync();
				return _mapper.Map<IReadOnlyList<EducationInstitutionDto>>(entities);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to get all education institutions: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Tìm EducationInstitution theo tên
		/// </summary>
		public async Task<IReadOnlyList<EducationInstitutionDto>> GetByNameAsync(string name)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(name))
					throw new ArgumentException("Name cannot be null or empty");

			var entities = await _educationInstitutionRepository.GetByNameAsync(name);
				return _mapper.Map<IReadOnlyList<EducationInstitutionDto>>(entities);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to get education institutions by name: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Lấy EducationInstitution theo loại trường
		/// </summary>
		public async Task<IReadOnlyList<EducationInstitutionDto>> GetByInstitutionTypeAsync(InstitutionType institutionType)
		{
			try
			{
			var entities = await _educationInstitutionRepository.GetByInstitutionTypeAsync(institutionType);
				return _mapper.Map<IReadOnlyList<EducationInstitutionDto>>(entities);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to get education institutions by type: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Tạo EducationInstitution mới
		/// </summary>
		public async Task<EducationInstitutionDto> CreateAsync(EducationInstitutionCreateRequest request)
		{
			try
			{
				if (request == null)
					throw new ArgumentException("Request cannot be null");

				if (string.IsNullOrWhiteSpace(request.Code))
					throw new ArgumentException("Code is required");
				if (string.IsNullOrWhiteSpace(request.Name))
					throw new ArgumentException("Name is required");

				// Check if code already exists
				var existingByCode = await _educationInstitutionRepository.GetByCodeAsync(request.Code);
				if (existingByCode != null)
				{
					throw new ArgumentException($"Education institution with code '{request.Code}' already exists");
				}

				var entity = new EducationInstitution
				{
					Code = request.Code,
					Name = request.Name,
					InstitutionType = request.InstitutionType.HasValue ? (int)request.InstitutionType.Value : 0,
					CreatedAt = DateTime.UtcNow,
					Verified = (int)VerifyStatus.Pending
				};

				await _educationInstitutionRepository.AddAsync(entity);
				return _mapper.Map<EducationInstitutionDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create education institution: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Cập nhật EducationInstitution
		/// </summary>
		public async Task<EducationInstitutionDto> UpdateAsync(EducationInstitutionUpdateRequest request)
		{
			try
			{
				if (request == null)
					throw new ArgumentException("Request cannot be null");

				if (request.Id <= 0)
					throw new ArgumentException("ID must be greater than 0");
				if (string.IsNullOrWhiteSpace(request.Code))
					throw new ArgumentException("Code is required");
				if (string.IsNullOrWhiteSpace(request.Name))
					throw new ArgumentException("Name is required");

				// Check if entity exists
				var existingEntity = await _educationInstitutionRepository.GetByIdAsync(request.Id);
				if (existingEntity == null)
				{
					throw new ArgumentException($"Education institution with ID {request.Id} not found");
				}

				// Check if code already exists (excluding current entity)
				var existingByCode = await _educationInstitutionRepository.GetByCodeAsync(request.Code);
				if (existingByCode != null && existingByCode.Id != request.Id)
				{
					throw new ArgumentException($"Education institution with code '{request.Code}' already exists");
				}

				// Update only provided fields
				existingEntity.Code = request.Code;
				existingEntity.Name = request.Name;
				if (request.InstitutionType.HasValue)
					existingEntity.InstitutionType = (int)request.InstitutionType.Value;

				await _educationInstitutionRepository.UpdateAsync(existingEntity);
				return _mapper.Map<EducationInstitutionDto>(existingEntity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update education institution: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Xóa EducationInstitution theo ID
		/// </summary>
		public async Task DeleteAsync(int id)
		{
			try
			{
				if (id <= 0)
					throw new ArgumentException("ID must be greater than 0");

				// Check if entity exists
			var existingEntity = await _educationInstitutionRepository.GetByIdAsync(id);
				if (existingEntity == null)
				{
					throw new ArgumentException($"Education institution with ID {id} not found");
				}

			await _educationInstitutionRepository.RemoveByIdAsync(id);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to delete education institution: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Xác thực EducationInstitution
		/// </summary>
		public async Task<EducationInstitutionDto> VerifyAsync(int id, string verifiedBy)
		{
			try
			{
				if (id <= 0)
					throw new ArgumentException("ID must be greater than 0");

				if (string.IsNullOrWhiteSpace(verifiedBy))
					throw new ArgumentException("VerifiedBy is required");

				// Check if entity exists
			var existingEntity = await _educationInstitutionRepository.GetByIdAsync(id);
				if (existingEntity == null)
				{
					throw new ArgumentException($"Education institution with ID {id} not found");
				}

				// Check if current status is Pending
				if (existingEntity.Verified != (int)VerifyStatus.Pending)
				{
					throw new InvalidOperationException($"Education institution with ID {id} is not in Pending status for verification");
				}

				// Update verification status
				existingEntity.Verified = (int)VerifyStatus.Verified;
				existingEntity.VerifiedBy = verifiedBy;
				existingEntity.VerifiedAt = DateTime.UtcNow;

			await _educationInstitutionRepository.UpdateAsync(existingEntity);
				return _mapper.Map<EducationInstitutionDto>(existingEntity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to verify education institution: {ex.Message}", ex);
			}
		}
	}
}
