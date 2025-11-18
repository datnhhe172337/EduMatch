using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class BookingRefundRequestRepository : IBookingRefundRequestRepository
    {
        private readonly EduMatchContext _context;

        public BookingRefundRequestRepository(EduMatchContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả BookingRefundRequest với lọc theo Status
        /// </summary>
        public async Task<IEnumerable<BookingRefundRequest>> GetAllAsync(int? status = null)
        {
            var query = _context.BookingRefundRequests
                .Include(brr => brr.Booking)
                .Include(brr => brr.RefundPolicy)
                .Include(brr => brr.LearnerEmailNavigation)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(brr => brr.Status == status.Value);
            }

            return await query
                .OrderByDescending(brr => brr.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy BookingRefundRequest theo ID
        /// </summary>
        public async Task<BookingRefundRequest?> GetByIdAsync(int id)
        {
            return await _context.BookingRefundRequests
                .Include(brr => brr.Booking)
                .Include(brr => brr.RefundPolicy)
                .Include(brr => brr.LearnerEmailNavigation)
                .FirstOrDefaultAsync(brr => brr.Id == id);
        }

        /// <summary>
        /// Lấy tất cả BookingRefundRequest theo LearnerEmail với lọc theo Status
        /// </summary>
        public async Task<IEnumerable<BookingRefundRequest>> GetAllByEmailAsync(string learnerEmail, int? status = null)
        {
            if (string.IsNullOrWhiteSpace(learnerEmail))
            {
                return new List<BookingRefundRequest>();
            }

            var query = _context.BookingRefundRequests
                .Include(brr => brr.Booking)
                .Include(brr => brr.RefundPolicy)
                .Include(brr => brr.LearnerEmailNavigation)
                .Where(brr => brr.LearnerEmail == learnerEmail)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(brr => brr.Status == status.Value);
            }

            return await query
                .OrderByDescending(brr => brr.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy tất cả BookingRefundRequest theo BookingId
        /// </summary>
        public async Task<IEnumerable<BookingRefundRequest>> GetAllByBookingIdAsync(int bookingId)
        {
            return await _context.BookingRefundRequests
                .Include(brr => brr.Booking)
                .Include(brr => brr.RefundPolicy)
                .Include(brr => brr.LearnerEmailNavigation)
                .Where(brr => brr.BookingId == bookingId)
                .OrderByDescending(brr => brr.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Tạo BookingRefundRequest mới
        /// </summary>
        public async Task CreateAsync(BookingRefundRequest entity)
        {
            await _context.BookingRefundRequests.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cập nhật BookingRefundRequest
        /// </summary>
        public async Task UpdateAsync(BookingRefundRequest entity)
        {
            _context.BookingRefundRequests.Update(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Xóa BookingRefundRequest theo ID
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var entity = await _context.BookingRefundRequests.FirstOrDefaultAsync(brr => brr.Id == id);
            if (entity != null)
            {
                _context.BookingRefundRequests.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}

