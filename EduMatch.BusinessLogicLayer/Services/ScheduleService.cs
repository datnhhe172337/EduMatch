using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Schedule;
using EduMatch.BusinessLogicLayer.Requests.MeetingSession;
using EduMatch.DataAccessLayer.Enum;
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
        private readonly IMeetingSessionService _meetingSessionService;
        private readonly ITutorAvailabilityService _tutorAvailabilityService;

        public ScheduleService(
            IScheduleRepository scheduleRepository,
            IBookingRepository bookingRepository,
            ITutorAvailabilityRepository tutorAvailabilityRepository,
            IMeetingSessionRepository meetingSessionRepository,
            IMeetingSessionService meetingSessionService,
            ITutorAvailabilityService tutorAvailabilityService,
            IMapper mapper)
        {
            _scheduleRepository = scheduleRepository;
            _bookingRepository = bookingRepository;
            _tutorAvailabilityRepository = tutorAvailabilityRepository;
            _meetingSessionRepository = meetingSessionRepository;
            _meetingSessionService = meetingSessionService;
            _tutorAvailabilityService = tutorAvailabilityService;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy danh sách Schedule theo bookingId và status với phân trang
        /// </summary>
        public async Task<List<ScheduleDto>> GetAllByBookingIdAndStatusAsync(int bookingId, ScheduleStatus? status, int page = 1, int pageSize = 10)
        {
            if (bookingId <= 0)
                throw new Exception("BookingId phải lớn hơn 0");
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            int? statusInt = status.HasValue ? (int?)status.Value : null;
            var entities = await _scheduleRepository.GetAllByBookingIdAndStatusAsync(bookingId, statusInt, page, pageSize);
            return _mapper.Map<List<ScheduleDto>>(entities);
        }

        /// <summary>
        /// Lấy danh sách Schedule theo bookingId và status (không phân trang)
        /// </summary>
        public async Task<List<ScheduleDto>> GetAllByBookingIdAndStatusNoPagingAsync(int bookingId, ScheduleStatus? status)
        {
            if (bookingId <= 0)
                throw new Exception("BookingId phải lớn hơn 0");
            int? statusInt = status.HasValue ? (int?)status.Value : null;
            var entities = await _scheduleRepository.GetAllByBookingIdAndStatusNoPagingAsync(bookingId, statusInt);
            return _mapper.Map<List<ScheduleDto>>(entities);
        }

        /// <summary>
        /// Đếm tổng số Schedule theo bookingId và status
        /// </summary>
        public Task<int> CountByBookingIdAndStatusAsync(int bookingId, ScheduleStatus? status)
        {
            if (bookingId <= 0)
                throw new Exception("BookingId phải lớn hơn 0");
            int? statusInt = status.HasValue ? (int?)status.Value : null;
            return _scheduleRepository.CountByBookingIdAndStatusAsync(bookingId, statusInt);
        }

        /// <summary>
        /// Lấy Schedule theo AvailabilityId
        /// </summary>
        public async Task<ScheduleDto?> GetByAvailabilityIdAsync(int availabilitiId)
        {
            if (availabilitiId <= 0)
                throw new Exception("AvailabilitiId phải lớn hơn 0");
            var entity = await _scheduleRepository.GetByAvailabilityIdAsync(availabilitiId);
            return entity == null ? null : _mapper.Map<ScheduleDto>(entity);
        }

        /// <summary>
        /// Lấy Schedule theo ID
        /// </summary>
        public async Task<ScheduleDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new Exception("Id phải lớn hơn 0");
            var entity = await _scheduleRepository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<ScheduleDto>(entity);
        }

        /// <summary>
        /// Tạo Schedule mới và cập nhật TutorAvailability status sang Booked
        /// </summary>
        public async Task<ScheduleDto> CreateAsync(ScheduleCreateRequest request)
        {
            // Validate TutorAvailability exists
            var availability = await _tutorAvailabilityRepository.GetByIdFullAsync(request.AvailabilitiId)
                ?? throw new Exception("TutorAvailability không tồn tại");

            // Availability phải đang trống
            if (availability.Status != (int)TutorAvailabilityStatus.Available)
                throw new Exception("TutorAvailability không ở trạng thái Available");

            // Validate Booking exists
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId)
                ?? throw new Exception("Booking không tồn tại");

            // Không được tạo vượt quá TotalSessions của Booking
            var currentCountForBooking = await _scheduleRepository.CountByBookingIdAndStatusAsync(request.BookingId, null);
            if (currentCountForBooking + 1 > booking.TotalSessions)
                throw new Exception("Số lượng Schedule vượt quá TotalSessions của Booking");

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

            // Đánh dấu availability đã được đặt
            await _tutorAvailabilityService.UpdateStatusAsync(request.AvailabilitiId, TutorAvailabilityStatus.Booked);

            // Nếu là online thì tạo MeetingSession, offline thì bỏ qua
            if (request.IsOnline)
            {
                await _meetingSessionService.CreateAsync(new MeetingSessionCreateRequest
                {
                    ScheduleId = entity.Id
                });
            }

            return _mapper.Map<ScheduleDto>(entity);
        }

        /// <summary>
        /// Tạo danh sách Schedule cho một Booking; tổng sau khi tạo phải bằng TotalSessions của Booking
        /// </summary>
        public async Task<List<ScheduleDto>> CreateListAsync(List<ScheduleCreateRequest> requests)
        {
            if (requests == null || requests.Count == 0)
                throw new Exception("Danh sách request rỗng");

            // Tất cả request phải cùng một BookingId
            var bookingId = requests.First().BookingId;
            if (requests.Any(r => r.BookingId != bookingId))
                throw new Exception("Tất cả Schedule phải thuộc cùng một Booking");

            // Lấy booking và kiểm tra tổng sessions
            var booking = await _bookingRepository.GetByIdAsync(bookingId)
                ?? throw new Exception("Booking không tồn tại");
            var currentCount = await _scheduleRepository.CountByBookingIdAndStatusAsync(bookingId, null);
            var totalAfterCreate = currentCount + requests.Count;
            if (totalAfterCreate != booking.TotalSessions)
                throw new Exception($"Tổng số Schedule sau khi tạo ({totalAfterCreate}) phải bằng TotalSessions ({booking.TotalSessions}) của Booking");

            var created = new List<ScheduleDto>();
            foreach (var req in requests)
            {
                var dto = await CreateAsync(req);
                created.Add(dto);
            }
            return created;
        }

        /// <summary>
        /// Cập nhật Schedule, nếu AvailabilitiId thay đổi thì cập nhật MeetingSession và trạng thái Availability
        /// </summary>
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

                    // Availability mới phải đang trống
                    if (availability.Status != (int)TutorAvailabilityStatus.Available)
                        throw new Exception("TutorAvailability mới không ở trạng thái Available");
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


            entity.UpdatedAt = DateTime.UtcNow;

            await _scheduleRepository.UpdateAsync(entity);

            // Nếu AvailabilitiId thay đổi: (online) cập nhật MeetingSession qua service (đẩy Google) và cập nhật trạng thái Availability
            if (availabilityIdChanged && request.AvailabilitiId.HasValue)
            {
                // Chỉ xử lý MeetingSession nếu online (IsOnline true hoặc không truyền -> coi như online)
                if (!request.IsOnline.HasValue || request.IsOnline.Value)
                {
                    // Update MeetingSession theo Schedule hiện tại (MeetingSessionService sẽ gửi update lên Google và đồng bộ Start/End từ response)
                    var meetingSession = await _meetingSessionRepository.GetByScheduleIdAsync(entity.Id);
                    if (meetingSession != null)
                    {
                        await _meetingSessionService.UpdateAsync(new MeetingSessionUpdateRequest
                        {
                            Id = meetingSession.Id,
                            ScheduleId = entity.Id
                        });
                    }
                    else
                    {
                        // Offline -> Online và chưa có MeetingSession thì tạo mới
                        await _meetingSessionService.CreateAsync(new MeetingSessionCreateRequest
                        {
                            ScheduleId = entity.Id
                        });
                    }
                }

                // Cập nhật trạng thái Availability: old -> Available, new -> Booked
                await _tutorAvailabilityService.UpdateStatusAsync(oldAvailabilityId, TutorAvailabilityStatus.Available);
                await _tutorAvailabilityService.UpdateStatusAsync(request.AvailabilitiId.Value, TutorAvailabilityStatus.Booked);
            }

            // Không đổi AvailabilitiId nhưng yêu cầu Online: đảm bảo MeetingSession tồn tại (offline -> online)
            if (!availabilityIdChanged && request.IsOnline.HasValue && request.IsOnline.Value)
            {
                var meetingSession = await _meetingSessionRepository.GetByScheduleIdAsync(entity.Id);
                if (meetingSession == null)
                {
                    await _meetingSessionService.CreateAsync(new MeetingSessionCreateRequest
                    {
                        ScheduleId = entity.Id
                    });
                }
            }

            return _mapper.Map<ScheduleDto>(entity);
        }

        /// <summary>
        /// Xóa Schedule, xóa MeetingSession (bao gồm Google Calendar event) trước
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            // Xóa MeetingSession trước qua service (service sẽ xóa Google Event rồi xóa DB)
            var meetingSessionDto = await _meetingSessionService.GetByScheduleIdAsync(id);
            if (meetingSessionDto != null)
                await _meetingSessionService.DeleteAsync(meetingSessionDto.Id);

            await _scheduleRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Hủy toàn bộ Schedule theo bookingId: set Status=Cancelled, xóa MeetingSession (bao gồm Google Calendar event), trả Availability về Available
        /// </summary>
        public async Task<List<ScheduleDto>> CancelAllByBookingAsync(int bookingId)
        {
            var schedules = (await _scheduleRepository.GetAllByBookingIdOrderedAsync(bookingId)).ToList();
            var updated = new List<Schedule>();
            foreach (var schedule in schedules)
            {
                // Delete meeting session first (includes Google event deletion)
                var ms = await _meetingSessionService.GetByScheduleIdAsync(schedule.Id);
                if (ms != null)
                    await _meetingSessionService.DeleteAsync(ms.Id);

                // Mark schedule as cancelled
                schedule.Status = (int)ScheduleStatus.Cancelled;
                schedule.UpdatedAt = DateTime.UtcNow;
                await _scheduleRepository.UpdateAsync(schedule);
                updated.Add(schedule);

                // Return availability to Available
                await _tutorAvailabilityService.UpdateStatusAsync(schedule.AvailabilitiId, TutorAvailabilityStatus.Available);
            }
            return _mapper.Map<List<ScheduleDto>>(updated);
        }
    }
}

