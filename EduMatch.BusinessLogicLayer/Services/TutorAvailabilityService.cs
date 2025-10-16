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
		private readonly ITutorProfileRepository _tutorProfileRepository;
		private readonly ITimeSlotRepository _timeSlotRepository;


		public TutorAvailabilityService(
			ITutorAvailabilityRepository repository,
			IMapper mapper,
			ITimeSlotRepository timeSlotRepository,
			ITutorProfileRepository tutorProfileRepository)
		{
			_repository = repository;
			_mapper = mapper;
			_timeSlotRepository = timeSlotRepository;
			_tutorProfileRepository = tutorProfileRepository;
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

				var tutor = await _tutorProfileRepository.GetByIdFullAsync(request.TutorId);
				if (tutor is null)
					throw new ArgumentException($"Tutor with ID {request.TutorId} not found.");

				var timeSlot = await _timeSlotRepository.GetByIdAsync(request.SlotId);
				if (timeSlot is null)
					throw new ArgumentException($"timeSlot with ID {request.SlotId} not found.");


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


		public async Task<List<TutorAvailabilityDto>> CreateMixedAvailabilitiesAsync(TutorAvailabilityMixedRequest request)
		{
			try
			{
				var requests = new List<TutorAvailabilityCreateRequest>();
				var seen = new HashSet<string>(); // tránh trùng (TutorId + Date + SlotId)

				// XỬ LÝ KHÔNG LẶP (Non-recurring)
				if (request.NonRecurringDaySlots != null)
				{
					foreach (var daySlot in request.NonRecurringDaySlots)
					{
						var date = daySlot.Date.Date;

						foreach (var slotId in daySlot.SlotIds)
						{
							var key = $"{request.TutorId}_{date:yyyyMMdd}_{slotId}";
							if (seen.Add(key))
							{
								requests.Add(new TutorAvailabilityCreateRequest
								{
									TutorId = request.TutorId,
									DayOfWeek = date.DayOfWeek,
									SlotId = slotId,
									IsRecurring = false,
									EffectiveFrom = date,
									EffectiveTo = date.AddDays(1).AddTicks(-1)
								});
							}
						}
					}
				}

				//  XỬ LÝ LẶP HÀNG TUẦN (Recurring)
				if (request.RecurringSchedule != null && request.RecurringSchedule.Count > 0)
				{
					foreach (var recurring in request.RecurringSchedule)
					{
						var start = recurring.StartDate.Date;
						var end = (recurring.EndDate ?? start.AddMonths(1)).Date;

						foreach (var daySlot in recurring.DaySlots)
						{
							if (daySlot.SlotIds == null || daySlot.SlotIds.Count == 0)
								continue;

							foreach (var slotId in daySlot.SlotIds.Distinct())
							{
								var key = $"{request.TutorId}_{daySlot.DayOfWeek}_{slotId}";
								if (seen.Add(key))
								{
									requests.Add(new TutorAvailabilityCreateRequest
									{
										TutorId = request.TutorId,
										DayOfWeek = daySlot.DayOfWeek,   
										SlotId = slotId,                
										IsRecurring = true,            
										EffectiveFrom = start,          
										EffectiveTo = end              
									});
								}
							}
						}
					}
				}

				if (requests.Count == 0) return new List<TutorAvailabilityDto>();

				var entities = _mapper.Map<List<TutorAvailability>>(requests);

				await _repository.AddRangeAsync(entities);



				return _mapper.Map<List<TutorAvailabilityDto>>(entities);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create mixed tutor availabilities: {ex.Message}", ex);
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
