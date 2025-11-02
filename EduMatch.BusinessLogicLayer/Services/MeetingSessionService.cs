using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.GoogleMeeting;
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
        private readonly IGoogleCalendarService _googleCalendarService;
        private readonly IMapper _mapper;

        public MeetingSessionService(
            IMeetingSessionRepository meetingSessionRepository,
            IScheduleRepository scheduleRepository,
            IGoogleTokenRepository googleTokenRepository,
            IGoogleCalendarService googleCalendarService,
            IMapper mapper)
        {
            _meetingSessionRepository = meetingSessionRepository;
            _scheduleRepository = scheduleRepository;
            _googleTokenRepository = googleTokenRepository;
            _googleCalendarService = googleCalendarService;
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
            // Validate Schedule exists and load related data
            var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId)
                ?? throw new Exception("Schedule không tồn tại");

            if (schedule.Availabiliti == null)
                throw new Exception("Schedule không có TutorAvailability");

            if (schedule.Availabiliti.Slot == null)
                throw new Exception("TutorAvailability không có TimeSlot");

            if (schedule.Booking == null)
                throw new Exception("Schedule không có Booking");

            //  Schedule chưa có MeetingSession
            var existingMeetingSession = await _meetingSessionRepository.GetByScheduleIdAsync(request.ScheduleId);
            if (existingMeetingSession != null)
                throw new Exception("Schedule này đã có MeetingSession");

            // Validate OrganizerEmail exists in GoogleToken
            var googleToken = await _googleTokenRepository.GetByEmailAsync(request.OrganizerEmail)
                ?? throw new Exception("OrganizerEmail không tồn tại trong hệ thống GoogleToken");

            // Calculate StartTime and EndTime from TutorAvailability
            var availability = schedule.Availabiliti;
            var slot = availability.Slot;
            var booking = schedule.Booking;
            var tutorSubject = booking.TutorSubject ?? throw new Exception("Booking không có TutorSubject");
            var tutor = tutorSubject.Tutor ?? throw new Exception("TutorSubject không có Tutor");
            var subject = tutorSubject.Subject ?? throw new Exception("TutorSubject không có Subject");
            var level = tutorSubject.Level ?? throw new Exception("TutorSubject không có Level");

            // Get Tutor email
            var tutorEmail = tutor.UserEmail;

            // StartTime = StartDate + Slot.StartTime
            var startTime = availability.StartDate.Date.Add(slot.StartTime.ToTimeSpan());
            
            // EndTime = StartDate + Slot.EndTime
            var endTime = availability.StartDate.Date.Add(slot.EndTime.ToTimeSpan());

            // Validate StartTime < EndTime
            if (startTime >= endTime)
                throw new Exception("Thời gian slot không hợp lệ: StartTime phải nhỏ hơn EndTime");

            // Build summary and description
            var subjectName = subject.SubjectName;
            var levelName = level?.Name ?? "Tất cả cấp độ";
            var summary = $"Buổi học {subjectName} - {levelName}";
            var description = $"Buổi học EduMatch\n" +
                             $"Môn học: {subjectName}\n" +
                             $"Cấp độ: {levelName}\n" +
                             $"Học viên: {booking.LearnerEmail}\n" +
                             $"Gia sư: {tutorEmail}\n" +
                             $"Schedule ID: {schedule.Id}";

            // Create Google Meeting request
            var meetingRequest = new CreateMeetingRequest
            {
                Summary = summary,
                Description = description,
                StartTime = startTime,
                EndTime = endTime,
                AttendeeEmails = new List<string> { booking.LearnerEmail, tutorEmail, request.OrganizerEmail }
            };

            // Create Google Calendar Event and get response
            var googleEventResponse = await _googleCalendarService.CreateEventAsync(meetingRequest)
                ?? throw new Exception("Không thể tạo Google Calendar Event");

            // Extract data from Google response
            var meetLink = googleEventResponse.HangoutLink ?? googleEventResponse.HtmlLink;
            var meetCode = googleEventResponse.ConferenceData?.ConferenceId;
            var eventId = googleEventResponse.EventId;

            var now = DateTime.UtcNow;

            // Create MeetingSession entity
            var entity = new MeetingSession
            {
                ScheduleId = request.ScheduleId,
                OrganizerEmail = request.OrganizerEmail,
                MeetLink = meetLink,
                MeetCode = meetCode,
                EventId = eventId,
                StartTime = startTime,
                EndTime = endTime,
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

                // Schedule chưa có MeetingSession 
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

