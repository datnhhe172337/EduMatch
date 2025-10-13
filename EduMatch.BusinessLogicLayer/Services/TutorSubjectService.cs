using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
	public class TutorSubjectService : ITutorSubjectService
	{
		private readonly ITutorSubjectRepository _repository;
		private readonly IMapper _mapper;

		public TutorSubjectService(ITutorSubjectRepository repository, IMapper mapper)
		{
			_repository = repository;
			_mapper = mapper;
		}

		public async Task<TutorSubjectDto?> GetByIdFullAsync(int id)
		{
			var entity = await _repository.GetByIdFullAsync(id);
			return entity != null ? _mapper.Map<TutorSubjectDto>(entity) : null;
		}

		public async Task<TutorSubjectDto?> GetByTutorIdFullAsync(int tutorId)
		{
			var entity = await _repository.GetByTutorIdFullAsync(tutorId);
			return entity != null ? _mapper.Map<TutorSubjectDto>(entity) : null;
		}

		public async Task<IReadOnlyList<TutorSubjectDto>> GetByTutorIdAsync(int tutorId)
		{
			var entities = await _repository.GetByTutorIdAsync(tutorId);
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorSubjectDto>> GetBySubjectIdAsync(int subjectId)
		{
			var entities = await _repository.GetBySubjectIdAsync(subjectId);
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorSubjectDto>> GetByLevelIdAsync(int levelId)
		{
			var entities = await _repository.GetByLevelIdAsync(levelId);
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorSubjectDto>> GetByHourlyRateRangeAsync(decimal minRate, decimal maxRate)
		{
			var entities = await _repository.GetByHourlyRateRangeAsync(minRate, maxRate);
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorSubjectDto>> GetTutorsBySubjectAndLevelAsync(int subjectId, int levelId)
		{
			var entities = await _repository.GetTutorsBySubjectAndLevelAsync(subjectId, levelId);
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorSubjectDto>> GetAllFullAsync()
		{
			var entities = await _repository.GetAllFullAsync();
			return _mapper.Map<IReadOnlyList<TutorSubjectDto>>(entities);
		}

		public async Task<TutorSubjectDto> CreateAsync(TutorSubjectCreateRequest request)
		{
			var entity = _mapper.Map<TutorSubject>(request);
			await _repository.AddAsync(entity);
			return _mapper.Map<TutorSubjectDto>(entity);
		}

		public async Task<TutorSubjectDto> UpdateAsync(TutorSubjectUpdateRequest request)
		{
			var entity = _mapper.Map<TutorSubject>(request);
			await _repository.UpdateAsync(entity);
			return _mapper.Map<TutorSubjectDto>(entity);
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
