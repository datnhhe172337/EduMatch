using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.BookingRefundRequest;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class BookingRefundRequestService : IBookingRefundRequestService
    {
        private readonly IBookingRefundRequestRepository _bookingRefundRequestRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IRefundPolicyRepository _refundPolicyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly CurrentUserService _currentUserService;

        public BookingRefundRequestService(
            IBookingRefundRequestRepository bookingRefundRequestRepository,
            IBookingRepository bookingRepository,
            IRefundPolicyRepository refundPolicyRepository,
            IUserRepository userRepository,
            IMapper mapper,
            CurrentUserService currentUserService)
        {
            _bookingRefundRequestRepository = bookingRefundRequestRepository;
            _bookingRepository = bookingRepository;
            _refundPolicyRepository = refundPolicyRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _currentUserService = currentUserService;
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
                var approvedAmount = booking.TotalAmount * (refundPolicy.RefundPercentage / 100);

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

                await _bookingRefundRequestRepository.CreateAsync(entity);
                return _mapper.Map<BookingRefundRequestDto>(entity);
            }
            catch (Exception ex)
            {
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

                entity.Status = (int)status;
                entity.ProcessedAt = DateTime.UtcNow;
                entity.ProcessedBy = currentUserEmail;

                await _bookingRefundRequestRepository.UpdateAsync(entity);
                return _mapper.Map<BookingRefundRequestDto>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật trạng thái yêu cầu hoàn tiền: {ex.Message}", ex);
            }
        }
    }
}

