using System;
using System.Threading.Tasks;
using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.MeetingSession;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class MeetingSessionService : IMeetingSessionService
    {
        private readonly IMeetingSessionRepository _meetingSessionRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IGoogleTokenRepository _googleTokenRepository;
        private readonly IMapper _mapper;

        public MeetingSessionService(
            IMeetingSessionRepository meetingSessionRepository,
            IScheduleRepository scheduleRepository,
            IGoogleTokenRepository googleTokenRepository,
            IMapper mapper)
        {
            _meetingSessionRepository = meetingSessionRepository;
            _scheduleRepository = scheduleRepository;
            _googleTokenRepository = googleTokenRepository;
            _mapper = mapper;
        }

        public async Task<MeetingSessionDto?> GetByIdAsync(int id)
        {
            var entity = await _meetingSessionRepository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<MeetingSessionDto>(entity);
        }

        public async Task<MeetingSessionDto?> GetByScheduleIdAsync(int scheduleId)
        {
            var entity = await _meetingSessionRepository.GetByScheduleIdAsync(scheduleId);
            return entity == null ? null : _mapper.Map<MeetingSessionDto>(entity);
        }

        public async Task<MeetingSessionDto> CreateAsync(MeetingSessionCreateRequest request)
        {
            // Validate Schedule exists
            var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId)
                ?? throw new Exception("Schedule không tồn tại");

            // Validate 1-1 relationship: Schedule chưa có MeetingSession
            var existingMeetingSession = await _meetingSessionRepository.GetByScheduleIdAsync(request.ScheduleId);
            if (existingMeetingSession != null)
                throw new Exception("Schedule này đã có MeetingSession");

            // Validate OrganizerEmail exists in GoogleToken
            var googleToken = await _googleTokenRepository.GetByEmailAsync(request.OrganizerEmail)
                ?? throw new Exception("OrganizerEmail không tồn tại trong hệ thống GoogleToken");

            // Validate StartTime < EndTime
            if (request.StartTime >= request.EndTime)
                throw new Exception("StartTime phải nhỏ hơn EndTime");

            var now = DateTime.UtcNow;

            // Create MeetingSession entity
            var entity = new MeetingSession
            {
                ScheduleId = request.ScheduleId,
                OrganizerEmail = request.OrganizerEmail,
                MeetLink = request.MeetLink,
                MeetCode = request.MeetCode,
                EventId = request.EventId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                MeetingType = (int)MeetingType.Main, // Default là Main
                CreatedAt = now,
                UpdatedAt = null
            };

            await _meetingSessionRepository.CreateAsync(entity);
            return _mapper.Map<MeetingSessionDto>(entity);
        }

        public async Task<MeetingSessionDto> UpdateAsync(MeetingSessionUpdateRequest request)
        {
            var entity = await _meetingSessionRepository.GetByIdAsync(request.Id)
                ?? throw new Exception("MeetingSession không tồn tại");

            // Validate Schedule if being updated
            if (request.ScheduleId.HasValue)
            {
                var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId.Value)
                    ?? throw new Exception("Schedule không tồn tại");

                // Validate 1-1 relationship: Schedule chưa có MeetingSession (trừ chính nó)
                var existingMeetingSession = await _meetingSessionRepository.GetByScheduleIdAsync(request.ScheduleId.Value);
                if (existingMeetingSession != null && existingMeetingSession.Id != request.Id)
                    throw new Exception("Schedule này đã có MeetingSession khác");

                entity.ScheduleId = request.ScheduleId.Value;
            }

            // Validate OrganizerEmail if being updated
            if (!string.IsNullOrWhiteSpace(request.OrganizerEmail))
            {
                var googleToken = await _googleTokenRepository.GetByEmailAsync(request.OrganizerEmail)
                    ?? throw new Exception("OrganizerEmail không tồn tại trong hệ thống GoogleToken");
                entity.OrganizerEmail = request.OrganizerEmail;
            }

            if (!string.IsNullOrWhiteSpace(request.MeetLink))
                entity.MeetLink = request.MeetLink;

            if (request.MeetCode != null)
                entity.MeetCode = request.MeetCode;

            if (request.EventId != null)
                entity.EventId = request.EventId;

            if (request.StartTime.HasValue)
                entity.StartTime = request.StartTime.Value;

            if (request.EndTime.HasValue)
                entity.EndTime = request.EndTime.Value;

            // Validate StartTime < EndTime after update
            if (entity.StartTime >= entity.EndTime)
                throw new Exception("StartTime phải nhỏ hơn EndTime");

            if (request.MeetingType.HasValue)
                entity.MeetingType = (int)request.MeetingType.Value;

            entity.UpdatedAt = DateTime.UtcNow;

            await _meetingSessionRepository.UpdateAsync(entity);
            return _mapper.Map<MeetingSessionDto>(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _meetingSessionRepository.DeleteAsync(id);
        }
    }
}

