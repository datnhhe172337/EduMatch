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
	public class LevelService : ILevelService
	{
		private readonly ILevelRepository _repository;
		private readonly IMapper _mapper;

		public LevelService(ILevelRepository repository, IMapper mapper)
		{
			_repository = repository;
			_mapper = mapper;
		}

		public async Task<LevelDto?> GetByIdAsync(int id)
		{
			var entity = await _repository.GetByIdAsync(id);
			return entity != null ? _mapper.Map<LevelDto>(entity) : null;
		}

		public async Task<IReadOnlyList<LevelDto>> GetAllAsync()
		{
			var entities = await _repository.GetAllAsync();
			return _mapper.Map<IReadOnlyList<LevelDto>>(entities);
		}

		public async Task<IReadOnlyList<LevelDto>> GetByNameAsync(string name)
		{
			var entities = await _repository.GetByNameAsync(name);
			return _mapper.Map<IReadOnlyList<LevelDto>>(entities);
		}

		public async Task<LevelDto> CreateAsync(LevelCreateRequest request)
		{
			var entity = _mapper.Map<Level>(request);
			await _repository.AddAsync(entity);
			return _mapper.Map<LevelDto>(entity);
		}

		public async Task<LevelDto> UpdateAsync(LevelUpdateRequest request)
		{
			var entity = _mapper.Map<Level>(request);
			await _repository.UpdateAsync(entity);
			return _mapper.Map<LevelDto>(entity);
		}

		public async Task DeleteAsync(int id)
		{
			await _repository.RemoveByIdAsync(id);
		}
	}
}
