using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Schedule;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ITutorAvailabilityRepository _tutorAvailabilityRepository;
        private readonly IMeetingSessionRepository _meetingSessionRepository;
        private readonly IMapper _mapper;

        public ScheduleService(
            IScheduleRepository scheduleRepository,
            IBookingRepository bookingRepository,
            ITutorAvailabilityRepository tutorAvailabilityRepository,
            IMeetingSessionRepository meetingSessionRepository,
            IMapper mapper)
        {
            _scheduleRepository = scheduleRepository;
            _bookingRepository = bookingRepository;
            _tutorAvailabilityRepository = tutorAvailabilityRepository;
            _meetingSessionRepository = meetingSessionRepository;
            _mapper = mapper;
        }

        public async Task<List<ScheduleDto>> GetAllByBookingIdAndStatusAsync(int bookingId, int? status, int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            var entities = await _scheduleRepository.GetAllByBookingIdAndStatusAsync(bookingId, status, page, pageSize);
            return _mapper.Map<List<ScheduleDto>>(entities);
        }

        public Task<int> CountByBookingIdAndStatusAsync(int bookingId, int? status)
        {
            return _scheduleRepository.CountByBookingIdAndStatusAsync(bookingId, status);
        }

        public async Task<ScheduleDto?> GetByAvailabilityIdAsync(int availabilitiId)
        {
            var entity = await _scheduleRepository.GetByAvailabilityIdAsync(availabilitiId);
            return entity == null ? null : _mapper.Map<ScheduleDto>(entity);
        }

        public async Task<ScheduleDto?> GetByIdAsync(int id)
        {
            var entity = await _scheduleRepository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<ScheduleDto>(entity);
        }

        public async Task<ScheduleDto> CreateAsync(ScheduleCreateRequest request)
        {
            // Validate TutorAvailability exists
            var availability = await _tutorAvailabilityRepository.GetByIdFullAsync(request.AvailabilitiId)
                ?? throw new Exception("TutorAvailability không tồn tại");

            // Validate Booking exists
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId)
                ?? throw new Exception("Booking không tồn tại");

            // AvailabilitiId chưa được sử dụng
            var existingSchedule = await _scheduleRepository.GetByAvailabilityIdAsync(request.AvailabilitiId);
            if (existingSchedule != null)
                throw new Exception("TutorAvailability này đã được sử dụng cho một Schedule khác");

            var now = DateTime.UtcNow;

            // Create Schedule entity
            var entity = new Schedule
            {
                AvailabilitiId = request.AvailabilitiId,
                BookingId = request.BookingId,
                Status = (int)ScheduleStatus.Upcoming,
                AttendanceNote = request.AttendanceNote,
                IsRefunded = false,
                RefundedAt = null,
                CreatedAt = now,
                UpdatedAt = null
            };

            await _scheduleRepository.CreateAsync(entity);
            return _mapper.Map<ScheduleDto>(entity);
        }

        public async Task<ScheduleDto> UpdateAsync(ScheduleUpdateRequest request)
        {
            var entity = await _scheduleRepository.GetByIdAsync(request.Id)
                ?? throw new Exception("Schedule không tồn tại");

            var oldAvailabilityId = entity.AvailabilitiId;
            var availabilityIdChanged = false;

            // Validate TutorAvailability if being updated
            if (request.AvailabilitiId.HasValue)
            {
                var availability = await _tutorAvailabilityRepository.GetByIdFullAsync(request.AvailabilitiId.Value)
                    ?? throw new Exception("TutorAvailability không tồn tại");

                if (availability.Slot == null)
                    throw new Exception("TutorAvailability không có TimeSlot");

                //  AvailabilitiId chưa được sử dụng 
                var existingSchedule = await _scheduleRepository.GetByAvailabilityIdAsync(request.AvailabilitiId.Value);
                if (existingSchedule != null && existingSchedule.Id != request.Id)
                    throw new Exception("TutorAvailability này đã được sử dụng cho một Schedule khác");

                // Check if AvailabilitiId changed
                if (oldAvailabilityId != request.AvailabilitiId.Value)
                {
                    availabilityIdChanged = true;
                }

                entity.AvailabilitiId = request.AvailabilitiId.Value;
            }

            // Validate Booking if being updated
            if (request.BookingId.HasValue)
            {
                var booking = await _bookingRepository.GetByIdAsync(request.BookingId.Value)
                    ?? throw new Exception("Booking không tồn tại");
                entity.BookingId = request.BookingId.Value;
            }

            if (request.Status.HasValue)
                entity.Status = (int)request.Status.Value;

            if (request.AttendanceNote != null)
                entity.AttendanceNote = request.AttendanceNote;

            if (request.IsRefunded.HasValue)
            {
                entity.IsRefunded = request.IsRefunded.Value;
                // Set RefundedAt if IsRefunded = true
                if (request.IsRefunded.Value && entity.RefundedAt == null)
                    entity.RefundedAt = DateTime.UtcNow;
                // Clear RefundedAt if IsRefunded = false
                else if (!request.IsRefunded.Value)
                    entity.RefundedAt = null;
            }

            entity.UpdatedAt = DateTime.UtcNow;

            await _scheduleRepository.UpdateAsync(entity);

            // If AvailabilitiId changed, update MeetingSession's StartTime and EndTime
            if (availabilityIdChanged && request.AvailabilitiId.HasValue)
            {
                var meetingSession = await _meetingSessionRepository.GetByScheduleIdAsync(entity.Id);
                if (meetingSession != null)
                {
                    // Get updated schedule with new Availability
                    var updatedSchedule = await _scheduleRepository.GetByIdAsync(entity.Id);
                    if (updatedSchedule?.Availabiliti != null && updatedSchedule.Availabiliti.Slot != null)
                    {
                        var availability = updatedSchedule.Availabiliti;
                        var slot = availability.Slot;
                        
                        // Calculate new StartTime and EndTime from new Availability
                        var newStartTime = availability.StartDate.Date.Add(slot.StartTime.ToTimeSpan());
                        var newEndTime = availability.StartDate.Date.Add(slot.EndTime.ToTimeSpan());

                        if (newStartTime >= newEndTime)
                            throw new Exception("Thời gian slot không hợp lệ: StartTime phải nhỏ hơn EndTime");

                        meetingSession.StartTime = newStartTime;
                        meetingSession.EndTime = newEndTime;
                        meetingSession.UpdatedAt = DateTime.UtcNow;

                        await _meetingSessionRepository.UpdateAsync(meetingSession);
                    }
                }
            }

            return _mapper.Map<ScheduleDto>(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _scheduleRepository.DeleteAsync(id);
        }
    }
}

