using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

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
			var entity = _mapper.Map<CertificateType>(request);
			await _repository.AddAsync(entity);
			return _mapper.Map<CertificateTypeDto>(entity);
		}

		public async Task<CertificateTypeDto> UpdateAsync(CertificateTypeUpdateRequest request)
		{
			var entity = _mapper.Map<CertificateType>(request);
			await _repository.UpdateAsync(entity);
			return _mapper.Map<CertificateTypeDto>(entity);
		}

		public async Task DeleteAsync(int id)
		{
			await _repository.RemoveByIdAsync(id);
		}
	}
}
