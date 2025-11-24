using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.GoogleMeeting;
using EduMatch.BusinessLogicLayer.Requests.MeetingSession;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.Extensions.Options;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class MeetingSessionService : IMeetingSessionService
    {
        private readonly IMeetingSessionRepository _meetingSessionRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IGoogleTokenRepository _googleTokenRepository;
        private readonly IGoogleCalendarService _googleCalendarService;
        private readonly GoogleCalendarSettings _googleCalendarSettings;
        private readonly IMapper _mapper;

        public MeetingSessionService(
            IMeetingSessionRepository meetingSessionRepository,
            IScheduleRepository scheduleRepository,
            IGoogleTokenRepository googleTokenRepository,
            IGoogleCalendarService googleCalendarService,
            IOptions<GoogleCalendarSettings> googleCalendarSettings,
            IMapper mapper)
        {
            _meetingSessionRepository = meetingSessionRepository;
            _scheduleRepository = scheduleRepository;
            _googleTokenRepository = googleTokenRepository;
            _googleCalendarService = googleCalendarService;
            _googleCalendarSettings = googleCalendarSettings.Value;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy MeetingSession theo ID
        /// </summary>
        public async Task<MeetingSessionDto?> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("ID must be greater than 0");
            var entity = await _meetingSessionRepository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<MeetingSessionDto>(entity);
        }

        /// <summary>
        /// Lấy MeetingSession theo ScheduleId
        /// </summary>
        public async Task<MeetingSessionDto?> GetByScheduleIdAsync(int scheduleId)
        {
            if (scheduleId <= 0) throw new ArgumentException("ScheduleId must be greater than 0");
            var entity = await _meetingSessionRepository.GetByScheduleIdAsync(scheduleId);
            return entity == null ? null : _mapper.Map<MeetingSessionDto>(entity);
        }

        /// <summary>
        /// Tạo MeetingSession mới và tạo Google Calendar event, đồng bộ Start/End từ Google response
        /// </summary>
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

            // Get OrganizerEmail from settings
            var organizerEmail = _googleCalendarSettings.SystemAccountEmail;
            if (string.IsNullOrWhiteSpace(organizerEmail))
                throw new Exception("SystemAccountEmail không được cấu hình");

            // Validate OrganizerEmail exists in GoogleToken
            var googleToken = await _googleTokenRepository.GetByEmailAsync(organizerEmail)
                ?? throw new Exception("SystemAccountEmail không tồn tại trong hệ thống GoogleToken");

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

            // Calculate session number
            var allSchedules = (await _scheduleRepository.GetAllByBookingIdOrderedAsync(booking.Id)).ToList();
            var sessionNumber = allSchedules.FindIndex(s => s.Id == schedule.Id) + 1;
            var totalSessions = booking.TotalSessions;

            // Build summary and description
            var subjectName = subject.SubjectName;
            var levelName = level?.Name ?? "Tất cả cấp độ";
            var summary = $"Buổi {sessionNumber}/{totalSessions} - {subjectName} - {levelName}";
            var description = $"Buổi học EduMatch - Buổi {sessionNumber}/{totalSessions}\n" +
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
                AttendeeEmails = new List<string> { booking.LearnerEmail, tutorEmail, organizerEmail }
            };

            // Create Google Calendar Event and get response
            var googleEventResponse = await _googleCalendarService.CreateEventAsync(meetingRequest)
                ?? throw new Exception("Không thể tạo Google Calendar Event");

            // Extract data from Google response
            var meetLink = googleEventResponse.HangoutLink ?? googleEventResponse.HtmlLink;
            var meetCode = googleEventResponse.ConferenceData?.ConferenceId;
            var eventId = googleEventResponse.EventId;

            // Đồng bộ Start/End từ Google 
            if (!string.IsNullOrEmpty(googleEventResponse.Start?.DateTime) && DateTime.TryParse(googleEventResponse.Start.DateTime, out var gStart))
                startTime = gStart;
            if (!string.IsNullOrEmpty(googleEventResponse.End?.DateTime) && DateTime.TryParse(googleEventResponse.End.DateTime, out var gEnd))
                endTime = gEnd;

            var now = DateTime.UtcNow;

            // Create MeetingSession entity
            var entity = new MeetingSession
            {
                ScheduleId = request.ScheduleId,
                OrganizerEmail = organizerEmail,
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

        /// <summary>
        /// Cập nhật MeetingSession, tính lại Start/End từ Schedule, cập nhật Google Calendar event và đồng bộ từ response
        /// </summary>
        public async Task<MeetingSessionDto> UpdateAsync(MeetingSessionUpdateRequest request)
        {
            var entity = await _meetingSessionRepository.GetByIdAsync(request.Id)
                ?? throw new Exception("MeetingSession không tồn tại");

            var shouldUpdateGoogleEvent = false;

            // 1-1: Không cho phép đổi ScheduleId của MeetingSession; chỉ tính lại thời gian từ Schedule hiện tại
            if (request.ScheduleId.HasValue)
            {
                if (request.ScheduleId.Value != entity.ScheduleId)
                    throw new Exception("Không thể thay đổi ScheduleId của MeetingSession");

                var schedule = await _scheduleRepository.GetByIdAsync(entity.ScheduleId)
                    ?? throw new Exception("Schedule không tồn tại");

                if (schedule.Availabiliti == null)
                    throw new Exception("Schedule không có TutorAvailability");

                if (schedule.Availabiliti.Slot == null)
                    throw new Exception("TutorAvailability không có TimeSlot");

                // Tính lại StartTime/EndTime từ Availability/Slot hiện tại của Schedule
                var availability = schedule.Availabiliti;
                var slot = availability.Slot;
                var newStartTime = availability.StartDate.Date.Add(slot.StartTime.ToTimeSpan());
                var newEndTime = availability.StartDate.Date.Add(slot.EndTime.ToTimeSpan());

                if (newStartTime >= newEndTime)
                    throw new Exception("Thời gian slot không hợp lệ: StartTime phải nhỏ hơn EndTime");

                // Luôn cập nhật Google Event theo thời gian từ Schedule
                // Thời gian của MeetingSession sẽ lấy từ response của Google sau khi update thành công
                entity.StartTime = newStartTime;
                entity.EndTime = newEndTime;
                shouldUpdateGoogleEvent = true;
            }

            // If MeetingType is Makeup, update Google Calendar Event
            if (request.MeetingType.HasValue)
            {
                entity.MeetingType = (int)request.MeetingType.Value;
                
                // If changing to Makeup, update Google Event
                if (request.MeetingType.Value == MeetingType.Makeup)
                {
                    shouldUpdateGoogleEvent = true;
                }
            }

            // Update Google Calendar Event if needed
            if (shouldUpdateGoogleEvent && !string.IsNullOrEmpty(entity.EventId))
            {
                // Get schedule for building meeting request (use entity.ScheduleId - already updated if changed)
                var schedule = await _scheduleRepository.GetByIdAsync(entity.ScheduleId)
                    ?? throw new Exception("Schedule không tồn tại");

                var booking = schedule.Booking ?? throw new Exception("Schedule không có Booking");
                var tutorSubject = booking.TutorSubject ?? throw new Exception("Booking không có TutorSubject");
                var tutor = tutorSubject.Tutor ?? throw new Exception("TutorSubject không có Tutor");
                var subject = tutorSubject.Subject ?? throw new Exception("TutorSubject không có Subject");
                var level = tutorSubject.Level ?? throw new Exception("TutorSubject không có Level");

                var tutorEmail = tutor.UserEmail;
                var subjectName = subject.SubjectName;
                var levelName = level?.Name ?? "Tất cả cấp độ";

                // Calculate session number
                var allSchedules = (await _scheduleRepository.GetAllByBookingIdOrderedAsync(booking.Id)).ToList();
                var sessionNumber = allSchedules.FindIndex(s => s.Id == schedule.Id) + 1;
                var totalSessions = booking.TotalSessions;

                var summary = $"Buổi {sessionNumber}/{totalSessions} - {subjectName} - {levelName}";
                if (entity.MeetingType == (int)MeetingType.Makeup)
                {
                    summary = $"[HỌC BÙ] {summary}";
                }

                var description = $"Buổi học EduMatch - Buổi {sessionNumber}/{totalSessions}\n" +
                                 $"Môn học: {subjectName}\n" +
                                 $"Cấp độ: {levelName}\n" +
                                 $"Học viên: {booking.LearnerEmail}\n" +
                                 $"Gia sư: {tutorEmail}\n" +
                                 $"Schedule ID: {schedule.Id}";

                // Kiểm tra SystemAccountEmail trước khi cập nhật Google Event
                var organizerEmail = _googleCalendarSettings.SystemAccountEmail;
                if (string.IsNullOrWhiteSpace(organizerEmail))
                {
                    throw new Exception("SystemAccountEmail không được cấu hình");
                }

                var meetingRequest = new CreateMeetingRequest
                {
                    Summary = summary,
                    Description = description,
                    StartTime = entity.StartTime, //  already updated if AvailabilitiId changed
                    EndTime = entity.EndTime,     //  already updated if AvailabilitiId changed
                    AttendeeEmails = new List<string> { booking.LearnerEmail, tutorEmail, organizerEmail }
                };

                var googleEventResponse = await _googleCalendarService.UpdateEventAsync(entity.EventId, meetingRequest);
                if (googleEventResponse != null)
                {
                    // Đồng bộ Start/End từ Google
                    if (!string.IsNullOrEmpty(googleEventResponse.Start?.DateTime) && DateTime.TryParse(googleEventResponse.Start.DateTime, out var gStart))
                        entity.StartTime = gStart;
                    if (!string.IsNullOrEmpty(googleEventResponse.End?.DateTime) && DateTime.TryParse(googleEventResponse.End.DateTime, out var gEnd))
                        entity.EndTime = gEnd;

                    // Update MeetLink và MeetCode nếu có thay đổi
                    var newMeetLink = googleEventResponse.HangoutLink ?? googleEventResponse.HtmlLink;
                    if (!string.IsNullOrEmpty(newMeetLink))
                        entity.MeetLink = newMeetLink;

                    var newMeetCode = googleEventResponse.ConferenceData?.ConferenceId;
                    if (!string.IsNullOrEmpty(newMeetCode))
                        entity.MeetCode = newMeetCode;
                }
            }

            entity.UpdatedAt = DateTime.UtcNow;

            await _meetingSessionRepository.UpdateAsync(entity);
            return _mapper.Map<MeetingSessionDto>(entity);
        }

        /// <summary>
        /// Xóa MeetingSession và Google Calendar event theo EventId
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("ID must be greater than 0");
            var entity = await _meetingSessionRepository.GetByIdAsync(id);
            if (entity == null)
                return;

            if (!string.IsNullOrEmpty(entity.EventId))
            {
                try
                {
                    await _googleCalendarService.DeleteEventAsync(entity.EventId);
                }
                catch
                {
                    // Rethrow to ensure caller knows Google deletion failed
                    throw;
                }
            }

            await _meetingSessionRepository.DeleteAsync(id);
        }
    }
}

