using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.RefundRequestEvidence;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IRefundRequestEvidenceService
    {
        /// <summary>
        /// Lấy RefundRequestEvidence theo ID
        /// </summary>
        Task<RefundRequestEvidenceDto?> GetByIdAsync(int id);

        /// <summary>
        /// Lấy tất cả RefundRequestEvidence theo BookingRefundRequestId
        /// </summary>
        Task<List<RefundRequestEvidenceDto>> GetByBookingRefundRequestIdAsync(int bookingRefundRequestId);

        /// <summary>
        /// Tạo RefundRequestEvidence mới
        /// </summary>
        Task<RefundRequestEvidenceDto> CreateAsync(RefundRequestEvidenceCreateRequest request);
    }
}

