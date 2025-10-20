using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.TimeSlot;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
			try
			{
				// Validate request
				var validationContext = new ValidationContext(request);
				var validationResults = new List<ValidationResult>();
				if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
				{
					throw new ArgumentException($"Validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}");
				}

				// Check if time slot already exists
				var existingSlot = await _repository.GetByExactTimeAsync(request.StartTime, request.EndTime);
				if (existingSlot != null)
				{
					throw new ArgumentException($"Time slot with start time {request.StartTime} and end time {request.EndTime} already exists");
				}

                var entity = new TimeSlot
                {
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    CreatedAt = DateTime.UtcNow
                };
				await _repository.AddAsync(entity);
				return _mapper.Map<TimeSlotDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create time slot: {ex.Message}", ex);
			}
		}

		public async Task<TimeSlotDto> UpdateAsync(TimeSlotUpdateRequest request)
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
					throw new ArgumentException($"Time slot with ID {request.Id} not found");
				}

				// Check if time slot already exists (excluding current entity)
				var existingSlot = await _repository.GetByExactTimeAsync(request.StartTime, request.EndTime);
				if (existingSlot != null && existingSlot.Id != request.Id)
				{
					throw new ArgumentException($"Time slot with start time {request.StartTime} and end time {request.EndTime} already exists");
				}

                // Update only provided fields
                existingEntity.StartTime = request.StartTime;
                existingEntity.EndTime = request.EndTime;
                existingEntity.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(existingEntity);
                return _mapper.Map<TimeSlotDto>(existingEntity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update time slot: {ex.Message}", ex);
			}
		}

		public async Task DeleteAsync(int id)
		{
			await _repository.RemoveByIdAsync(id);
		}
	}
}
