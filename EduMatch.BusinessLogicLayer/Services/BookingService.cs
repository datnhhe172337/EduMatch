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
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ILearnerTrialLessonService _learnerTrialLessonService;
        private const string SystemWalletEmail = "system@edumatch.com";
        private readonly TimeZoneInfo _vietnamTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public BookingService(
            IBookingRepository bookingRepository,
            ITutorSubjectRepository tutorSubjectRepository,
            ISystemFeeRepository systemFeeRepository,
            IUserRepository userRepository,
            IMapper mapper,
            IScheduleRepository scheduleRepository,
            IMeetingSessionRepository meetingSessionRepository,
            ITutorAvailabilityRepository tutorAvailabilityRepository,
            IGoogleCalendarService googleCalendarService,
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            ILearnerTrialLessonService learnerTrialLessonService)
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
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _learnerTrialLessonService = learnerTrialLessonService;
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

            var isTrial = request.IsTrial;

            // Trial booking: chỉ được phép 1 buổi
            if (isTrial && totalSessions != 1)
            {
                throw new Exception("Booking học thử chỉ được phép 1 buổi học");
            }

            // Nếu là booking học thử: kiểm tra learner đã dùng buổi học thử với tutor & môn này chưa
            if (isTrial)
            {
                var hasTrialed = await _learnerTrialLessonService.HasTrialedAsync(
                    request.LearnerEmail,
                    tutorSubject.TutorId,
                    tutorSubject.SubjectId);
                if (hasTrialed)
                {
                    throw new Exception("Bạn đã sử dụng buổi học thử miễn phí cho môn này với gia sư này");
                }
            }

            // Get active SystemFee
            var activeSystemFee = await _systemFeeRepository.GetActiveSystemFeeAsync()
                ?? throw new Exception("Không tìm thấy SystemFee đang hoạt động");

            var now = DateTime.UtcNow;

            // Calculate base amount (tổng đơn hàng)
            var baseAmount = unitPrice * totalSessions;

            // Tính phí hệ thống và số tiền gia sư nhận được
            decimal systemFeeAmount = 0;
            decimal totalAmount;
            decimal tutorReceiveAmount;

            if (isTrial)
            {
                // Học thử: miễn phí hoàn toàn
                totalAmount = 0;
                tutorReceiveAmount = 0;
                systemFeeAmount = 0;
            }
            else
            {
                // Booking thường: tính phí hệ thống như cũ
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
                totalAmount = baseAmount;

                // Calculate TutorReceiveAmount (số tiền tutor nhận được sau khi trừ phí hệ thống)
                tutorReceiveAmount = totalAmount - systemFeeAmount;
            }

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
                TutorReceiveAmount = tutorReceiveAmount,
                CreatedAt = now,
                UpdatedAt = null
            };

            await _bookingRepository.CreateAsync(entity);

            // Nếu là booking học thử: ghi lại vào LearnerTrialLesson để lần sau không cho học thử miễn phí nữa
            if (isTrial)
            {
                await _learnerTrialLessonService.RecordTrialAsync(
                    request.LearnerEmail,
                    tutorSubject.TutorId,
                    tutorSubject.SubjectId);
            }

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

            bool needRecalculate = false;

            if (request.TutorSubjectId.HasValue)
            {
                var tutorSubject = await _tutorSubjectRepository.GetByIdFullAsync(request.TutorSubjectId.Value)
                    ?? throw new Exception("TutorSubject không tồn tại");
                entity.TutorSubjectId = request.TutorSubjectId.Value;
                // Update UnitPrice if TutorSubject changes
                if (tutorSubject.HourlyRate.HasValue && tutorSubject.HourlyRate.Value > 0)
                {
                    entity.UnitPrice = tutorSubject.HourlyRate.Value;
                    needRecalculate = true;
                }
            }

            // Recalculate if TotalSessions changes
            if (request.TotalSessions.HasValue && request.TotalSessions.Value > 0)
            {
                entity.TotalSessions = request.TotalSessions.Value;
                needRecalculate = true;
            }

            // Recalculate amounts if needed
            if (needRecalculate)
            {
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
                // Recalculate TutorReceiveAmount
                entity.TutorReceiveAmount = baseAmount - systemFeeAmount;
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
        /// Cập nhật PaymentStatus của Booking (chỉ cho phép cập nhật theo thứ tự tăng dần)
        /// </summary>
        public async Task<BookingDto> UpdatePaymentStatusAsync(int id, PaymentStatus paymentStatus)
        {
            var entity = await _bookingRepository.GetByIdAsync(id)
                ?? throw new Exception("Booking không tồn tại");

            var currentStatus = (PaymentStatus)entity.PaymentStatus;
            var newStatus = paymentStatus;

            // Validate: Chỉ cho phép cập nhật theo thứ tự tăng dần
            // Pending (0) -> Paid (1) -> RefundPending (2) -> Refunded (3)
            if (newStatus <= currentStatus)
                throw new Exception($"Không thể cập nhật PaymentStatus từ {currentStatus} về {newStatus}. Chỉ được phép cập nhật theo thứ tự tăng dần.");

            // Validate: Không cho phép nhảy bước (ví dụ: Pending -> RefundPending)
            if (currentStatus == PaymentStatus.Pending && newStatus != PaymentStatus.Paid)
                throw new Exception("Từ trạng thái Pending chỉ có thể chuyển sang Paid");

            if (currentStatus == PaymentStatus.Paid && newStatus != PaymentStatus.RefundPending)
                throw new Exception("Từ trạng thái Paid chỉ có thể chuyển sang RefundPending");

            if (currentStatus == PaymentStatus.RefundPending && newStatus != PaymentStatus.Refunded)
                throw new Exception("Từ trạng thái RefundPending chỉ có thể chuyển sang Refunded");

            if (currentStatus == PaymentStatus.Refunded)
                throw new Exception("Không thể cập nhật PaymentStatus khi đã ở trạng thái Refunded");

            entity.PaymentStatus = (int)paymentStatus;
            entity.UpdatedAt = DateTime.UtcNow;
            await _bookingRepository.UpdateAsync(entity);
            return _mapper.Map<BookingDto>(entity);
        }

        /// <summary>
        /// Thanh toán booking: trừ số dư ví học viên, khóa tiền và ghi nhận vào hệ thống.
        /// </summary>
        public async Task<BookingDto> PayForBookingAsync(int bookingId, string learnerEmail)
        {
            if (string.IsNullOrWhiteSpace(learnerEmail))
                throw new ArgumentException("Learner email is required.", nameof(learnerEmail));

            var booking = await _bookingRepository.GetByIdAsync(bookingId)
                ?? throw new Exception("Booking không tồn tại");

            if (!booking.LearnerEmail.Equals(learnerEmail, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Bạn không thể thanh toán booking này.");

            if (booking.Status == (int)BookingStatus.Cancelled)
                throw new InvalidOperationException("Booking đã bị hủy.");

            if (booking.PaymentStatus == (int)PaymentStatus.Paid)
                throw new InvalidOperationException("Booking đã được thanh toán.");

            var amountToPay = booking.TotalAmount - booking.RefundedAmount;
            if (amountToPay <= 0)
                throw new InvalidOperationException("Không có số tiền cần thanh toán.");

            var learnerWallet = await GetOrCreateWalletAsync(booking.LearnerEmail);

            if (learnerWallet.Balance < amountToPay)
                throw new InvalidOperationException("Số dư ví không đủ để thanh toán booking.");

            var now = DateTime.UtcNow;
            var learnerBalanceBefore = learnerWallet.Balance;

            learnerWallet.Balance -= amountToPay;
            learnerWallet.LockedBalance += amountToPay;
            learnerWallet.UpdatedAt = now;
            _unitOfWork.Wallets.Update(learnerWallet);

            var referenceCode = $"BOOKING_PAYMENT_{booking.Id}";

            var learnerTransaction = new WalletTransaction
            {
                WalletId = learnerWallet.Id,
                Amount = amountToPay,
                TransactionType = WalletTransactionType.Debit,
                Reason = WalletTransactionReason.BookingPayment,
                Status = TransactionStatus.Pending,
                BalanceBefore = learnerBalanceBefore,
                BalanceAfter = learnerWallet.Balance,
                CreatedAt = now,
                ReferenceCode = referenceCode,
                BookingId = booking.Id
            };
            await _unitOfWork.WalletTransactions.AddAsync(learnerTransaction);

            var systemWallet = await GetOrCreateWalletAsync(SystemWalletEmail);

            var systemLockedBefore = systemWallet.LockedBalance;
            systemWallet.LockedBalance += amountToPay;
            systemWallet.UpdatedAt = now;
            _unitOfWork.Wallets.Update(systemWallet);

            var systemTransaction = new WalletTransaction
            {
                WalletId = systemWallet.Id,
                Amount = amountToPay,
                TransactionType = WalletTransactionType.Credit,
                Reason = WalletTransactionReason.BookingPayment,
                Status = TransactionStatus.Pending,
                BalanceBefore = systemLockedBefore,
                BalanceAfter = systemWallet.LockedBalance,
                CreatedAt = now,
                ReferenceCode = referenceCode,
                BookingId = booking.Id
            };
            await _unitOfWork.WalletTransactions.AddAsync(systemTransaction);

            await _unitOfWork.CompleteAsync();

            booking.PaymentStatus = (int)PaymentStatus.Paid;
            booking.UpdatedAt = now;
            await _bookingRepository.UpdateAsync(booking);

            //await _notificationService.CreateNotificationAsync(
            //    booking.LearnerEmail,
            //    $"Bạn đã thanh toán booking #{booking.Id}. Số tiền {amountToPay:N0} VND đã được khóa và chờ hoàn tất buổi học.",
            //    "/wallet/my-wallet");

            return _mapper.Map<BookingDto>(booking);
        }

        /// <summary>
        /// Huy booking do learner yeu cau, tra lai toan bo so tien con khoa va giai phong lich.
        /// </summary>
        public async Task<BookingDto> CancelByLearnerAsync(int bookingId, string learnerEmail)
        {
            if (bookingId <= 0)
                throw new ArgumentException("BookingId must be greater than 0.");
            if (string.IsNullOrWhiteSpace(learnerEmail))
                throw new ArgumentException("Learner email is required.", nameof(learnerEmail));

            var booking = await _bookingRepository.GetByIdAsync(bookingId)
                ?? throw new Exception("Booking khong ton tai");

            if (!booking.LearnerEmail.Equals(learnerEmail, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Only the booking owner can cancel this booking.");

            var currentStatus = (BookingStatus)booking.Status;
            if (currentStatus == BookingStatus.Completed)
                throw new InvalidOperationException("Khong the huy booking da hoan thanh.");

            if (currentStatus == BookingStatus.Cancelled)
                return _mapper.Map<BookingDto>(booking);

            var now = DateTime.UtcNow;

            // Huy cac lich sap toi de giai phong slot va chan tra tien cho tutor
            await CancelUpcomingSchedulesByBookingAsync(bookingId);

            decimal refunded = 0;

            if (booking.PaymentStatus == (int)PaymentStatus.Paid ||
                booking.PaymentStatus == (int)PaymentStatus.RefundPending)
            {
                var learnerWallet = await GetOrCreateWalletAsync(booking.LearnerEmail);
                var systemWallet = await GetOrCreateWalletAsync(SystemWalletEmail);

                var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTz);
                var upcomingCount = booking.Schedules?.Count(s =>
                    s.Status != (int)ScheduleStatus.Completed &&
                    s.Status != (int)ScheduleStatus.Cancelled &&
                    (s.TutorPayout == null || s.TutorPayout.Status != (byte)TutorPayoutStatus.OnHold) &&
                    IsFutureSchedule(s, nowLocal)) ?? 0;
                var totalSessions = booking.TotalSessions > 0 ? booking.TotalSessions : booking.Schedules?.Count ?? 0;
                var perSessionAmount = totalSessions > 0 ? booking.TotalAmount / totalSessions : 0;
                var refundableBase = decimal.Round(perSessionAmount * upcomingCount, 2, MidpointRounding.AwayFromZero);

                var refundable = refundableBase;
                refundable = Math.Min(refundable, Math.Min(learnerWallet.LockedBalance, systemWallet.LockedBalance));

                if (refundable > 0)
                {
                    var systemLockedBefore = systemWallet.LockedBalance;
                    learnerWallet.LockedBalance -= refundable;
                    var learnerBalanceBefore = learnerWallet.Balance;
                    learnerWallet.Balance += refundable;
                    learnerWallet.UpdatedAt = now;
                    _unitOfWork.Wallets.Update(learnerWallet);

                    systemWallet.LockedBalance -= refundable;
                    systemWallet.UpdatedAt = now;
                    _unitOfWork.Wallets.Update(systemWallet);

                    await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
                    {
                        WalletId = learnerWallet.Id,
                        Amount = refundable,
                        TransactionType = WalletTransactionType.Credit,
                        Reason = WalletTransactionReason.BookingRefund,
                        Status = TransactionStatus.Completed,
                        BalanceBefore = learnerBalanceBefore,
                        BalanceAfter = learnerWallet.Balance,
                        CreatedAt = now,
                        ReferenceCode = $"BOOKING_LEARNER_CANCEL_{booking.Id}",
                        BookingId = booking.Id
                    });

                    await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
                    {
                        WalletId = systemWallet.Id,
                        Amount = refundable,
                        TransactionType = WalletTransactionType.Debit,
                        Reason = WalletTransactionReason.BookingRefund,
                        Status = TransactionStatus.Completed,
                        BalanceBefore = systemLockedBefore,
                        BalanceAfter = systemWallet.LockedBalance,
                        CreatedAt = now,
                        ReferenceCode = $"BOOKING_LEARNER_CANCEL_{booking.Id}",
                        BookingId = booking.Id
                    });

                    await _unitOfWork.CompleteAsync();

                    booking.RefundedAmount += refundable;
                    booking.PaymentStatus = Math.Abs(refundable - refundableBase) < 0.01m
                        ? (int)PaymentStatus.Refunded
                        : (int)PaymentStatus.RefundPending;

                    refunded = refundable;
                }
            }

            booking.Status = (int)BookingStatus.Cancelled;
            booking.UpdatedAt = now;
            await _bookingRepository.UpdateAsync(booking);

            var tutorEmail = booking.TutorSubject?.Tutor?.UserEmail;

            await _notificationService.CreateNotificationAsync(
                booking.LearnerEmail,
                refunded > 0
                    ? $"Booking với gia sư {tutorEmail} đã bị hủy. {refunded:N0} VND đã được hoàn về ví của bạn."
                    : $"Booking với gia sư {tutorEmail} đã bị hủy.",
                "/bookings");

            if (!string.IsNullOrWhiteSpace(tutorEmail))
            {
                await _notificationService.CreateNotificationAsync(
                    tutorEmail,
                    $"Booking với học viên {booking.LearnerEmail} đã bị hủy bởi học viên.",
                    "/bookings");
            }

            return _mapper.Map<BookingDto>(booking);
        }

        /// <summary>
        /// Xem trước thông tin hủy booking: số buổi chưa học và số tiền dự kiến hoàn lại.
        /// </summary>
        public async Task<BookingCancelPreviewDto> GetCancelPreviewAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId)
                ?? throw new Exception("Booking không tồn tại");

            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTz);
            var upcomingCount = booking.Schedules?.Count(s =>
                s.Status != (int)ScheduleStatus.Completed &&
                s.Status != (int)ScheduleStatus.Cancelled &&
                (s.TutorPayout == null || s.TutorPayout.Status != (byte)TutorPayoutStatus.OnHold) &&
                IsFutureSchedule(s, nowLocal)) ?? 0;

            decimal paidToTutor = 0;
            if (booking.Schedules != null && booking.Schedules.Any())
            {
                paidToTutor = booking.Schedules
                    .Where(s => s.TutorPayout != null && s.TutorPayout.Status == (byte)TutorPayoutStatus.Paid)
                    .Sum(s => s.TutorPayout!.Amount);
            }

            var totalSessions = booking.TotalSessions > 0 ? booking.TotalSessions : booking.Schedules?.Count ?? 0;
            var perSessionAmount = totalSessions > 0 ? booking.TotalAmount / totalSessions : 0;
            var refundableBase = decimal.Round(perSessionAmount * upcomingCount, 2, MidpointRounding.AwayFromZero);
            var refundable = refundableBase;

            return new BookingCancelPreviewDto
            {
                BookingId = booking.Id,
                UpcomingSchedules = upcomingCount,
                RefundableAmount = refundable
            };
        }

        public async Task<BookingDto> RefundBookingAsync(int bookingId, decimal learnerPercentage)
        {
            if (bookingId <= 0)
                throw new ArgumentException("BookingId must be greater than 0.");
            if (learnerPercentage < 0 || learnerPercentage > 100)
                throw new ArgumentOutOfRangeException(nameof(learnerPercentage), "Learner percentage must be between 0 and 100.");

            var booking = await _bookingRepository.GetByIdAsync(bookingId)
                ?? throw new Exception("Booking không tồn tại");
            var tutorEmail = booking.TutorSubject?.Tutor?.UserEmail;

            if (booking.PaymentStatus != (int)PaymentStatus.Paid &&
                booking.PaymentStatus != (int)PaymentStatus.RefundPending)
                throw new InvalidOperationException("Chỉ có thể hoàn tiền cho booking đã thanh toán hoặc đang chờ hoàn.");

            var amountToProcess = booking.TotalAmount - booking.RefundedAmount;
            if (amountToProcess <= 0)
                throw new InvalidOperationException("Booking đã được xử lý hoàn tiền trước đó.");

            var now = DateTime.UtcNow;
            var learnerWallet = await GetOrCreateWalletAsync(booking.LearnerEmail);
            if (learnerWallet.LockedBalance < amountToProcess)
                throw new InvalidOperationException("Số dư bị khóa của học viên không đủ để hoàn tiền.");

            learnerWallet.LockedBalance -= amountToProcess;

            var systemWallet = await GetOrCreateWalletAsync(SystemWalletEmail);
            if (systemWallet.LockedBalance < amountToProcess)
                throw new InvalidOperationException("Số dư khóa của hệ thống không hợp lệ.");
            var systemLockedBefore = systemWallet.LockedBalance;
            var systemBalanceBefore = systemWallet.Balance;
            systemWallet.LockedBalance -= amountToProcess;

            decimal learnerAmount;
            decimal tutorAmount = 0;
            decimal platformFeePortion = 0;

            if (learnerPercentage >= 100m)
            {
                learnerAmount = amountToProcess;
            }
            else
            {
                platformFeePortion = Math.Min(booking.SystemFeeAmount, amountToProcess);
                var distributable = Math.Max(amountToProcess - platformFeePortion, 0);
                var learnerRatio = learnerPercentage / 100m;

                learnerAmount = decimal.Round(distributable * learnerRatio, 2, MidpointRounding.AwayFromZero);
                tutorAmount = distributable - learnerAmount;
                if (tutorAmount < 0)
                {
                    tutorAmount = 0;
                }
            }

            var learnerBalanceBefore = learnerWallet.Balance;
            if (learnerAmount > 0)
            {
                learnerWallet.Balance += learnerAmount;
            }
            learnerWallet.UpdatedAt = now;
            _unitOfWork.Wallets.Update(learnerWallet);

            if (learnerAmount > 0)
            {
                await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
                {
                    WalletId = learnerWallet.Id,
                    Amount = learnerAmount,
                    TransactionType = WalletTransactionType.Credit,
                    Reason = WalletTransactionReason.BookingRefund,
                    Status = TransactionStatus.Completed,
                    BalanceBefore = learnerBalanceBefore,
                    BalanceAfter = learnerWallet.Balance,
                    CreatedAt = now,
                    ReferenceCode = $"BOOKING_REFUND_{booking.Id}"
                });
            }

                if (platformFeePortion > 0)
                {
                    systemWallet.Balance += platformFeePortion;
                    systemWallet.UpdatedAt = now;

                    await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
                    {
                        WalletId = systemWallet.Id,
                        Amount = platformFeePortion,
                        TransactionType = WalletTransactionType.Credit,
                        Reason = WalletTransactionReason.PlatformFee,
                        Status = TransactionStatus.Completed,
                        BalanceBefore = systemBalanceBefore,
                        BalanceAfter = systemWallet.Balance,
                        CreatedAt = now,
                        ReferenceCode = $"BOOKING_PLATFORM_FEE_{booking.Id}"
                });
            }
            else
            {
                systemWallet.UpdatedAt = now;
            }

            await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
            {
                WalletId = systemWallet.Id,
                Amount = amountToProcess,
                TransactionType = WalletTransactionType.Debit,
                Reason = WalletTransactionReason.BookingRefund,
                Status = TransactionStatus.Completed,
                BalanceBefore = systemLockedBefore,
                BalanceAfter = systemWallet.LockedBalance,
                CreatedAt = now,
                ReferenceCode = $"BOOKING_REFUND_{booking.Id}"
            });
            _unitOfWork.Wallets.Update(systemWallet);

            if (tutorAmount > 0)
            {
                if (string.IsNullOrWhiteSpace(tutorEmail))
                    throw new InvalidOperationException("Không tìm thấy email gia sư để chi trả.");

                var tutorWallet = await GetOrCreateWalletAsync(tutorEmail);
                var tutorBalanceBefore = tutorWallet.Balance;
                tutorWallet.Balance += tutorAmount;
                tutorWallet.UpdatedAt = now;
                _unitOfWork.Wallets.Update(tutorWallet);

                await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
                {
                    WalletId = tutorWallet.Id,
                    Amount = tutorAmount,
                    TransactionType = WalletTransactionType.Credit,
                    Reason = WalletTransactionReason.BookingPayout,
                    Status = TransactionStatus.Completed,
                    BalanceBefore = tutorBalanceBefore,
                    BalanceAfter = tutorWallet.Balance,
                    CreatedAt = now,
                    ReferenceCode = $"BOOKING_PAYOUT_{booking.Id}"
                });
            }

            await _unitOfWork.CompleteAsync();

            booking.RefundedAmount += learnerAmount;
            booking.TutorReceiveAmount = tutorAmount;
            booking.PaymentStatus = booking.RefundedAmount >= booking.TotalAmount
                ? (int)PaymentStatus.Refunded
                : (int)PaymentStatus.RefundPending;
            booking.Status = (int)BookingStatus.Cancelled;
            booking.UpdatedAt = now;

            await _bookingRepository.UpdateAsync(booking);

            if (learnerAmount > 0)
            {
                await _notificationService.CreateNotificationAsync(
                    booking.LearnerEmail,
                    $"Booking với gia sư {tutorEmail} đã được hoàn {learnerAmount:N0} VND về ví của bạn.",
                    "/wallet/my-wallet");
            }

            if (tutorAmount > 0 && !string.IsNullOrWhiteSpace(tutorEmail))
            {
                await _notificationService.CreateNotificationAsync(
                    tutorEmail,
                    $"Bạn đã nhận {tutorAmount:N0} VND từ booking với học viên {booking.LearnerEmail} sau khi xử lý hoàn tiền.",
                    "/wallet/my-wallet");
            }

            return _mapper.Map<BookingDto>(booking);
        }

        /// <summary>
        /// Cập nhật Status của Booking. Nếu status là Cancelled thì hủy tất cả Schedule liên quan
        /// </summary>
        public async Task<BookingDto> UpdateStatusAsync(int id, BookingStatus status)
        {
            var entity = await _bookingRepository.GetByIdAsync(id)
                ?? throw new Exception("Booking không tồn tại");

            var currentStatus = (BookingStatus)entity.Status;
            var newStatus = status;

            // Validate: Không cho phép cập nhật khi đã ở trạng thái Completed hoặc Cancelled
            if (currentStatus == BookingStatus.Completed)
                throw new Exception("Không thể cập nhật Status khi đã ở trạng thái Completed");

            if (currentStatus == BookingStatus.Cancelled)
                throw new Exception("Không thể cập nhật Status khi đã ở trạng thái Cancelled");

            // Validate: Không cho phép nhảy bước từ Pending sang Completed
            if (currentStatus == BookingStatus.Pending && newStatus == BookingStatus.Completed)
                throw new Exception("Không thể cập nhật Status từ Pending sang Completed. Phải chuyển sang Confirmed trước.");

            // Validate: Trạng thái mới phải lớn hơn trạng thái hiện tại
            if (newStatus <= currentStatus)
                throw new Exception($"Không thể cập nhật Status từ {currentStatus} về {newStatus}. Trạng thái mới phải lớn hơn trạng thái hiện tại.");

            entity.Status = (int)status;
            entity.UpdatedAt = DateTime.UtcNow;
            await _bookingRepository.UpdateAsync(entity);

			// Kiểm tra nếu booking là học thử (TotalAmount == 0)
			var isTrialLesson = entity.TotalAmount == 0;
  
            // Nếu status là Cancelled thì hủy tất cả Schedule liên quan
            if (status == BookingStatus.Cancelled)
            {
                await CancelAllSchedulesByBookingAsync(id);

                var tutorSubject = await _tutorSubjectRepository.GetByIdFullAsync(entity.TutorSubjectId);

                // Nếu booking là học thử thì xóa LearnerTrialLesson
                if (isTrialLesson && tutorSubject != null)
                {
                    await _learnerTrialLessonService.DeleteTrialAsync(
                        entity.LearnerEmail,
                        tutorSubject.TutorId,
                        tutorSubject.SubjectId);
                }

                await _notificationService.CreateNotificationAsync(
                    entity.LearnerEmail,
                    $"Booking với gia sư {tutorSubject?.Tutor?.UserEmail} đã được hủy.",
                    "/bookings");

                // Gửi notification báo đơn hàng đã hủy cho tutor TRƯỚC
                if (tutorSubject?.Tutor?.UserEmail != null)
                {
                    await _notificationService.CreateNotificationAsync(
                        tutorSubject.Tutor.UserEmail,
                        $"Booking với học viên {entity.LearnerEmail} đã được hủy.",
                        "/bookings");
                }
                
                // Sau đó mới thực hiện hoàn tiền nếu chuyển từ Pending sang Cancelled và PaymentStatus là Paid
                // Bỏ qua hoàn tiền cho đơn học thử (TotalAmount = 0)
                if (currentStatus == BookingStatus.Pending 
                    && entity.PaymentStatus == (int)PaymentStatus.Paid
                    && entity.TotalAmount > 0)
                {
                    await RefundBookingAsync(id, 100);
                }
            }

            return _mapper.Map<BookingDto>(entity);
        }

		/// <summary>
		/// Tự động hủy các booking Pending nếu quá 3 ngày chưa xác nhận hoặc lịch học sắp diễn ra
		/// </summary>
		public async Task<int> AutoCancelUnconfirmedBookingsAsync()
		{
			var serverNow = DateTime.Now;
			var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
			var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

			var createdThreshold = serverNow.AddDays(-3);
			var scheduleThreshold = vietnamNow.AddHours(1);

			var pendingBookings = await _bookingRepository.GetPendingBookingsNeedingAutoCancelAsync(createdThreshold, scheduleThreshold);
			int cancelledCount = 0;

			foreach (var booking in pendingBookings)
			{
				if (booking.Status != (int)BookingStatus.Pending)
					continue;

				await UpdateStatusAsync(booking.Id, BookingStatus.Cancelled);
				cancelledCount++;
			}

			return cancelledCount;
		}

		/// <summary>
		/// Tự động hoàn thành các booking Confirmed khi tất cả schedule đều không còn Upcoming
		/// </summary>
		public async Task<int> AutoCompleteConfirmedBookingsAsync()
		{
			var confirmedBookings = await _bookingRepository.GetConfirmedBookingsAsync();
			int completedCount = 0;

			foreach (var booking in confirmedBookings)
			{
				// Lấy tất cả schedules của booking
				var schedules = await _scheduleRepository.GetAllByBookingIdOrderedAsync(booking.Id);
				var schedulesList = schedules.ToList();

				// Kiểm tra nếu booking không có schedule nào thì bỏ qua
				if (!schedulesList.Any())
					continue;

				// Kiểm tra xem tất cả schedules có status khác Upcoming không
				bool allSchedulesNotUpcoming = schedulesList.All(s => s.Status != (int)ScheduleStatus.Upcoming);

				if (allSchedulesNotUpcoming)
				{
					await UpdateStatusAsync(booking.Id, BookingStatus.Completed);
					completedCount++;
				}
			}

			return completedCount;
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

                // Cancel related completion and payout if they exist
                if (_unitOfWork.ScheduleCompletions != null)
                {
                    var completion = await _unitOfWork.ScheduleCompletions.GetByScheduleIdAsync(schedule.Id);
                    if (completion != null)
                    {
                        completion.Status = (byte)ScheduleCompletionStatus.Cancelled;
                        completion.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.ScheduleCompletions.Update(completion);
                    }
                }

                if (_unitOfWork.TutorPayouts != null)
                {
                    var payout = await _unitOfWork.TutorPayouts.GetByScheduleIdAsync(schedule.Id);
                    if (payout != null && payout.Status != (byte)TutorPayoutStatus.Paid)
                    {
                        payout.Status = (byte)TutorPayoutStatus.Cancelled;
                        payout.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.TutorPayouts.Update(payout);
                    }
                }
            }

            if (_unitOfWork.ScheduleCompletions != null || _unitOfWork.TutorPayouts != null)
            {
                await _unitOfWork.CompleteAsync();
            }
        }

        /// <summary>
        /// Hủy các Schedule chưa diễn ra (chưa Completed/Cancelled và chưa tới giờ bắt đầu) theo bookingId.
        /// </summary>
        private async Task CancelUpcomingSchedulesByBookingAsync(int bookingId)
        {
            var schedules = (await _scheduleRepository.GetAllByBookingIdOrderedAsync(bookingId)).ToList();
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTz);
            foreach (var schedule in schedules)
            {
                // Chỉ hủy các buổi học chưa diễn ra và chưa hoàn thành
                if (schedule.Status == (int)ScheduleStatus.Completed ||
                    schedule.Status == (int)ScheduleStatus.Cancelled ||
                    !IsFutureSchedule(schedule, nowLocal))
                {
                    continue;
                }

                // Delete meeting session first (includes Google event deletion)
                var meetingSession = await _meetingSessionRepository.GetByScheduleIdAsync(schedule.Id);
                if (meetingSession != null)
                {
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
                    await _meetingSessionRepository.DeleteAsync(meetingSession.Id);
                }

                schedule.Status = (int)ScheduleStatus.Cancelled;
                schedule.UpdatedAt = DateTime.UtcNow;
                await _scheduleRepository.UpdateAsync(schedule);

                var availability = await _tutorAvailabilityRepository.GetByIdFullAsync(schedule.AvailabilitiId);
                if (availability != null)
                {
                    availability.Status = (int)TutorAvailabilityStatus.Available;
                    await _tutorAvailabilityRepository.UpdateAsync(availability);
                }

                if (_unitOfWork.ScheduleCompletions != null)
                {
                    var completion = await _unitOfWork.ScheduleCompletions.GetByScheduleIdAsync(schedule.Id);
                    if (completion != null)
                    {
                        completion.Status = (byte)ScheduleCompletionStatus.Cancelled;
                        completion.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.ScheduleCompletions.Update(completion);
                    }
                }

                if (_unitOfWork.TutorPayouts != null)
                {
                    var payout = await _unitOfWork.TutorPayouts.GetByScheduleIdAsync(schedule.Id);
                    if (payout != null && payout.Status != (byte)TutorPayoutStatus.Paid)
                    {
                        payout.Status = (byte)TutorPayoutStatus.Cancelled;
                        payout.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.TutorPayouts.Update(payout);
                    }
                }
            }

            if (_unitOfWork.ScheduleCompletions != null || _unitOfWork.TutorPayouts != null)
            {
                await _unitOfWork.CompleteAsync();
            }
        }

        private bool IsFutureSchedule(Schedule schedule, DateTime nowLocal)
        {
            if (schedule?.Availabiliti?.Slot == null)
            {
                return true;
            }

            var lessonStart = schedule.Availabiliti.StartDate.Date.Add(schedule.Availabiliti.Slot.StartTime.ToTimeSpan());
            return lessonStart > nowLocal;
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
    }
}
