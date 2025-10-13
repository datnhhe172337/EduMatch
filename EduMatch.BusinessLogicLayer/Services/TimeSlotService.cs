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
	public class TimeSlotService : ITimeSlotService
	{
		private readonly ITimeSlotRepository _repository;
		private readonly IMapper _mapper;

		public TimeSlotService(ITimeSlotRepository repository, IMapper mapper)
		{
			_repository = repository;
			_mapper = mapper;
		}

		public async Task<TimeSlotDto?> GetByIdAsync(int id)
		{
			var entity = await _repository.GetByIdAsync(id);
			return entity != null ? _mapper.Map<TimeSlotDto>(entity) : null;
		}

		public async Task<IReadOnlyList<TimeSlotDto>> GetAllAsync()
		{
			var entities = await _repository.GetAllAsync();
			return _mapper.Map<IReadOnlyList<TimeSlotDto>>(entities);
		}

		public async Task<IReadOnlyList<TimeSlotDto>> GetByTimeRangeAsync(TimeOnly startTime, TimeOnly endTime)
		{
			var entities = await _repository.GetByTimeRangeAsync(startTime, endTime);
			return _mapper.Map<IReadOnlyList<TimeSlotDto>>(entities);
		}

		public async Task<TimeSlotDto?> GetByExactTimeAsync(TimeOnly startTime, TimeOnly endTime)
		{
			var entity = await _repository.GetByExactTimeAsync(startTime, endTime);
			return entity != null ? _mapper.Map<TimeSlotDto>(entity) : null;
		}

		public async Task<TimeSlotDto> CreateAsync(TimeSlotCreateRequest request)
		{
			var entity = _mapper.Map<TimeSlot>(request);
			await _repository.AddAsync(entity);
			return _mapper.Map<TimeSlotDto>(entity);
		}

		public async Task<TimeSlotDto> UpdateAsync(TimeSlotUpdateRequest request)
		{
			var entity = _mapper.Map<TimeSlot>(request);
			await _repository.UpdateAsync(entity);
			return _mapper.Map<TimeSlotDto>(entity);
		}

		public async Task DeleteAsync(int id)
		{
			await _repository.RemoveByIdAsync(id);
		}
	}
}
