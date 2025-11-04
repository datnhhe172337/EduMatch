using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.CertificateType;
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
	public class CertificateTypeService : ICertificateTypeService
	{
		private readonly ICertificateTypeRepository _repository;
		private readonly ICertificateTypeSubjectRepository _certificateTypeSubjectRepository;
		private readonly IMapper _mapper;

		public CertificateTypeService(ICertificateTypeRepository repository, ICertificateTypeSubjectRepository certificateTypeSubjectRepository, IMapper mapper)
		{
			_repository = repository;
			_certificateTypeSubjectRepository = certificateTypeSubjectRepository;
			_mapper = mapper;
		}

		/// <summary>
		/// Lấy CertificateType theo ID
		/// </summary>
		public async Task<CertificateTypeDto?> GetByIdAsync(int id)
		{
			var entity = await _repository.GetByIdAsync(id);
			return entity != null ? _mapper.Map<CertificateTypeDto>(entity) : null;
		}

		/// <summary>
		/// Lấy CertificateType theo Code
		/// </summary>
		public async Task<CertificateTypeDto?> GetByCodeAsync(string code)
		{
			var entity = await _repository.GetByCodeAsync(code);
			return entity != null ? _mapper.Map<CertificateTypeDto>(entity) : null;
		}

		/// <summary>
		/// Lấy tất cả CertificateType
		/// </summary>
		public async Task<IReadOnlyList<CertificateTypeDto>> GetAllAsync()
		{
			var entities = await _repository.GetAllAsync();
			return _mapper.Map<IReadOnlyList<CertificateTypeDto>>(entities);
		}

		/// <summary>
		/// Tìm CertificateType theo tên
		/// </summary>
		public async Task<IReadOnlyList<CertificateTypeDto>> GetByNameAsync(string name)
		{
			var entities = await _repository.GetByNameAsync(name);
			return _mapper.Map<IReadOnlyList<CertificateTypeDto>>(entities);
		}

		/// <summary>
		/// Tạo CertificateType mới
		/// </summary>
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

				var entity = new CertificateType
				{
					Code = request.Code,
					Name = request.Name,
					CreatedAt = DateTime.UtcNow,
					Verified = (int)VerifyStatus.Pending
				};
				await _repository.AddAsync(entity);
				return _mapper.Map<CertificateTypeDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create certificate type: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Cập nhật CertificateType
		/// </summary>
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

				// Update only provided fields
				existingEntity.Code = request.Code;
				existingEntity.Name = request.Name;

				await _repository.UpdateAsync(existingEntity);
				return _mapper.Map<CertificateTypeDto>(existingEntity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update certificate type: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Xóa CertificateType theo ID
		/// </summary>
		public async Task DeleteAsync(int id)
		{
			await _repository.RemoveByIdAsync(id);
		}

		/// <summary>
		/// Xác thực CertificateType
		/// </summary>
		public async Task<CertificateTypeDto> VerifyAsync(int id, string verifiedBy)
		{
			try
			{
				if (id <= 0)
					throw new ArgumentException("ID must be greater than 0");

				if (string.IsNullOrWhiteSpace(verifiedBy))
					throw new ArgumentException("VerifiedBy is required");

				// Check if entity exists
				var existingEntity = await _repository.GetByIdAsync(id);
				if (existingEntity == null)
				{
					throw new ArgumentException($"Certificate type with ID {id} not found");
				}

				// Check if current status is Pending
				if (existingEntity.Verified != (int)VerifyStatus.Pending)
				{
					throw new InvalidOperationException($"Certificate type with ID {id} is not in Pending status for verification");
				}

				// Update verification status
				existingEntity.Verified = (int)VerifyStatus.Verified;
				existingEntity.VerifiedBy = verifiedBy;
				existingEntity.VerifiedAt = DateTime.UtcNow;

				await _repository.UpdateAsync(existingEntity);
				return _mapper.Map<CertificateTypeDto>(existingEntity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to verify certificate type: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Thêm danh sách Subject vào CertificateType
		/// </summary>
		public async Task<CertificateTypeDto> AddSubjectsToCertificateTypeAsync(int certificateTypeId, List<int> subjectIds)
		{
			try
			{
				if (certificateTypeId <= 0)
					throw new ArgumentException("Certificate type ID must be greater than 0");

				if (subjectIds == null || !subjectIds.Any())
					throw new ArgumentException("Subject IDs list cannot be null or empty");

				// Check if certificate type exists
				var certificateType = await _repository.GetByIdAsync(certificateTypeId);
				if (certificateType == null)
					throw new ArgumentException($"Certificate type with ID {certificateTypeId} not found");

				// Get existing relationships to avoid duplicates
				var existingRelationships = await _certificateTypeSubjectRepository.GetByCertificateTypeIdAsync(certificateTypeId);
				var existingSubjectIds = existingRelationships.Select(r => r.SubjectId).ToList();

				// Filter out subjects that are already associated
				var newSubjectIds = subjectIds.Where(id => !existingSubjectIds.Contains(id)).ToList();

				if (newSubjectIds.Any())
				{
					// Create new relationships only for subjects that aren't already associated
					var certificateTypeSubjects = newSubjectIds.Select(subjectId => new CertificateTypeSubject
					{
						CertificateTypeId = certificateTypeId,
						SubjectId = subjectId
					}).ToList();

					await _certificateTypeSubjectRepository.AddRangeAsync(certificateTypeSubjects);
				}

				// Return updated certificate type with all relationships
				var updatedCertificateType = await _repository.GetByIdAsync(certificateTypeId);
				return _mapper.Map<CertificateTypeDto>(updatedCertificateType);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to add subjects to certificate type: {ex.Message}", ex);
			}
		}
	}
}
