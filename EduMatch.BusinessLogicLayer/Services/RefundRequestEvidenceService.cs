using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.RefundRequestEvidence;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class RefundRequestEvidenceService : IRefundRequestEvidenceService
    {
        private readonly IRefundRequestEvidenceRepository _refundRequestEvidenceRepository;
        private readonly IBookingRefundRequestRepository _bookingRefundRequestRepository;
        private readonly IMapper _mapper;

        public RefundRequestEvidenceService(
            IRefundRequestEvidenceRepository refundRequestEvidenceRepository,
            IBookingRefundRequestRepository bookingRefundRequestRepository,
            IMapper mapper)
        {
            _refundRequestEvidenceRepository = refundRequestEvidenceRepository;
            _bookingRefundRequestRepository = bookingRefundRequestRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy RefundRequestEvidence theo ID
        /// </summary>
        public async Task<RefundRequestEvidenceDto?> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new Exception("Id phải lớn hơn 0");

                var entity = await _refundRequestEvidenceRepository.GetByIdAsync(id);
                return entity == null ? null : _mapper.Map<RefundRequestEvidenceDto>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy bằng chứng hoàn tiền: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lấy tất cả RefundRequestEvidence theo BookingRefundRequestId
        /// </summary>
        public async Task<List<RefundRequestEvidenceDto>> GetByBookingRefundRequestIdAsync(int bookingRefundRequestId)
        {
            try
            {
                if (bookingRefundRequestId <= 0)
                    throw new Exception("BookingRefundRequestId phải lớn hơn 0");

                var entities = await _refundRequestEvidenceRepository.GetByBookingRefundRequestIdAsync(bookingRefundRequestId);
                return _mapper.Map<List<RefundRequestEvidenceDto>>(entities);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách bằng chứng hoàn tiền: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Tạo RefundRequestEvidence mới
        /// </summary>
        public async Task<RefundRequestEvidenceDto> CreateAsync(RefundRequestEvidenceCreateRequest request)
        {
            try
            {
                if (request == null)
                    throw new Exception("Yêu cầu không được để trống");

                if (request.BookingRefundRequestId <= 0)
                    throw new Exception("BookingRefundRequestId phải lớn hơn 0");

                if (string.IsNullOrWhiteSpace(request.FileUrl))
                    throw new Exception("FileUrl không được để trống");

                // Validate BookingRefundRequest exists
                var bookingRefundRequest = await _bookingRefundRequestRepository.GetByIdAsync(request.BookingRefundRequestId);
                if (bookingRefundRequest == null)
                    throw new Exception("BookingRefundRequest không tồn tại");

                var entity = _mapper.Map<RefundRequestEvidence>(request);
                await _refundRequestEvidenceRepository.CreateAsync(entity);

                return _mapper.Map<RefundRequestEvidenceDto>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo bằng chứng hoàn tiền: {ex.Message}", ex);
            }
        }
    }
}

