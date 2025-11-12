using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.TimeSlot;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace EduMatch.BusinessLogicLayer.Services
{
	public class TimeSlotService : ITimeSlotService
	{
		private readonly ITimeSlotRepository _timeSlotRepository;
		private readonly IMapper _mapper;

		public TimeSlotService(ITimeSlotRepository repository, IMapper mapper)
		{
			_timeSlotRepository = repository;
			_mapper = mapper;
		}

		/// <summary>
		/// Lấy TimeSlot theo ID
		/// </summary>
		public async Task<TimeSlotDto?> GetByIdAsync(int id)
		{
			if (id <= 0)
				throw new ArgumentException("ID must be greater than 0");
			var entity = await _timeSlotRepository.GetByIdAsync(id);
			return entity != null ? _mapper.Map<TimeSlotDto>(entity) : null;
		}

		/// <summary>
		/// Lấy tất cả TimeSlot
		/// </summary>
		public async Task<IReadOnlyList<TimeSlotDto>> GetAllAsync()
		{
			var entities = await _timeSlotRepository.GetAllAsync();
			return _mapper.Map<IReadOnlyList<TimeSlotDto>>(entities);
		}

		/// <summary>
		/// Lấy TimeSlot theo khoảng thời gian
		/// </summary>
		public async Task<IReadOnlyList<TimeSlotDto>> GetByTimeRangeAsync(TimeOnly startTime, TimeOnly endTime)
		{
			var entities = await _timeSlotRepository.GetByTimeRangeAsync(startTime, endTime);
			return _mapper.Map<IReadOnlyList<TimeSlotDto>>(entities);
		}

		/// <summary>
		/// Lấy TimeSlot theo thời gian chính xác
		/// </summary>
		public async Task<TimeSlotDto?> GetByExactTimeAsync(TimeOnly startTime, TimeOnly endTime)
		{
			var entity = await _timeSlotRepository.GetByExactTimeAsync(startTime, endTime);
			return entity != null ? _mapper.Map<TimeSlotDto>(entity) : null;
		}

		/// <summary>
		/// Tạo TimeSlot mới
		/// </summary>
        public async Task<TimeSlotDto> CreateAsync(TimeSlotCreateRequest request)
		{
			try
			{
				// Check if time slot already exists
				var existingSlot = await _timeSlotRepository.GetByExactTimeAsync(request.StartTime, request.EndTime);
				if (existingSlot != null)
				{
					throw new ArgumentException($"Time slot with start time {request.StartTime} and end time {request.EndTime} already exists");
				}

                var entity = new TimeSlot
                {
                    StartTime = request.StartTime,
                    EndTime = request.EndTime
                };
				await _timeSlotRepository.AddAsync(entity);
				return _mapper.Map<TimeSlotDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create time slot: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Cập nhật TimeSlot
		/// </summary>
	public async Task<TimeSlotDto> UpdateAsync(TimeSlotUpdateRequest request)
	{
		try
		{
			// Check if entity exists
				var existingEntity = await _timeSlotRepository.GetByIdAsync(request.Id);
				if (existingEntity == null)
				{
					throw new ArgumentException($"Time slot with ID {request.Id} not found");
				}

				// Check if time slot already exists (excluding current entity)
				var existingSlot = await _timeSlotRepository.GetByExactTimeAsync(request.StartTime, request.EndTime);
				if (existingSlot != null && existingSlot.Id != request.Id)
				{
					throw new ArgumentException($"Time slot with start time {request.StartTime} and end time {request.EndTime} already exists");
				}

                // Update only provided fields
                existingEntity.StartTime = request.StartTime;
                existingEntity.EndTime = request.EndTime;

				await _timeSlotRepository.UpdateAsync(existingEntity);
                return _mapper.Map<TimeSlotDto>(existingEntity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update time slot: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Xóa TimeSlot theo ID
		/// </summary>
		public async Task DeleteAsync(int id)
		{
			if (id <= 0)
				throw new ArgumentException("ID must be greater than 0");
			await _timeSlotRepository.RemoveByIdAsync(id);
		}
	}
}
