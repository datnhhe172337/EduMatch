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
using Microsoft.EntityFrameworkCore;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ITutorSubjectRepository _tutorSubjectRepository;
        private readonly ISystemFeeRepository _systemFeeRepository;
        private readonly IMapper _mapper;
        private readonly EduMatchContext _context;

        public BookingService(
            IBookingRepository bookingRepository,
            ITutorSubjectRepository tutorSubjectRepository,
            ISystemFeeRepository systemFeeRepository,
            IMapper mapper,
            EduMatchContext context)
        {
            _bookingRepository = bookingRepository;
            _tutorSubjectRepository = tutorSubjectRepository;
            _systemFeeRepository = systemFeeRepository;
            _mapper = mapper;
            _context = context;
        }

        public async Task<List<BookingDto>> GetAllByLearnerEmailAsync(string email, int? status, int? tutorSubjectId, int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            var entities = await _bookingRepository.GetAllByLearnerEmailAsync(email, status, tutorSubjectId, page, pageSize);
            return _mapper.Map<List<BookingDto>>(entities);
        }

        public Task<int> CountByLearnerEmailAsync(string email, int? status, int? tutorSubjectId)
        {
            return _bookingRepository.CountByLearnerEmailAsync(email, status, tutorSubjectId);
        }

        public async Task<BookingDto?> GetByIdAsync(int id)
        {
            var entity = await _bookingRepository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<BookingDto>(entity);
        }

        public async Task<BookingDto> CreateAsync(BookingCreateRequest request)
        {
            // Validate TutorSubject exists and get HourlyRate
            var tutorSubject = await _tutorSubjectRepository.GetByIdFullAsync(request.TutorSubjectId)
                ?? throw new Exception("TutorSubject không tồn tại");

            if (!tutorSubject.HourlyRate.HasValue || tutorSubject.HourlyRate.Value <= 0)
                throw new Exception("TutorSubject không có giá hợp lệ");

            var unitPrice = tutorSubject.HourlyRate.Value;
            var totalSessions = request.TotalSessions ?? 1;

            // Get active SystemFee
            var now = DateTime.UtcNow;
            var activeSystemFee = await _context.SystemFees
                .Where(sf => sf.IsActive == true 
                    && sf.EffectiveFrom <= now 
                    && (sf.EffectiveTo == null || sf.EffectiveTo >= now))
                .OrderByDescending(sf => sf.EffectiveFrom)
                .FirstOrDefaultAsync()
                ?? throw new Exception("Không tìm thấy SystemFee đang hoạt động");

            // Calculate base amount (tổng đơn hàng)
            var baseAmount = unitPrice * totalSessions;

            // Calculate SystemFeeAmount
            // Phí % = tính trên tổng đơn hàng (baseAmount)
            // Phí cố định = tính một lần trên tổng đơn hàng
            // TotalAmount giữ nguyên giá gốc (baseAmount), không cộng phí vào
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

            // TotalAmount giữ nguyên giá gốc, không cộng phí
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

        public async Task<BookingDto> UpdateAsync(BookingUpdateRequest request)
        {
            var entity = await _bookingRepository.GetByIdAsync(request.Id)
                ?? throw new Exception("Booking không tồn tại");

            if (!string.IsNullOrWhiteSpace(request.LearnerEmail))
                entity.LearnerEmail = request.LearnerEmail;

            if (request.TutorSubjectId.HasValue)
            {
                var tutorSubject = await _tutorSubjectRepository.GetByIdFullAsync(request.TutorSubjectId.Value)
                    ?? throw new Exception("TutorSubject không tồn tại");
                entity.TutorSubjectId = request.TutorSubjectId.Value;
            }

            if (request.TotalSessions.HasValue && request.TotalSessions.Value > 0)
            {
                entity.TotalSessions = request.TotalSessions.Value;
                // Recalculate TotalAmount if TotalSessions changes
                var baseAmount = entity.UnitPrice * entity.TotalSessions;
                entity.TotalAmount = baseAmount + entity.SystemFeeAmount;
            }

            if (request.PaymentStatus.HasValue)
                entity.PaymentStatus = (int)request.PaymentStatus.Value;

            if (request.RefundedAmount.HasValue)
                entity.RefundedAmount = request.RefundedAmount.Value;

            if (request.Status.HasValue)
                entity.Status = (int)request.Status.Value;

            entity.UpdatedAt = DateTime.UtcNow;

            await _bookingRepository.UpdateAsync(entity);
            return _mapper.Map<BookingDto>(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _bookingRepository.DeleteAsync(id);
        }
    }
}

