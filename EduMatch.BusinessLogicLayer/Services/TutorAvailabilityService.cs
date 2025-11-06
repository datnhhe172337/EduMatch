using AutoMapper; // Added
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.TutorAvailability;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EduMatch.BusinessLogicLayer.Requests.User; // Added

namespace EduMatch.BusinessLogicLayer.Services
{
    public class TutorAvailabilityService : ITutorAvailabilityService
    {
        private readonly ITutorAvailabilityRepository _repository;
        private readonly IMapper _mapper;
        private readonly ITutorProfileRepository _tutorProfileRepository;
        private readonly ITimeSlotRepository _timeSlotRepository;
        private readonly IScheduleRepository _scheduleRepository;

        public TutorAvailabilityService(
            ITutorAvailabilityRepository repository,
            IMapper mapper,
            ITimeSlotRepository timeSlotRepository,
            ITutorProfileRepository tutorProfileRepository,
            IScheduleRepository scheduleRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _timeSlotRepository = timeSlotRepository;
            _tutorProfileRepository = tutorProfileRepository;
            _scheduleRepository = scheduleRepository;
        }

        /// <summary>
        /// Lấy TutorAvailability theo ID với đầy đủ thông tin
        /// </summary>
        public async Task<TutorAvailabilityDto?> GetByIdFullAsync(int id)
        {
            var entity = await _repository.GetByIdFullAsync(id);
            return entity != null ? _mapper.Map<TutorAvailabilityDto>(entity) : null;
        }

        /// <summary>
        /// Lấy danh sách TutorAvailability theo TutorId
        /// </summary>
        public async Task<IReadOnlyList<TutorAvailabilityDto>> GetByTutorIdAsync(int tutorId)
        {
            var entities = await _repository.GetByTutorIdAsync(tutorId);
            return _mapper.Map<IReadOnlyList<TutorAvailabilityDto>>(entities);
        }

        /// <summary>
        /// Lấy danh sách TutorAvailability theo TutorId với đầy đủ thông tin
        /// </summary>
        public async Task<IReadOnlyList<TutorAvailabilityDto>> GetByTutorIdFullAsync(int tutorId)
        {
            var entities = await _repository.GetByTutorIdFullAsync(tutorId);
            return _mapper.Map<IReadOnlyList<TutorAvailabilityDto>>(entities);
        }

        /// <summary>
        /// Lấy tất cả TutorAvailability với đầy đủ thông tin
        /// </summary>
        public async Task<IReadOnlyList<TutorAvailabilityDto>> GetAllFullAsync()
        {
            var entities = await _repository.GetAllFullAsync();
            return _mapper.Map<IReadOnlyList<TutorAvailabilityDto>>(entities);
        }

		/// <summary>
		/// Tạo TutorAvailability mới, tính StartDate và EndDate từ TimeSlot
		/// </summary>
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

                // Kiểm tra trùng: cùng TutorId + cùng ngày (Date) + cùng SlotId đã tồn tại
				var desiredDate = request.StartDate.Date;
				var existingForTutor = await _repository.GetByTutorIdAsync(request.TutorId);
				var isDuplicate = existingForTutor.Any(a => a.SlotId == request.SlotId && a.StartDate.Date == desiredDate);
				if (isDuplicate)
					throw new InvalidOperationException("TutorAvailability đã tồn tại cho ngày và slot này");

                // Kiểm tra xung đột với lịch học (Schedule) hiện có của tutor
                var hasScheduleConflict = await _scheduleRepository.HasTutorScheduleOnSlotDateAsync(request.TutorId, request.SlotId, desiredDate);
                if (hasScheduleConflict)
                    throw new InvalidOperationException($"Lịch dạy của gia sư trùng với lịch học đã đặt: ngày {desiredDate:dd/MM/yyyy}, khung {timeSlot.StartTime:hh\\:mm}-{timeSlot.EndTime:hh\\:mm} (SlotId {request.SlotId})");

				var entity = new TutorAvailability
				{
					TutorId = request.TutorId,
					SlotId = request.SlotId,
					Status = (int)TutorAvailabilityStatus.Available,
					CreatedAt = DateTime.UtcNow,
					StartDate = request.StartDate.Date.Add(timeSlot.StartTime.ToTimeSpan()),
					EndDate = request.StartDate.Date.Add(timeSlot.EndTime.ToTimeSpan())
				};

				await _repository.AddAsync(entity);
				return _mapper.Map<TutorAvailabilityDto>(entity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to create tutor availability: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Cập nhật TutorAvailability, tính lại StartDate và EndDate từ TimeSlot
		/// </summary>
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

				// Update only provided fields
				existingEntity.TutorId = request.TutorId;
				existingEntity.SlotId = request.SlotId;
				if (request.Status.HasValue)
					existingEntity.Status = (int)request.Status.Value;

				// Update StartDate and EndDate based on SlotId
				var timeSlot = await _timeSlotRepository.GetByIdAsync(request.SlotId);
				if (timeSlot is null)
					throw new ArgumentException($"timeSlot with ID {request.SlotId} not found.");

                // Kiểm tra trùng trước khi set ngày/giờ mới
				var desiredDate = request.StartDate.Date;
				var existingForTutor = await _repository.GetByTutorIdAsync(request.TutorId);
				var isDuplicate = existingForTutor.Any(a => a.Id != request.Id && a.SlotId == request.SlotId && a.StartDate.Date == desiredDate);
				if (isDuplicate)
					throw new InvalidOperationException("TutorAvailability đã tồn tại cho ngày và slot này");

                // Kiểm tra xung đột với lịch học (Schedule) hiện có của tutor
                var hasScheduleConflict = await _scheduleRepository.HasTutorScheduleOnSlotDateAsync(request.TutorId, request.SlotId, desiredDate);
                if (hasScheduleConflict)
                    throw new InvalidOperationException($"Lịch dạy của gia sư trùng với lịch học đã đặt: ngày {desiredDate:dd/MM/yyyy}, khung {timeSlot.StartTime:hh\\:mm}-{timeSlot.EndTime:hh\\:mm} (SlotId {request.SlotId})");

				existingEntity.StartDate = request.StartDate.Date.Add(timeSlot.StartTime.ToTimeSpan());
				existingEntity.EndDate = request.StartDate.Date.Add(timeSlot.EndTime.ToTimeSpan());
				existingEntity.UpdatedAt = DateTime.UtcNow;

				await _repository.UpdateAsync(existingEntity);
				return _mapper.Map<TutorAvailabilityDto>(existingEntity);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Failed to update tutor availability: {ex.Message}", ex);
			}
		}


		
		/// <summary>
		/// Tạo nhiều TutorAvailability
		/// </summary>
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


        /// <summary>
        /// Cập nhật trạng thái của TutorAvailability (Available/Booked/InProgress/Cancelled)
        /// </summary>
        public async Task<TutorAvailabilityDto> UpdateStatusAsync(int id, TutorAvailabilityStatus status)
        {
            var existingEntity = await _repository.GetByIdFullAsync(id)
                ?? throw new ArgumentException($"Tutor availability with ID {id} not found");

            existingEntity.Status = (int)status;
            existingEntity.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(existingEntity);
            return _mapper.Map<TutorAvailabilityDto>(existingEntity);
        }

       
        /// <summary>
        /// Xóa TutorAvailability theo ID
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            await _repository.RemoveByIdAsync(id);
        }

      
       
    }
}