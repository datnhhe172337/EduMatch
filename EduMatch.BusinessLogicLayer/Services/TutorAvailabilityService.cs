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
	public class TutorAvailabilityService : ITutorAvailabilityService
	{
		private readonly ITutorAvailabilityRepository _repository;
		private readonly IMapper _mapper;

		public TutorAvailabilityService(ITutorAvailabilityRepository repository, IMapper mapper)
		{
			_repository = repository;
			_mapper = mapper;
		}

		public async Task<TutorAvailabilityDto?> GetByIdFullAsync(int id)
		{
			var entity = await _repository.GetByIdFullAsync(id);
			return entity != null ? _mapper.Map<TutorAvailabilityDto>(entity) : null;
		}

		public async Task<TutorAvailabilityDto?> GetByTutorIdFullAsync(int tutorId)
		{
			var entity = await _repository.GetByTutorIdFullAsync(tutorId);
			return entity != null ? _mapper.Map<TutorAvailabilityDto>(entity) : null;
		}

		public async Task<IReadOnlyList<TutorAvailabilityDto>> GetByTutorIdAsync(int tutorId)
		{
			var entities = await _repository.GetByTutorIdAsync(tutorId);
			return _mapper.Map<IReadOnlyList<TutorAvailabilityDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorAvailabilityDto>> GetByDayOfWeekAsync(DayOfWeek dayOfWeek)
		{
			var entities = await _repository.GetByDayOfWeekAsync(dayOfWeek);
			return _mapper.Map<IReadOnlyList<TutorAvailabilityDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorAvailabilityDto>> GetByTimeSlotAsync(int slotId)
		{
			var entities = await _repository.GetByTimeSlotAsync(slotId);
			return _mapper.Map<IReadOnlyList<TutorAvailabilityDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorAvailabilityDto>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
		{
			var entities = await _repository.GetByDateRangeAsync(fromDate, toDate);
			return _mapper.Map<IReadOnlyList<TutorAvailabilityDto>>(entities);
		}

		public async Task<IReadOnlyList<TutorAvailabilityDto>> GetAllFullAsync()
		{
			var entities = await _repository.GetAllFullAsync();
			return _mapper.Map<IReadOnlyList<TutorAvailabilityDto>>(entities);
		}

		public async Task<TutorAvailabilityDto> CreateAsync(TutorAvailabilityCreateRequest request)
		{
			var entity = _mapper.Map<TutorAvailability>(request);
			await _repository.AddAsync(entity);
			return _mapper.Map<TutorAvailabilityDto>(entity);
		}

		public async Task<TutorAvailabilityDto> UpdateAsync(TutorAvailabilityUpdateRequest request)
		{
			var entity = _mapper.Map<TutorAvailability>(request);
			await _repository.UpdateAsync(entity);
			return _mapper.Map<TutorAvailabilityDto>(entity);
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
