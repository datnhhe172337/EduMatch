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

namespace EduMatch.BusinessLogicLayer.Services
{
	public class TutorCertificateService : ITutorCertificateService
	{
		private readonly ITutorCertificateRepository _repository;
		private readonly IMapper _mapper;

		public TutorCertificateService(ITutorCertificateRepository repository, IMapper mapper)
		{
			_repository = repository;
			_mapper = mapper;
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
			var entity = _mapper.Map<TutorCertificate>(request);
			await _repository.AddAsync(entity);
			return _mapper.Map<TutorCertificateDto>(entity);
		}

		public async Task<TutorCertificateDto> UpdateAsync(TutorCertificateUpdateRequest request)
		{
			var entity = _mapper.Map<TutorCertificate>(request);
			await _repository.UpdateAsync(entity);
			return _mapper.Map<TutorCertificateDto>(entity);
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
