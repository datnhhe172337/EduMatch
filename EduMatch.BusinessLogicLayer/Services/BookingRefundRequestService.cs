using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.BookingRefundRequest;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class BookingRefundRequestService : IBookingRefundRequestService
    {
        private readonly IBookingRefundRequestRepository _bookingRefundRequestRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IRefundPolicyRepository _refundPolicyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRefundRequestEvidenceRepository _refundRequestEvidenceRepository;
        private readonly IMapper _mapper;
        private readonly CurrentUserService _currentUserService;
        private readonly EduMatchContext _context;
        private readonly IBookingService _bookingService;
        private readonly INotificationService _notificationService;

        public BookingRefundRequestService(
            IBookingRefundRequestRepository bookingRefundRequestRepository,
            IBookingRepository bookingRepository,
            IRefundPolicyRepository refundPolicyRepository,
            IUserRepository userRepository,
            IRefundRequestEvidenceRepository refundRequestEvidenceRepository,
            IMapper mapper,
            CurrentUserService currentUserService,
            EduMatchContext context,
            IBookingService bookingService,
            INotificationService notificationService)
        {
            _bookingRefundRequestRepository = bookingRefundRequestRepository;
            _bookingRepository = bookingRepository;
            _refundPolicyRepository = refundPolicyRepository;
            _userRepository = userRepository;
            _refundRequestEvidenceRepository = refundRequestEvidenceRepository;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _context = context;
            _bookingService = bookingService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Lấy tất cả BookingRefundRequest với lọc theo Status
        /// </summary>
        public async Task<List<BookingRefundRequestDto>> GetAllAsync(BookingRefundRequestStatus? status = null)
        {
            try
            {
                int? statusInt = status.HasValue ? (int?)status.Value : null;
                var entities = await _bookingRefundRequestRepository.GetAllAsync(statusInt);
                return _mapper.Map<List<BookingRefundRequestDto>>(entities);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách yêu cầu hoàn tiền: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lấy BookingRefundRequest theo ID
        /// </summary>
        public async Task<BookingRefundRequestDto?> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new Exception("Id phải lớn hơn 0");

                var entity = await _bookingRefundRequestRepository.GetByIdAsync(id);
                return entity == null ? null : _mapper.Map<BookingRefundRequestDto>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy yêu cầu hoàn tiền: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lấy tất cả BookingRefundRequest theo LearnerEmail với lọc theo Status
        /// </summary>
        public async Task<List<BookingRefundRequestDto>> GetAllByEmailAsync(string learnerEmail, BookingRefundRequestStatus? status = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(learnerEmail))
                    throw new Exception("Email không được để trống");

                int? statusInt = status.HasValue ? (int?)status.Value : null;
                var entities = await _bookingRefundRequestRepository.GetAllByEmailAsync(learnerEmail, statusInt);
                return _mapper.Map<List<BookingRefundRequestDto>>(entities);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách yêu cầu hoàn tiền theo email: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Tạo BookingRefundRequest mới
        /// </summary>
        public async Task<BookingRefundRequestDto> CreateAsync(BookingRefundRequestCreateRequest request)
        {
            // Sử dụng transaction cho toàn bộ hàm
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (request == null)
                    throw new Exception("Yêu cầu không được để trống");

                if (request.BookingId <= 0)
                    throw new Exception("BookingId phải lớn hơn 0");

                if (string.IsNullOrWhiteSpace(request.LearnerEmail))
                    throw new Exception("LearnerEmail không được để trống");

                if (request.RefundPolicyId <= 0)
                    throw new Exception("RefundPolicyId phải lớn hơn 0");

                // Validate Booking exists
                var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
                if (booking == null)
                    throw new Exception("Booking không tồn tại");

                // Validate Booking status phải là Confirmed
                if (booking.Status != (int)BookingStatus.Confirmed)
                    throw new Exception("Chỉ có thể tạo yêu cầu hoàn tiền cho booking đã được xác nhận");

                // Validate Booking payment status phải là Paid
                if (booking.PaymentStatus != (int)PaymentStatus.Paid)
                    throw new Exception("Chỉ có thể tạo yêu cầu hoàn tiền cho booking đã thanh toán");

                // Check xem đã có BookingRefundRequest nào cho BookingId này chưa
                var existingRequests = await _bookingRefundRequestRepository.GetAllByBookingIdAsync(request.BookingId);
                if (existingRequests != null && existingRequests.Any())
                {
                    // Kiểm tra xem có request nào với status Pending hoặc Approved không
                    var hasPendingOrApproved = existingRequests.Any(r => 
                        r.Status == (int)BookingRefundRequestStatus.Pending ||
                        r.Status == (int)BookingRefundRequestStatus.Approved);
                    
                    if (hasPendingOrApproved)
                    {
                        throw new Exception("Đã tồn tại yêu cầu hoàn tiền đang chờ duyệt hoặc đã được duyệt cho booking này");
                    }
                    // Chỉ cho phép tạo mới nếu tất cả các request đều là Rejected
                }

                // Validate LearnerEmail exists
                var learner = await _userRepository.GetUserByEmailAsync(request.LearnerEmail);
                if (learner == null)
                    throw new Exception("LearnerEmail không tồn tại trong hệ thống");

                // Validate RefundPolicy exists
                var refundPolicy = await _refundPolicyRepository.GetByIdAsync(request.RefundPolicyId);
                if (refundPolicy == null)
                    throw new Exception("Chính sách hoàn tiền không tồn tại");

                if (!refundPolicy.IsActive)
                    throw new Exception("Chính sách hoàn tiền không đang hoạt động");

                // Tính ApprovedAmount dựa trên RefundPolicy
                var approvedAmount = (booking.TotalAmount - booking.SystemFeeAmount) * (refundPolicy.RefundPercentage / 100);

                var now = DateTime.UtcNow;
                var entity = new BookingRefundRequest
                {
                    BookingId = request.BookingId,
                    LearnerEmail = request.LearnerEmail,
                    RefundPolicyId = request.RefundPolicyId,
                    Reason = request.Reason,
                    Status = (int)BookingRefundRequestStatus.Pending,
                    ApprovedAmount = approvedAmount,
                    AdminNote = null,
                    CreatedAt = now,
                    ProcessedAt = null,
                    ProcessedBy = null
                };

                // Tạo BookingRefundRequest bằng repository
                await _bookingRefundRequestRepository.CreateAsync(entity);

                // Nếu có FileUrls, tạo các RefundRequestEvidence bằng repository
                if (request.FileUrls != null && request.FileUrls.Any())
                {
                    foreach (var fileUrl in request.FileUrls)
                    {
                        var evidence = new RefundRequestEvidence
                        {
                            BookingRefundRequestId = entity.Id,
                            FileUrl = fileUrl,
                            CreatedAt = now
                        };
                        await _refundRequestEvidenceRepository.CreateAsync(evidence);
                    }
                }

                // Commit transaction
                await dbTransaction.CommitAsync();

                return _mapper.Map<BookingRefundRequestDto>(entity);
            }
            catch (Exception ex)
            {
                // Rollback nếu có lỗi
                await dbTransaction.RollbackAsync();
                throw new Exception($"Lỗi khi tạo yêu cầu hoàn tiền: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cập nhật BookingRefundRequest
        /// </summary>
        public async Task<BookingRefundRequestDto> UpdateAsync(BookingRefundRequestUpdateRequest request)
        {
            try
            {
                if (request == null)
                    throw new Exception("Yêu cầu không được để trống");

                if (request.Id <= 0)
                    throw new Exception("Id phải lớn hơn 0");

                var entity = await _bookingRefundRequestRepository.GetByIdAsync(request.Id);
                if (entity == null)
                    throw new Exception("Yêu cầu hoàn tiền không tồn tại");

                var currentUserEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(currentUserEmail))
                    throw new Exception("Không thể xác định người dùng hiện tại");

                // Cập nhật các trường nếu có giá trị
                bool hasChanges = false;

                if (request.Reason != null)
                {
                    entity.Reason = request.Reason;
                    hasChanges = true;
                }

                if (request.AdminNote != null)
                {
                    entity.AdminNote = request.AdminNote;
                    hasChanges = true;
                }

                // Nếu có thay đổi thì cập nhật ProcessedBy và ProcessedAt
                if (hasChanges)
                {
                    entity.ProcessedAt = DateTime.UtcNow;
                    entity.ProcessedBy = currentUserEmail;
                }

                await _bookingRefundRequestRepository.UpdateAsync(entity);
                return _mapper.Map<BookingRefundRequestDto>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật yêu cầu hoàn tiền: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cập nhật Status của BookingRefundRequest
        /// </summary>
        public async Task<BookingRefundRequestDto> UpdateStatusAsync(int id, BookingRefundRequestStatus status)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (id <= 0)
                    throw new Exception("Id phải lớn hơn 0");

                var entity = await _bookingRefundRequestRepository.GetByIdAsync(id);
                if (entity == null)
                    throw new Exception("Yêu cầu hoàn tiền không tồn tại");

                var currentUserEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(currentUserEmail))
                    throw new Exception("Không thể xác định người dùng hiện tại");

                var currentStatus = (BookingRefundRequestStatus)entity.Status;
                var newStatus = status;

                // Validation: Chỉ cho phép cập nhật status theo thứ tự tăng dần
                // Không cho phép từ Approved về Rejected
                if (currentStatus == BookingRefundRequestStatus.Approved && newStatus == BookingRefundRequestStatus.Rejected)
                {
                    throw new Exception("Không thể chuyển trạng thái từ Đã duyệt sang Bị từ chối");
                }

                // Không cho phép thay đổi nếu đã là Rejected
                if (currentStatus == BookingRefundRequestStatus.Rejected)
                {
                    throw new Exception("Không thể thay đổi trạng thái của yêu cầu đã bị từ chối");
                }

                // Không cho phép chuyển ngược lại (ví dụ từ Approved về Pending)
                if ((int)newStatus < (int)currentStatus)
                {
                    throw new Exception("Không thể chuyển trạng thái ngược lại");
                }

                entity.Status = (int)status;
                entity.ProcessedAt = DateTime.UtcNow;
                entity.ProcessedBy = currentUserEmail;

                // Nếu status được update sang Approved, cần update Booking
                if (newStatus == BookingRefundRequestStatus.Approved)
                {
                    // Kiểm tra ApprovedAmount có giá trị
                    if (!entity.ApprovedAmount.HasValue || entity.ApprovedAmount.Value <= 0)
                        throw new Exception("Không thể duyệt yêu cầu hoàn tiền khi ApprovedAmount không có giá trị hoặc bằng 0");

                    // Lấy Booking
                    var booking = await _bookingRepository.GetByIdAsync(entity.BookingId);
                    if (booking == null)
                        throw new Exception("Booking không tồn tại");

                    // Kiểm tra PaymentStatus phải là Paid (RefundBookingAsync yêu cầu)
                    if (booking.PaymentStatus != (int)PaymentStatus.Paid)
                        throw new Exception("Chỉ có thể hoàn tiền cho booking đã thanh toán");

                    // Lấy RefundPolicy để lấy phần trăm hoàn tiền
                    var refundPolicy = await _refundPolicyRepository.GetByIdAsync(entity.RefundPolicyId);
                    if (refundPolicy == null)
                        throw new Exception("Chính sách hoàn tiền không tồn tại");

                    // Sử dụng RefundPercentage từ RefundPolicy
                    var learnerPercentage = refundPolicy.RefundPercentage;

                    // Gọi hàm refund từ BookingService
                    await _bookingService.RefundBookingAsync(booking.Id, learnerPercentage);

                    // Gửi thông báo cho học viên
                    await _notificationService.CreateNotificationAsync(
                        entity.LearnerEmail,
                        $"Đơn hoàn tiền của bạn đã được duyệt. Số tiền {entity.ApprovedAmount.Value:N0} VND sẽ được hoàn về ví của bạn.",
                        "/wallet/my-wallet");
                }

                // Nếu status được update sang Rejected, gửi thông báo cho học viên
                if (newStatus == BookingRefundRequestStatus.Rejected)
                {
                    var rejectMessage = !string.IsNullOrWhiteSpace(entity.AdminNote)
                        ? $"Yêu cầu hoàn tiền của bạn đã bị từ chối. Lý do: {entity.AdminNote}."
                        : "Yêu cầu hoàn tiền của bạn đã bị từ chối.";

                    await _notificationService.CreateNotificationAsync(
                        entity.LearnerEmail,
                        rejectMessage,
                        "/booking/refund-requests");
                }

                await _bookingRefundRequestRepository.UpdateAsync(entity);
                
                // Commit transaction
                await dbTransaction.CommitAsync();
                
                return _mapper.Map<BookingRefundRequestDto>(entity);
            }
            catch (Exception ex)
            {
                // Rollback transaction nếu có lỗi
                await dbTransaction.RollbackAsync();
                throw new Exception($"Lỗi khi cập nhật trạng thái yêu cầu hoàn tiền: {ex.Message}", ex);
            }
        }
    }
}

