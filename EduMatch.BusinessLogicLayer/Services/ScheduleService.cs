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
using EduMatch.DataAccessLayer.Interfaces;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IBookingService _bookingService;
        private readonly IMapper _mapper;
        private readonly IMeetingSessionService _meetingSessionService;
        private readonly ITutorAvailabilityService _tutorAvailabilityService;
        private readonly ITutorProfileService _tutorProfileService;
        private readonly IScheduleCompletionRepository? _scheduleCompletionRepository;
        private readonly ITutorPayoutRepository? _tutorPayoutRepository;
        private readonly IUnitOfWork? _unitOfWork;

        public ScheduleService(
            IScheduleRepository scheduleRepository,
            IBookingService bookingService,
            IMeetingSessionService meetingSessionService,
            ITutorAvailabilityService tutorAvailabilityService,
            ITutorProfileService tutorProfileService,
            IScheduleCompletionRepository? scheduleCompletionRepository,
            ITutorPayoutRepository? tutorPayoutRepository,
            IUnitOfWork? unitOfWork,
            IMapper mapper)
        {
            _scheduleRepository = scheduleRepository;
            _bookingService = bookingService;
            _meetingSessionService = meetingSessionService;
            _tutorAvailabilityService = tutorAvailabilityService;
            _tutorProfileService = tutorProfileService;
            _scheduleCompletionRepository = scheduleCompletionRepository;
            _tutorPayoutRepository = tutorPayoutRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public ScheduleService(
            IScheduleRepository scheduleRepository,
            IBookingService bookingService,
            IMeetingSessionService meetingSessionService,
            ITutorAvailabilityService tutorAvailabilityService,
            ITutorProfileService tutorProfileService,
            IMapper mapper)
            : this(scheduleRepository, bookingService, meetingSessionService, tutorAvailabilityService, tutorProfileService, null, null, null, mapper)
        {
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
                throw new ArgumentException("AvailabilitiId must be greater than 0");
            var entity = await _scheduleRepository.GetByAvailabilityIdAsync(availabilitiId);
            return entity == null ? null : _mapper.Map<ScheduleDto>(entity);
        }

        /// <summary>
        /// Lấy Schedule theo ID
        /// </summary>
        public async Task<ScheduleDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID must be greater than 0");
            var entity = await _scheduleRepository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<ScheduleDto>(entity);
        }

        /// <summary>
        /// Tạo Schedule mới và cập nhật TutorAvailability status sang Booked
        /// </summary>
        public async Task<ScheduleDto> CreateAsync(ScheduleCreateRequest request)
        {
            // Validate TutorAvailability exists
            var availability = await _tutorAvailabilityService.GetByIdFullAsync(request.AvailabilitiId)
                ?? throw new Exception("TutorAvailability không tồn tại");

            // Availability phải đang trống
            if (availability.Status != TutorAvailabilityStatus.Available)
                throw new Exception("TutorAvailability không ở trạng thái Available");

            // Validate Booking exists
            var bookingDto = await _bookingService.GetByIdAsync(request.BookingId)
                ?? throw new Exception("Booking không tồn tại");
            var booking = new { TotalSessions = bookingDto.TotalSessions };

            // Nếu LearnerEmail là tutor: không được đăng ký lịch học trùng với lịch dạy (cùng slot và cùng ngày)
            if (!string.IsNullOrWhiteSpace(bookingDto.LearnerEmail))
            {
                var tutorProfile = await _tutorProfileService.GetByEmailFullAsync(bookingDto.LearnerEmail);
                if (tutorProfile != null)
                {
                    var hasConflict = await HasTutorScheduleConflictAsync(tutorProfile.Id, availability.SlotId, availability.StartDate.Date);
                    if (hasConflict)
                    {
                        var dateLabel = availability.StartDate.Date.ToString("dd/MM/yyyy");
                        var slotLabel = availability.Slot != null
                            ? $"{availability.Slot.StartTime:hh\\:mm}-{availability.Slot.EndTime:hh\\:mm}"
                            : $"SlotId {availability.SlotId}";
                        throw new Exception($"Tài khoản {bookingDto.LearnerEmail} là Gia sư và đã có lịch dạy trùng thời gian: ngày {dateLabel}, khung {slotLabel}. Không thể đăng ký học.");
                    }
                }
            }

            // Không được tạo vượt quá TotalSessions của Booking
            var currentCountForBooking = await _scheduleRepository.CountByBookingIdAndStatusAsync(request.BookingId, null);
            if (currentCountForBooking + 1 > booking.TotalSessions)
                throw new Exception("Số lượng Schedule vượt quá TotalSessions của Booking");

            // AvailabilitiId chưa được sử dụng (trừ khi Schedule trước đó đã bị hủy)
            var existingSchedule = await _scheduleRepository.GetByAvailabilityIdAsync(request.AvailabilitiId);
            if (existingSchedule != null && existingSchedule.Status != (int)ScheduleStatus.Cancelled)
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

            // Create completion & payout records for this schedule
            await CreateCompletionAndPayoutAsync(entity, availability, bookingDto);

            return _mapper.Map<ScheduleDto>(entity);
        }

        /// <summary>
        /// Tạo danh sách Schedule cho một Booking; nếu Booking đã có Schedule thì chặn tạo mới
        /// </summary>
        public async Task<List<ScheduleDto>> CreateListAsync(List<ScheduleCreateRequest> requests)
        {
            if (requests == null || requests.Count == 0)
                throw new Exception("Danh sách request rỗng");

            // Tất cả request phải cùng một BookingId
            var bookingId = requests.First().BookingId;
            if (requests.Any(r => r.BookingId != bookingId))
                throw new Exception("Tất cả Schedule phải thuộc cùng một Booking");

            // Lấy booking và chặn tạo mới nếu đã có Schedule trước đó
            var bookingDto = await _bookingService.GetByIdAsync(bookingId);
            if (bookingDto == null)
                throw new Exception("Booking không tồn tại");

            var currentCount = await _scheduleRepository.CountByBookingIdAndStatusAsync(bookingId, null);
            if (currentCount > 0)
                throw new Exception("Booking này đã có Schedule, không thể tạo thêm");

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
                var availability = await _tutorAvailabilityService.GetByIdFullAsync(request.AvailabilitiId.Value)
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
                    if (availability.Status != TutorAvailabilityStatus.Available)
                        throw new Exception("TutorAvailability mới không ở trạng thái Available");

                    // Availability mới phải cách thời điểm hiện tại tối thiểu 12h (StartDate đã theo giờ VN)
                    const double MIN_HOURS_FROM_NOW = 12;
                    // So sánh theo giờ Việt Nam, tránh lệch timezone của máy chủ (Ubuntu/Coolify dùng "Asia/Ho_Chi_Minh")
                    TimeZoneInfo vnTz;
                    try
                    {
                        vnTz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
                    }
                    catch (TimeZoneNotFoundException)
                    {
                        // Fallback cho môi trường Windows / một số distro
                        vnTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    }
					var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTz);

					// Availability.StartDate giữ nguyên (đã là giờ VN), chỉ quy đổi "hiện tại" sang VN
					var availabilityStart = availability.StartDate;
                    var hoursDiff = (availabilityStart - nowVn).TotalHours;
                    if (hoursDiff < MIN_HOURS_FROM_NOW)
                        throw new Exception("TutorAvailability mới phải cách thời điểm hiện tại tối thiểu 12 giờ.");

                    // Nếu LearnerEmail là tutor: không được đổi sang khung trùng với lịch dạy (cùng slot và cùng ngày)
                    var bookingDtoForUpdate = await _bookingService.GetByIdAsync(entity.BookingId)
                        ?? throw new Exception("Booking không tồn tại");
                    if (!string.IsNullOrWhiteSpace(bookingDtoForUpdate.LearnerEmail))
                    {
                        var tutorProfile = await _tutorProfileService.GetByEmailFullAsync(bookingDtoForUpdate.LearnerEmail);
                        if (tutorProfile != null)
                        {
                            var hasConflict = await HasTutorScheduleConflictAsync(tutorProfile.Id, availability.SlotId, availability.StartDate.Date);
                            if (hasConflict)
                            {
                                var dateLabel = availability.StartDate.Date.ToString("dd/MM/yyyy");
                                var slotLabel = availability.Slot != null
                                    ? $"{availability.Slot.StartTime:hh\\:mm}-{availability.Slot.EndTime:hh\\:mm}"
                                    : $"SlotId {availability.SlotId}";
                                throw new Exception($"Tài khoản {bookingDtoForUpdate.LearnerEmail} là Gia sư và đã có lịch dạy trùng thời gian: ngày {dateLabel}, khung {slotLabel}. Không thể đổi lịch.");
                            }
                        }
                    }
                }

                entity.AvailabilitiId = request.AvailabilitiId.Value;
            }

            // Validate Booking if being updated
            if (request.BookingId.HasValue)
            {
                var booking = await _bookingService.GetByIdAsync(request.BookingId.Value)
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
                // Kiểm tra xem Schedule có MeetingSession hay không
                var meetingSession = await _meetingSessionService.GetByScheduleIdAsync(entity.Id);
                
                if (meetingSession != null)
                {
                    // Nếu đã có MeetingSession, luôn cập nhật nó (dù IsOnline = false)
                    await _meetingSessionService.UpdateAsync(new MeetingSessionUpdateRequest
                    {
                        Id = meetingSession.Id,
                        ScheduleId = entity.Id
                    });
                }
                else if (request.IsOnline == true)
                {
                    // Chỉ tạo mới MeetingSession khi IsOnline = true rõ ràng (không mặc định)
                    // Offline -> Online và chưa có MeetingSession thì tạo mới
                    await _meetingSessionService.CreateAsync(new MeetingSessionCreateRequest
                    {
                        ScheduleId = entity.Id
                    });
                }
                // Nếu không có MeetingSession và IsOnline = false hoặc null: không làm gì (giữ nguyên offline)

                // Cập nhật trạng thái Availability: old -> Available, new -> Booked
                await _tutorAvailabilityService.UpdateStatusAsync(oldAvailabilityId, TutorAvailabilityStatus.Available);
                await _tutorAvailabilityService.UpdateStatusAsync(request.AvailabilitiId.Value, TutorAvailabilityStatus.Booked);
            }

            // Không đổi AvailabilitiId nhưng yêu cầu Online: đảm bảo MeetingSession tồn tại (offline -> online)
            if (!availabilityIdChanged && request.IsOnline.HasValue && request.IsOnline.Value)
            {
                var meetingSession = await _meetingSessionService.GetByScheduleIdAsync(entity.Id);
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
            if (id <= 0)
                throw new ArgumentException("ID must be greater than 0");
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

        /// <summary>
        /// Lấy tất cả lịch học của Learner theo email (có thể lọc theo khoảng thời gian và Status)
        /// </summary>
        public async Task<List<ScheduleDto>> GetAllByLearnerEmailAsync(string learnerEmail, DateTime? startDate = null, DateTime? endDate = null, ScheduleStatus? status = null)
        {
            if (string.IsNullOrWhiteSpace(learnerEmail))
                throw new Exception("LearnerEmail không được để trống");

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
                throw new Exception("StartDate không được lớn hơn EndDate");

            int? statusInt = status.HasValue ? (int?)status.Value : null;
            var entities = await _scheduleRepository.GetAllByLearnerEmailAsync(learnerEmail, startDate, endDate, statusInt);
            return _mapper.Map<List<ScheduleDto>>(entities);
        }

        /// <summary>
        /// Lấy tất cả lịch dạy của Tutor theo email (có thể lọc theo khoảng thời gian và Status)
        /// </summary>
        public async Task<List<ScheduleDto>> GetAllByTutorEmailAsync(string tutorEmail, DateTime? startDate = null, DateTime? endDate = null, ScheduleStatus? status = null)
        {
            if (string.IsNullOrWhiteSpace(tutorEmail))
                throw new Exception("TutorEmail không được để trống");

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
                throw new Exception("StartDate không được lớn hơn EndDate");

            int? statusInt = status.HasValue ? (int?)status.Value : null;
            var entities = await _scheduleRepository.GetAllByTutorEmailAsync(tutorEmail, startDate, endDate, statusInt);
            return _mapper.Map<List<ScheduleDto>>(entities);
        }

        /// <summary>
        /// Lấy một số buổi dạy của Tutor theo email và status, sắp xếp theo thời gian tăng dần (mặc định lấy 1 buổi).
        /// Nếu API không truyền status thì controller sẽ mặc định là Upcoming.
        /// </summary>
        public async Task<List<ScheduleDto>> GetByTutorEmailAndStatusAsync(string tutorEmail, ScheduleStatus status, int bookingId, int take = 1)
        {
            if (string.IsNullOrWhiteSpace(tutorEmail))
                throw new Exception("TutorEmail không được để trống");

            if (!Enum.IsDefined(typeof(ScheduleStatus), status))
                throw new Exception("Status không hợp lệ");

            if (bookingId <= 0)
                throw new Exception("BookingId phải lớn hơn 0");

            // Kiểm tra booking tồn tại và thuộc về tutor
            var bookingDto = await _bookingService.GetByIdAsync(bookingId);
            if (bookingDto == null)
                throw new Exception("Booking không tồn tại");

            // Kiểm tra booking có thuộc về tutor không
            if (bookingDto.TutorSubject == null || bookingDto.TutorSubject.TutorEmail != tutorEmail)
                throw new Exception("Booking không thuộc về tutor này");

            int statusInt = (int)status;
            // Lấy schedule theo bookingId và status
            var entities = await _scheduleRepository.GetAllByBookingIdAndStatusNoPagingAsync(bookingId, statusInt);
            var dtos = _mapper.Map<List<ScheduleDto>>(entities);

            // Lấy theo giờ Việt Nam (UTC+7) để đảm bảo là lịch sắp dạy
            var nowVn = DateTime.UtcNow.AddHours(7);

            return dtos
                .Where(s => s.Availability != null && s.Availability.StartDate >= nowVn)
                .OrderBy(s => s.Availability!.StartDate)
                .ThenBy(s => s.Id)
                .Take(take)
                .ToList();
        }

        /// <summary>
        /// Trả về thống kê số buổi đã học/chưa học/đã hủy theo booking
        /// </summary>
        public async Task<ScheduleAttendanceSummaryDto> GetAttendanceSummaryByBookingAsync(int bookingId)
        {
            if (bookingId <= 0)
                throw new ArgumentException("BookingId must be greater than 0");

            var studied = await _scheduleRepository.CountByBookingIdAndStatusAsync(bookingId, (int)ScheduleStatus.Completed);
            var upcoming = await _scheduleRepository.CountByBookingIdAndStatusAsync(bookingId, (int)ScheduleStatus.Upcoming);
            var inProgress = await _scheduleRepository.CountByBookingIdAndStatusAsync(bookingId, (int)ScheduleStatus.InProgress);
            var pending = await _scheduleRepository.CountByBookingIdAndStatusAsync(bookingId, (int)ScheduleStatus.Pending);
            var processing = await _scheduleRepository.CountByBookingIdAndStatusAsync(bookingId, (int)ScheduleStatus.Processing);
            var cancelled = await _scheduleRepository.CountByBookingIdAndStatusAsync(bookingId, (int)ScheduleStatus.Cancelled);

            return new ScheduleAttendanceSummaryDto
            {
                Studied = studied,
                NotStudiedYet = upcoming + inProgress + pending + processing,
                Cancelled = cancelled
            };
        }

        /// <summary>
        /// Kiểm tra tutor có lịch học trùng với slot và ngày hay không (loại trừ Schedule bị Cancelled)
        /// </summary>
        public async Task<bool> HasTutorScheduleConflictAsync(int tutorId, int slotId, DateTime date)
        {
            if (tutorId <= 0) throw new Exception("TutorId phải lớn hơn 0");
            if (slotId <= 0) throw new Exception("SlotId phải lớn hơn 0");
            return await _scheduleRepository.HasTutorScheduleOnSlotDateAsync(tutorId, slotId, date);
        }

        /// <summary>
        /// Cập nhật Status của Schedule (chỉ cho phép update tiến dần: status mới phải >= status cũ theo giá trị enum)
        /// Flow: Upcoming (0) → InProgress (1) → Pending (2) → Processing (3) → Completed (4)
        /// Cancelled (5) có thể được set từ bất kỳ status nào
        /// </summary>
        public async Task<ScheduleDto> UpdateStatusAsync(int id, ScheduleStatus status)
        {
            if (id <= 0)
                throw new ArgumentException("ID phải lớn hơn 0");

            var entity = await _scheduleRepository.GetByIdAsync(id)
                ?? throw new Exception("Schedule không tồn tại");

            var oldStatus = (ScheduleStatus)entity.Status;

            // Chỉ cho phép update tiến dần: status mới phải >= status cũ (theo giá trị enum)
            if ((int)status < (int)oldStatus)
            {
                throw new Exception($"Không thể cập nhật Status từ {oldStatus} về {status}. Chỉ cho phép chuyển từ status nhỏ hơn sang status lớn hơn");
            }

            entity.Status = (int)status;
            entity.UpdatedAt = DateTime.Now;

            await _scheduleRepository.UpdateAsync(entity);

            return _mapper.Map<ScheduleDto>(entity);
        }

        private async Task CreateCompletionAndPayoutAsync(Schedule schedule, TutorAvailabilityDto availability, BookingDto bookingDto)
        {
            if (_scheduleCompletionRepository == null || _tutorPayoutRepository == null || _unitOfWork == null)
                throw new InvalidOperationException("Completion/payout dependencies are not configured.");

            var vietnamTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var lessonDate = availability.StartDate.Date;
            var endTime = availability.Slot?.EndTime ?? TimeOnly.MinValue;
            var confirmationDeadline = lessonDate.Add(endTime.ToTimeSpan()).AddDays(3);
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTz);

            var completion = new ScheduleCompletion
            {
                ScheduleId = schedule.Id,
                BookingId = schedule.BookingId,
                TutorId = availability.TutorId,
                LearnerEmail = bookingDto.LearnerEmail,
                Status = (byte)ScheduleCompletionStatus.PendingConfirm,
                ConfirmationDeadline = confirmationDeadline,
                CreatedAt = now
            };
            await _scheduleCompletionRepository.AddAsync(completion);

            var totalSessions = bookingDto.TotalSessions <= 0 ? 1 : bookingDto.TotalSessions;
            var perSessionTutor = decimal.Round(bookingDto.TutorReceiveAmount / totalSessions, 2, MidpointRounding.AwayFromZero);
            var perSessionFee = decimal.Round(bookingDto.SystemFeeAmount / totalSessions, 2, MidpointRounding.AwayFromZero);

            var tutorEmail = bookingDto.TutorSubject?.TutorEmail ?? availability.Tutor?.UserEmail;
            if (string.IsNullOrWhiteSpace(tutorEmail))
                throw new InvalidOperationException("Tutor email not found for payout creation.");

            var tutorWallet = await GetOrCreateWalletAsync(tutorEmail);

            var payout = new TutorPayout
            {
                ScheduleId = schedule.Id,
                BookingId = schedule.BookingId,
                TutorWalletId = tutorWallet.Id,
                Amount = perSessionTutor,
                SystemFeeAmount = perSessionFee,
                Status = (byte)TutorPayoutStatus.Pending,
                PayoutTrigger = (byte)TutorPayoutTrigger.None,
                ScheduledPayoutDate = DateOnly.FromDateTime(now),
                CreatedAt = now
            };

            await _tutorPayoutRepository.AddAsync(payout);
            await _unitOfWork.CompleteAsync();
        }

        private async Task<Wallet> GetOrCreateWalletAsync(string userEmail)
        {
            var wallet = await _unitOfWork.Wallets.GetWalletByUserEmailAsync(userEmail);
            if (wallet != null)
            {
                return wallet;
            }

            var newWallet = new Wallet
            {
                UserEmail = userEmail,
                Balance = 0,
                LockedBalance = 0,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Wallets.AddAsync(newWallet);
            await _unitOfWork.CompleteAsync();
            return newWallet;
        }

        /// <summary>
        /// Helper to confirm a schedule and release payout immediately via completion service; does not alter existing flows.
        /// </summary>
        public Task<bool> FinishAndPayScheduleAsync(int scheduleId, IScheduleCompletionService completionService)
        {
            if (completionService == null) throw new ArgumentNullException(nameof(completionService));
            return completionService.FinishAndPayAsync(scheduleId);
        }



    }
}
