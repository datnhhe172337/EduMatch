using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IRefundRequestEvidenceRepository
    {
        /// <summary>
        /// Lấy RefundRequestEvidence theo ID
        /// </summary>
        Task<RefundRequestEvidence?> GetByIdAsync(int id);

        /// <summary>
        /// Lấy tất cả RefundRequestEvidence theo BookingRefundRequestId
        /// </summary>
        Task<IEnumerable<RefundRequestEvidence>> GetByBookingRefundRequestIdAsync(int bookingRefundRequestId);

        /// <summary>
        /// Tạo RefundRequestEvidence mới
        /// </summary>
        Task CreateAsync(RefundRequestEvidence entity);
    }
}

