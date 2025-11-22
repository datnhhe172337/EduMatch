using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class RefundRequestEvidenceRepository : IRefundRequestEvidenceRepository
    {
        private readonly EduMatchContext _context;

        public RefundRequestEvidenceRepository(EduMatchContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy RefundRequestEvidence theo ID
        /// </summary>
        public async Task<RefundRequestEvidence?> GetByIdAsync(int id)
        {
            return await _context.RefundRequestEvidences
                .Include(rre => rre.BookingRefundRequest)
                .FirstOrDefaultAsync(rre => rre.Id == id);
        }

        /// <summary>
        /// Lấy tất cả RefundRequestEvidence theo BookingRefundRequestId
        /// </summary>
        public async Task<IEnumerable<RefundRequestEvidence>> GetByBookingRefundRequestIdAsync(int bookingRefundRequestId)
        {
            return await _context.RefundRequestEvidences
                .Include(rre => rre.BookingRefundRequest)
                .Where(rre => rre.BookingRefundRequestId == bookingRefundRequestId)
                .OrderByDescending(rre => rre.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Tạo RefundRequestEvidence mới
        /// </summary>
        public async Task CreateAsync(RefundRequestEvidence entity)
        {
            await _context.RefundRequestEvidences.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
    }
}

