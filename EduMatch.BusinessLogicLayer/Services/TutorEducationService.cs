using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.TutorEducation;
using EduMatch.BusinessLogicLayer.Requests.Common;
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
		private readonly ITutorEducationRepository _tutorEducationRepository;
		private readonly IEducationInstitutionRepository _educationInstitutionRepository;
		private readonly ITutorProfileRepository _tutorProfileRepository;
		private readonly IMapper _mapper;
		private readonly ICloudMediaService _cloudMedia;
		private readonly CurrentUserService _currentUserService;

        public TutorEducationService(
			ITutorEducationRepository repository,
            IMapper mapper,
            ICloudMediaService cloudMedia,
            CurrentUserService currentUserService,
            ITutorProfileRepository tutorProfileRepository,
            IEducationInstitutionRepository educationInstitutionRepository
            )
		{
			_tutorEducationRepository = repository;
			_mapper = mapper;
            _cloudMedia = cloudMedia; 
			_currentUserService = currentUserService;
			_tutorProfileRepository = tutorProfileRepository;
			_educationInstitutionRepository = educationInstitutionRepository;
		}

		/// <summary>
		/// Lấy TutorEducation theo ID với đầy đủ thông tin
		/// </summary>
		public async Task<TutorEducationDto?> GetByIdFullAsync(int id)
		{
			if (id <= 0)
				throw new ArgumentException("ID must be greater than 0");
			var entity = await _tutorEducationRepository.GetByIdFullAsync(id);
			return entity != null ? _mapper.Map<TutorEducationDto>(entity) : null;
		}

		/// <summary>
		/// Lấy TutorEducation theo TutorId với đầy đủ thông tin
		/// </summary>
		public async Task<TutorEducationDto?> GetByTutorIdFullAsync(int tutorId)
		{
			if (tutorId <= 0)
				throw new ArgumentException("TutorId must be greater than 0");
			var entity = await _tutorEducationRepository.GetByTutorIdFullAsync(tutorId);
			return entity != null ? _mapper.Map<TutorEducationDto>(entity) : null;
		}

		/// <summary>
		/// Lấy danh sách TutorEducation theo TutorId
		/// </summary>
		public async Task<IReadOnlyList<TutorEducationDto>> GetByTutorIdAsync(int tutorId)
		{
			if (tutorId <= 0)
				throw new ArgumentException("TutorId must be greater than 0");
			var entities = await _tutorEducationRepository.GetByTutorIdAsync(tutorId);
			return _mapper.Map<IReadOnlyList<TutorEducationDto>>(entities);
		}

		/// <summary>
		/// Lấy danh sách TutorEducation theo InstitutionId
		/// </summary>
		public async Task<IReadOnlyList<TutorEducationDto>> GetByInstitutionIdAsync(int institutionId)
		{
			if (institutionId <= 0)
				throw new ArgumentException("InstitutionId must be greater than 0");
			var entities = await _tutorEducationRepository.GetByInstitutionIdAsync(institutionId);
			return _mapper.Map<IReadOnlyList<TutorEducationDto>>(entities);
		}

		/// <summary>
		/// Lấy danh sách TutorEducation theo trạng thái xác thực
		/// </summary>
		public async Task<IReadOnlyList<TutorEducationDto>> GetByVerifiedStatusAsync(VerifyStatus verified)
		{
			var entities = await _tutorEducationRepository.GetByVerifiedStatusAsync(verified);
			return _mapper.Map<IReadOnlyList<TutorEducationDto>>(entities);
		}

		/// <summary>
		/// Lấy danh sách TutorEducation đang chờ xác thực
		/// </summary>
		public async Task<IReadOnlyList<TutorEducationDto>> GetPendingVerificationsAsync()
		{
			var entities = await _tutorEducationRepository.GetPendingVerificationsAsync();
			return _mapper.Map<IReadOnlyList<TutorEducationDto>>(entities);
		}

		/// <summary>
		/// Lấy tất cả TutorEducation với đầy đủ thông tin
		/// </summary>
		public async Task<IReadOnlyList<TutorEducationDto>> GetAllFullAsync()
		{
			var entities = await _tutorEducationRepository.GetAllFullAsync();
			return _mapper.Map<IReadOnlyList<TutorEducationDto>>(entities);
		}

	
	
	
			/// <summary>
		/// Tạo TutorEducation mới
		/// </summary>
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

				if (string.IsNullOrWhiteSpace(request.CertificateEducationUrl))
					throw new ArgumentException("Certificate education URL is required.");

				// MAP  -> ENTITY
				var entity = new TutorEducation
				{
					TutorId = request.TutorId,
					InstitutionId = request.InstitutionId,
					IssueDate = request.IssueDate,
					CertificateUrl = request.CertificateEducationUrl,
					CertificatePublicId = null, // No public ID for external URLs
					CreatedAt = DateTime.UtcNow,
					Verified = (int)VerifyStatus.Pending,
					RejectReason = null
				};

				await _tutorEducationRepository.AddAsync(entity);
				return _mapper.Map<TutorEducationDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create tutor education: {ex.Message}", ex);
			}
		}


	
		

		/// <summary>
		/// Cập nhật TutorEducation
		/// </summary>
		public async Task<TutorEducationDto> UpdateAsync(TutorEducationUpdateRequest request)
		{
			try
			{
				// Check if entity exists
				var existingEntity = await _tutorEducationRepository.GetByIdFullAsync(request.Id);
				if (existingEntity == null)
				{
					throw new ArgumentException($"Tutor education with ID {request.Id} not found");
				}

				// Validate InstitutionId exists
				var institution = await _educationInstitutionRepository.GetByIdAsync(request.InstitutionId);
				if (institution == null)
				{
					throw new ArgumentException($"Education institution with ID {request.InstitutionId} not found");
				}

				// Update only provided fields (chỉ update khi có giá trị)
				existingEntity.TutorId = request.TutorId;
				// InstitutionId luôn được update vì có validation Required
				existingEntity.InstitutionId = request.InstitutionId;
				if (request.IssueDate.HasValue)
					existingEntity.IssueDate = request.IssueDate.Value;

				// Update certificate URL if provided
				if (!string.IsNullOrWhiteSpace(request.CertificateEducationUrl))
				{
					existingEntity.CertificateUrl = request.CertificateEducationUrl;
					existingEntity.CertificatePublicId = null; // No public ID for external URLs
				}

				// Handle Verified status
				if (request.Verified.HasValue)
				{
					existingEntity.Verified = (int)request.Verified.Value;
					if (request.Verified.Value == VerifyStatus.Rejected)
					{
						var reason = request.RejectReason ?? existingEntity.RejectReason;
						if (string.IsNullOrWhiteSpace(reason))
							throw new ArgumentException("Reject reason is required when verification status is Rejected.");
						existingEntity.RejectReason = reason.Trim();
					}
					else
					{
						existingEntity.RejectReason = null;
					}
				}

				await _tutorEducationRepository.UpdateAsync(existingEntity);
				return _mapper.Map<TutorEducationDto>(existingEntity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update tutor education: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Tạo nhiều TutorEducation
		/// </summary>
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

		/// <summary>
		/// Xóa TutorEducation theo ID
		/// </summary>
		public async Task DeleteAsync(int id)
		{
			if (id <= 0)
				throw new ArgumentException("ID must be greater than 0");
			await _tutorEducationRepository.RemoveByIdAsync(id);
		}

		/// <summary>
		/// Xóa tất cả TutorEducation theo TutorId
		/// </summary>
		public async Task DeleteByTutorIdAsync(int tutorId)
		{
			if (tutorId <= 0)
				throw new ArgumentException("TutorId must be greater than 0");
			await _tutorEducationRepository.RemoveByTutorIdAsync(tutorId);
		}
	}
}
