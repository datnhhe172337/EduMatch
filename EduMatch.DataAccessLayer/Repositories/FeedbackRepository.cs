using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly EduMatchContext _context;

        public FeedbackRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task AddFeedbackAsync(TutorFeedback feedback)
        {
            await _context.TutorFeedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();
        }

        public async Task AddFeedbackDetailRangeAsync(IEnumerable<TutorFeedbackDetail> details)
        {
            await _context.TutorFeedbackDetails.AddRangeAsync(details);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByBookingAsync(int bookingId, string learnerEmail)
        {
            return await _context.TutorFeedbacks
                .AnyAsync(f => f.BookingId == bookingId && f.LearnerEmail == learnerEmail);
        }

        // Giả sử bạn có bảng Schedules
        public async Task<int> CountCompletedSessionsAsync(int bookingId)
        {
            return await _context.Schedules
                .CountAsync(s => s.BookingId == bookingId && s.Status == 2);
        }

        public async Task<int> GetTotalSessionsAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            return booking?.TotalSessions ?? 0;
        }
    }

}
