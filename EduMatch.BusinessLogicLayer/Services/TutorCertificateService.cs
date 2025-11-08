using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.TutorCertificate;
using EduMatch.BusinessLogicLayer.Requests.Common;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
	public class TutorCertificateService : ITutorCertificateService
	{
		private readonly ITutorCertificateRepository _tutorCertificateRepository;
		private readonly IMapper _mapper;
		private readonly ICloudMediaService _cloudMedia;
		private readonly CurrentUserService _currentUserService;
		private readonly ITutorProfileRepository _tutorProfileRepository;
		private readonly ICertificateTypeRepository _certificateTypeRepository;	

		public TutorCertificateService(
			ITutorCertificateRepository repository,
			IMapper mapper,
			ICloudMediaService cloudMedia,
			CurrentUserService currentUserService ,
			ITutorProfileRepository tutorProfileRepository,
			ICertificateTypeRepository certificateTypeRepository	)
		{
			_tutorCertificateRepository = repository;
			_mapper = mapper;
			_cloudMedia = cloudMedia;
			_currentUserService = currentUserService;
			_tutorProfileRepository = tutorProfileRepository;
			_certificateTypeRepository = certificateTypeRepository;
		}

		/// <summary>
		/// Lấy TutorCertificate theo ID với đầy đủ thông tin
		/// </summary>
		public async Task<TutorCertificateDto?> GetByIdFullAsync(int id)
		{
			if (id <= 0)
				throw new ArgumentException("ID must be greater than 0");
			var entity = await _tutorCertificateRepository.GetByIdFullAsync(id);
			return entity != null ? _mapper.Map<TutorCertificateDto>(entity) : null;
		}

		/// <summary>
		/// Lấy TutorCertificate theo TutorId với đầy đủ thông tin
		/// </summary>
		public async Task<TutorCertificateDto?> GetByTutorIdFullAsync(int tutorId)
		{
			if (tutorId <= 0)
				throw new ArgumentException("TutorId must be greater than 0");
			var entity = await _tutorCertificateRepository.GetByTutorIdFullAsync(tutorId);
			return entity != null ? _mapper.Map<TutorCertificateDto>(entity) : null;
		}

		/// <summary>
		/// Lấy danh sách TutorCertificate theo TutorId
		/// </summary>
		public async Task<IReadOnlyList<TutorCertificateDto>> GetByTutorIdAsync(int tutorId)
		{
			if (tutorId <= 0)
				throw new ArgumentException("TutorId must be greater than 0");
			var entities = await _tutorCertificateRepository.GetByTutorIdAsync(tutorId);
			return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
		}

		/// <summary>
		/// Lấy danh sách TutorCertificate theo CertificateTypeId
		/// </summary>
		public async Task<IReadOnlyList<TutorCertificateDto>> GetByCertificateTypeAsync(int certificateTypeId)
		{
			if (certificateTypeId <= 0)
				throw new ArgumentException("CertificateTypeId must be greater than 0");
			var entities = await _tutorCertificateRepository.GetByCertificateTypeAsync(certificateTypeId);
			return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
		}

		/// <summary>
		/// Lấy danh sách TutorCertificate theo trạng thái xác thực
		/// </summary>
		public async Task<IReadOnlyList<TutorCertificateDto>> GetByVerifiedStatusAsync(VerifyStatus verified)
		{
			var entities = await _tutorCertificateRepository.GetByVerifiedStatusAsync(verified);
			return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
		}

		/// <summary>
		/// Lấy danh sách TutorCertificate đã hết hạn
		/// </summary>
		public async Task<IReadOnlyList<TutorCertificateDto>> GetExpiredCertificatesAsync()
		{
			var entities = await _tutorCertificateRepository.GetExpiredCertificatesAsync();
			return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
		}

		/// <summary>
		/// Lấy danh sách TutorCertificate sắp hết hạn
		/// </summary>
		public async Task<IReadOnlyList<TutorCertificateDto>> GetExpiringCertificatesAsync(DateTime beforeDate)
		{
			var entities = await _tutorCertificateRepository.GetExpiringCertificatesAsync(beforeDate);
			return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
		}

		/// <summary>
		/// Lấy tất cả TutorCertificate với đầy đủ thông tin
		/// </summary>
		public async Task<IReadOnlyList<TutorCertificateDto>> GetAllFullAsync()
		{
			var entities = await _tutorCertificateRepository.GetAllFullAsync();
			return _mapper.Map<IReadOnlyList<TutorCertificateDto>>(entities);
		}

	
		
		
		/// <summary>
		/// Tạo TutorCertificate mới
		/// </summary>
	public async Task<TutorCertificateDto> CreateAsync(TutorCertificateCreateRequest request)
	{
		try
		{
			var tutor = await _tutorProfileRepository.GetByIdFullAsync(request.TutorId);
				if (tutor is null)
					throw new ArgumentException($"Tutor with ID {request.TutorId} not found.");

				var certificateType = await _certificateTypeRepository.GetByIdAsync(request.CertificateTypeId);
				if (certificateType is null)
					throw new ArgumentException($"CertificateType with ID {request.CertificateTypeId} not found.");

				if (string.IsNullOrWhiteSpace(request.CertificateUrl))
					throw new ArgumentException("Certificate URL is required.");

				// MAP  -> ENTITY
				var entity = new TutorCertificate
				{
					TutorId = request.TutorId,
					CertificateTypeId = request.CertificateTypeId,
					IssueDate = request.IssueDate,
					ExpiryDate = request.ExpiryDate,
					CertificateUrl = request.CertificateUrl,
					CertificatePublicId = null, // No public ID for external URLs
					CreatedAt = DateTime.UtcNow,
					Verified = (int)VerifyStatus.Pending,
					RejectReason = null
				};

				await _tutorCertificateRepository.AddAsync(entity);
				return _mapper.Map<TutorCertificateDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create tutor certificate: {ex.Message}", ex);
			}
		}

		
		
		
		/// <summary>
		/// Cập nhật TutorCertificate
		/// </summary>
	public async Task<TutorCertificateDto> UpdateAsync(TutorCertificateUpdateRequest request)
	{
		try
		{
			// Check if entity exists
				var existingEntity = await _tutorCertificateRepository.GetByIdFullAsync(request.Id);
				if (existingEntity == null)
				{
					throw new ArgumentException($"Tutor certificate with ID {request.Id} not found");
				}

				// Validate CertificateTypeId exists
				var certificateType = await _certificateTypeRepository.GetByIdAsync(request.CertificateTypeId);
				if (certificateType == null)
				{
					throw new ArgumentException($"CertificateType with ID {request.CertificateTypeId} not found");
				}

				// Update only provided fields 
				existingEntity.TutorId = request.TutorId;
				// CertificateTypeId luôn được update vì có validation Required
				existingEntity.CertificateTypeId = request.CertificateTypeId;
				if (request.IssueDate.HasValue) 
					existingEntity.IssueDate = request.IssueDate.Value;
				if (request.ExpiryDate.HasValue) 
					existingEntity.ExpiryDate = request.ExpiryDate.Value;

				// Update certificate URL if provided
				if (!string.IsNullOrWhiteSpace(request.CertificateUrl))
				{
					existingEntity.CertificateUrl = request.CertificateUrl;
					existingEntity.CertificatePublicId = null; // No public ID for external URLs
				}

				// Handle Verified status
				if (request.Verified.HasValue)
				{
					existingEntity.Verified = (int)request.Verified.Value;
					if (request.Verified.Value == VerifyStatus.Rejected)
					{
						if (string.IsNullOrWhiteSpace(request.RejectReason))
							throw new ArgumentException("Reject reason is required when verification status is Rejected.");
						existingEntity.RejectReason = request.RejectReason!.Trim();
					}
					else
					{
						existingEntity.RejectReason = null;
					}
				}

				await _tutorCertificateRepository.UpdateAsync(existingEntity);
				return _mapper.Map<TutorCertificateDto>(existingEntity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update tutor certificate: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Tạo nhiều TutorCertificate
		/// </summary>
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

		/// <summary>
		/// Xóa TutorCertificate theo ID
		/// </summary>
		public async Task DeleteAsync(int id)
		{
			if (id <= 0)
				throw new ArgumentException("ID must be greater than 0");
			await _tutorCertificateRepository.RemoveByIdAsync(id);
		}

		/// <summary>
		/// Xóa tất cả TutorCertificate theo TutorId
		/// </summary>
		public async Task DeleteByTutorIdAsync(int tutorId)
		{
			if (tutorId <= 0)
				throw new ArgumentException("TutorId must be greater than 0");
			await _tutorCertificateRepository.RemoveByTutorIdAsync(tutorId);
		}

		
	}
}
