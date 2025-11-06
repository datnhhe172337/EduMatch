using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.Booking;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ITutorSubjectRepository _tutorSubjectRepository;
        private readonly ISystemFeeRepository _systemFeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IMeetingSessionRepository _meetingSessionRepository;
        private readonly ITutorAvailabilityRepository _tutorAvailabilityRepository;
        private readonly IGoogleCalendarService _googleCalendarService;

        public BookingService(
            IBookingRepository bookingRepository,
            ITutorSubjectRepository tutorSubjectRepository,
            ISystemFeeRepository systemFeeRepository,
            IUserRepository userRepository,
            IMapper mapper,
            IScheduleRepository scheduleRepository,
            IMeetingSessionRepository meetingSessionRepository,
            ITutorAvailabilityRepository tutorAvailabilityRepository,
            IGoogleCalendarService googleCalendarService)
        {
            _bookingRepository = bookingRepository;
            _tutorSubjectRepository = tutorSubjectRepository;
            _systemFeeRepository = systemFeeRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _scheduleRepository = scheduleRepository;
            _meetingSessionRepository = meetingSessionRepository;
            _tutorAvailabilityRepository = tutorAvailabilityRepository;
            _googleCalendarService = googleCalendarService;
        }

        /// <summary>
        /// Lấy danh sách Booking theo learnerEmail với phân trang và lọc theo status, tutorSubjectId
        /// </summary>
        public async Task<List<BookingDto>> GetAllByLearnerEmailAsync(string email, BookingStatus? status, int? tutorSubjectId, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("Email không được để trống");
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            int? statusInt = status.HasValue ? (int?)status.Value : null;
            var entities = await _bookingRepository.GetAllByLearnerEmailAsync(email, statusInt, tutorSubjectId, page, pageSize);
            return _mapper.Map<List<BookingDto>>(entities);
        }

        /// <summary>
        /// Đếm tổng số Booking theo learnerEmail với lọc theo status, tutorSubjectId
        /// </summary>
        public Task<int> CountByLearnerEmailAsync(string email, BookingStatus? status, int? tutorSubjectId)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("Email không được để trống");
            int? statusInt = status.HasValue ? (int?)status.Value : null;
            return _bookingRepository.CountByLearnerEmailAsync(email, statusInt, tutorSubjectId);
        }

        /// <summary>
        /// Lấy danh sách Booking theo learnerEmail (không phân trang)
        /// </summary>
        public async Task<List<BookingDto>> GetAllByLearnerEmailNoPagingAsync(string email, BookingStatus? status, int? tutorSubjectId)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("Email không được để trống");
            int? statusInt = status.HasValue ? (int?)status.Value : null;
            var entities = await _bookingRepository.GetAllByLearnerEmailNoPagingAsync(email, statusInt, tutorSubjectId);
            return _mapper.Map<List<BookingDto>>(entities);
        }

     
        /// <summary>
        /// Lấy danh sách Booking theo tutorId với phân trang và lọc theo status, tutorSubjectId
        /// </summary>
        public async Task<List<BookingDto>> GetAllByTutorIdAsync(int tutorId, BookingStatus? status, int? tutorSubjectId, int page = 1, int pageSize = 10)
        {
            if (tutorId <= 0)
                throw new Exception("TutorId phải lớn hơn 0");
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            int? statusInt = status.HasValue ? (int?)status.Value : null;
            var entities = await _bookingRepository.GetAllByTutorIdAsync(tutorId, statusInt, tutorSubjectId, page, pageSize);
            return _mapper.Map<List<BookingDto>>(entities);
        }

        /// <summary>
        /// Đếm tổng số Booking theo tutorId với lọc theo status, tutorSubjectId
        /// </summary>
        public Task<int> CountByTutorIdAsync(int tutorId, BookingStatus? status, int? tutorSubjectId)
        {
            if (tutorId <= 0)
                throw new Exception("TutorId phải lớn hơn 0");
            int? statusInt = status.HasValue ? (int?)status.Value : null;
            return _bookingRepository.CountByTutorIdAsync(tutorId, statusInt, tutorSubjectId);
        }

        /// <summary>
        /// Lấy danh sách Booking theo tutorId (không phân trang)
        /// </summary>
        public async Task<List<BookingDto>> GetAllByTutorIdNoPagingAsync(int tutorId, BookingStatus? status, int? tutorSubjectId)
        {
            if (tutorId <= 0)
                throw new Exception("TutorId phải lớn hơn 0");
            int? statusInt = status.HasValue ? (int?)status.Value : null;
            var entities = await _bookingRepository.GetAllByTutorIdNoPagingAsync(tutorId, statusInt, tutorSubjectId);
            return _mapper.Map<List<BookingDto>>(entities);
        }

        /// <summary>
        /// Lấy Booking theo ID
        /// </summary>
        public async Task<BookingDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new Exception("Id phải lớn hơn 0");
            var entity = await _bookingRepository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<BookingDto>(entity);
        }

        /// <summary>
        /// Tạo Booking mới và tính phí hệ thống theo SystemFee đang hoạt động
        /// </summary>
        public async Task<BookingDto> CreateAsync(BookingCreateRequest request)
        {
            // Validate LearnerEmail exists
            var learner = await _userRepository.GetUserByEmailAsync(request.LearnerEmail)
                ?? throw new Exception("LearnerEmail không tồn tại trong hệ thống");

            // Validate TutorSubject exists and get HourlyRate
            var tutorSubject = await _tutorSubjectRepository.GetByIdFullAsync(request.TutorSubjectId)
                ?? throw new Exception("TutorSubject không tồn tại");

            if (!tutorSubject.HourlyRate.HasValue || tutorSubject.HourlyRate.Value <= 0)
                throw new Exception("TutorSubject không có giá hợp lệ");

            var unitPrice = tutorSubject.HourlyRate.Value;
            var totalSessions = request.TotalSessions ?? 1;

            // Get active SystemFee
            var activeSystemFee = await _systemFeeRepository.GetActiveSystemFeeAsync()
                ?? throw new Exception("Không tìm thấy SystemFee đang hoạt động");

            var now = DateTime.UtcNow;

            // Calculate base amount (tổng đơn hàng)
            var baseAmount = unitPrice * totalSessions;

            // Calculate SystemFeeAmount

            decimal systemFeeAmount = 0;
            if (activeSystemFee.Percentage.HasValue)
            {
                // Phí % tính trên tổng đơn hàng
                systemFeeAmount += baseAmount * (activeSystemFee.Percentage.Value / 100);
            }
            if (activeSystemFee.FixedAmount.HasValue)
            {
                // Phí cố định tính một lần trên tổng đơn hàng
                systemFeeAmount += activeSystemFee.FixedAmount.Value;
            }

            // TotalAmount giữ nguyên giá gốc
            var totalAmount = baseAmount;

            // Create Booking entity
            var entity = new Booking
            {
                LearnerEmail = request.LearnerEmail,
                TutorSubjectId = request.TutorSubjectId,
                BookingDate = now,
                TotalSessions = totalSessions,
                UnitPrice = unitPrice,
                TotalAmount = totalAmount,
                PaymentStatus = (int)PaymentStatus.Pending,
                RefundedAmount = 0,
                Status = (int)BookingStatus.Pending,
                SystemFeeId = activeSystemFee.Id,
                SystemFeeAmount = systemFeeAmount,
                CreatedAt = now,
                UpdatedAt = null
            };

            await _bookingRepository.CreateAsync(entity);
            return _mapper.Map<BookingDto>(entity);
        }

        /// <summary>
        /// Cập nhật Booking và tính lại các giá trị liên quan khi thay đổi
        /// </summary>
        public async Task<BookingDto> UpdateAsync(BookingUpdateRequest request)
        {
            var entity = await _bookingRepository.GetByIdAsync(request.Id)
                ?? throw new Exception("Booking không tồn tại");

            if (!string.IsNullOrWhiteSpace(request.LearnerEmail))
            {
                // Validate LearnerEmail exists
                var learner = await _userRepository.GetUserByEmailAsync(request.LearnerEmail)
                    ?? throw new Exception("LearnerEmail không tồn tại trong hệ thống");
                entity.LearnerEmail = request.LearnerEmail;
            }

            if (request.TutorSubjectId.HasValue)
            {
                var tutorSubject = await _tutorSubjectRepository.GetByIdFullAsync(request.TutorSubjectId.Value)
                    ?? throw new Exception("TutorSubject không tồn tại");
                entity.TutorSubjectId = request.TutorSubjectId.Value;
                // Update UnitPrice if TutorSubject changes
                if (tutorSubject.HourlyRate.HasValue && tutorSubject.HourlyRate.Value > 0)
                {
                    entity.UnitPrice = tutorSubject.HourlyRate.Value;
                }
            }

            // Recalculate if TotalSessions changes
            if (request.TotalSessions.HasValue && request.TotalSessions.Value > 0)
            {
                entity.TotalSessions = request.TotalSessions.Value;
                
                // Recalculate baseAmount and SystemFeeAmount
                var baseAmount = entity.UnitPrice * entity.TotalSessions;
                
                // Get current SystemFee to recalculate fee
                var systemFee = await _systemFeeRepository.GetByIdAsync(entity.SystemFeeId)
                    ?? throw new Exception("SystemFee không tồn tại");
                
                // Recalculate SystemFeeAmount
                decimal systemFeeAmount = 0;
                if (systemFee.Percentage.HasValue)
                {
                    systemFeeAmount += baseAmount * (systemFee.Percentage.Value / 100);
                }
                if (systemFee.FixedAmount.HasValue)
                {
                    systemFeeAmount += systemFee.FixedAmount.Value;
                }
                
                entity.SystemFeeAmount = systemFeeAmount;
                // TotalAmount giữ nguyên giá gốc, không cộng phí
                entity.TotalAmount = baseAmount;
            }
            
            entity.UpdatedAt = DateTime.UtcNow;

            await _bookingRepository.UpdateAsync(entity);
            return _mapper.Map<BookingDto>(entity);
        }

        /// <summary>
        /// Xóa Booking theo ID
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            await _bookingRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Cập nhật PaymentStatus của Booking
        /// </summary>
        public async Task<BookingDto> UpdatePaymentStatusAsync(int id, PaymentStatus paymentStatus)
        {
            var entity = await _bookingRepository.GetByIdAsync(id)
                ?? throw new Exception("Booking không tồn tại");

            entity.PaymentStatus = (int)paymentStatus;
            entity.UpdatedAt = DateTime.UtcNow;
            await _bookingRepository.UpdateAsync(entity);
            return _mapper.Map<BookingDto>(entity);
        }

        /// <summary>
        /// Cập nhật Status của Booking. Nếu status là Cancelled thì hủy tất cả Schedule liên quan
        /// </summary>
        public async Task<BookingDto> UpdateStatusAsync(int id, BookingStatus status)
        {
            var entity = await _bookingRepository.GetByIdAsync(id)
                ?? throw new Exception("Booking không tồn tại");

            entity.Status = (int)status;
            entity.UpdatedAt = DateTime.UtcNow;
            await _bookingRepository.UpdateAsync(entity);

            // Nếu status là Cancelled thì hủy tất cả Schedule liên quan
            if (status == BookingStatus.Cancelled)
            {
                await CancelAllSchedulesByBookingAsync(id);
            }

            return _mapper.Map<BookingDto>(entity);
        }

        /// <summary>
        /// Hủy toàn bộ Schedule theo bookingId: set Status=Cancelled, xóa MeetingSession (bao gồm Google Calendar event), trả Availability về Available
        /// </summary>
        private async Task CancelAllSchedulesByBookingAsync(int bookingId)
        {
            var schedules = (await _scheduleRepository.GetAllByBookingIdOrderedAsync(bookingId)).ToList();
            foreach (var schedule in schedules)
            {
                // Delete meeting session first (includes Google event deletion)
                var meetingSession = await _meetingSessionRepository.GetByScheduleIdAsync(schedule.Id);
                if (meetingSession != null)
                {
                    // Xóa Google Calendar event nếu có
                    if (!string.IsNullOrEmpty(meetingSession.EventId))
                    {
                        try
                        {
                            await _googleCalendarService.DeleteEventAsync(meetingSession.EventId);
                        }
                        catch
                        {
                            // Log error but continue
                        }
                    }
                    // Xóa meeting session
                    await _meetingSessionRepository.DeleteAsync(meetingSession.Id);
                }

                // Mark schedule as cancelled
                schedule.Status = (int)ScheduleStatus.Cancelled;
                schedule.UpdatedAt = DateTime.UtcNow;
                await _scheduleRepository.UpdateAsync(schedule);

                // Return availability to Available
                var availability = await _tutorAvailabilityRepository.GetByIdFullAsync(schedule.AvailabilitiId);
                if (availability != null)
                {
                    availability.Status = (int)TutorAvailabilityStatus.Available;
                    await _tutorAvailabilityRepository.UpdateAsync(availability);
                }
            }
        }
    }
}

