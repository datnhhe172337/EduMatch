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
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
			try
			{
				// Validate request
				var validationContext = new ValidationContext(request);
				var validationResults = new List<ValidationResult>();
				if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
				{
					throw new ArgumentException($"Validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}");
				}

				var entity = _mapper.Map<TutorAvailability>(request);
				await _repository.AddAsync(entity);
				return _mapper.Map<TutorAvailabilityDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create tutor availability: {ex.Message}", ex);
			}
		}

		public async Task<TutorAvailabilityDto> UpdateAsync(TutorAvailabilityUpdateRequest request)
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
				var existingEntity = await _repository.GetByIdFullAsync(request.Id);
				if (existingEntity == null)
				{
					throw new ArgumentException($"Tutor availability with ID {request.Id} not found");
				}

				var entity = _mapper.Map<TutorAvailability>(request);
				await _repository.UpdateAsync(entity);
				return _mapper.Map<TutorAvailabilityDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update tutor availability: {ex.Message}", ex);
			}
		}

		public async Task<List<TutorAvailabilityDto>> CreateBulkAsync(List<TutorAvailabilityCreateRequest> requests)
		{
			try
			{
				var results = new List<TutorAvailabilityDto>();
				foreach (var request in requests)
				{
					var result = await CreateAsync(request);
					results.Add(result);
				}
				return results;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create bulk tutor availabilities: {ex.Message}", ex);
			}
		}

		public async Task<List<TutorAvailabilityDto>> CreateRecurringWeeklyAsync(int tutorId, List<int> slotIds, List<DayOfWeek> daysOfWeek, DateTime startDate, DateTime? endDate = null)
		{
			try
			{
				var requests = new List<TutorAvailabilityCreateRequest>();
				
				foreach (var dayOfWeek in daysOfWeek)
				{
					foreach (var slotId in slotIds)
					{
						requests.Add(new TutorAvailabilityCreateRequest
						{
							TutorId = tutorId,
							DayOfWeek = dayOfWeek,
							SlotId = slotId,
							IsRecurring = true,
							EffectiveFrom = startDate,
							EffectiveTo = endDate
						});
					}
				}

				return await CreateBulkAsync(requests);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create recurring weekly availability: {ex.Message}", ex);
			}
		}

		public async Task<List<TutorAvailabilityDto>> CreateSingleDayAsync(int tutorId, List<int> slotIds, DateTime specificDate)
		{
			try
			{
				var requests = new List<TutorAvailabilityCreateRequest>();
				
				foreach (var slotId in slotIds)
				{
					requests.Add(new TutorAvailabilityCreateRequest
					{
						TutorId = tutorId,
						DayOfWeek = specificDate.DayOfWeek,
						SlotId = slotId,
						IsRecurring = false,
						EffectiveFrom = specificDate.Date,
						EffectiveTo = specificDate.Date.AddDays(1).AddTicks(-1) // End of the same day
					});
				}

				return await CreateBulkAsync(requests);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create single day availability: {ex.Message}", ex);
			}
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
