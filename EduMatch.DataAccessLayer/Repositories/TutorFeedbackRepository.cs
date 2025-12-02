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
    public class TutorFeedbackRepository : ITutorFeedbackRepository
    {
        private readonly EduMatchContext _context;

        public TutorFeedbackRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsByBookingAsync(int bookingId)
        {
            return await _context.Bookings.AnyAsync(b => b.Id == bookingId);
        }

        public async Task<TutorFeedback?> GetFeedbackByBookingAsync(int bookingId, string learnerEmail, int tutorId)
        {
            return await _context.TutorFeedbacks
                .Include(f => f.TutorFeedbackDetails)
                    .ThenInclude(d => d.Criterion)
                .FirstOrDefaultAsync(f => f.BookingId == bookingId
                                      && f.LearnerEmail == learnerEmail
                                      && f.TutorId == tutorId);
        }

        public async Task<TutorFeedback> GetByIdIncludeDetailsAsync(int id)
        {
            return await _context.TutorFeedbacks
                .Include(f => f.TutorFeedbackDetails)
                    .ThenInclude(d => d.Criterion)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task AddFeedbackAsync(TutorFeedback feedback)
        {
            await _context.TutorFeedbacks.AddAsync(feedback);
        }

        public async Task AddFeedbackDetailRangeAsync(List<TutorFeedbackDetail> details)
        {
            await _context.TutorFeedbackDetails.AddRangeAsync(details);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

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

        public async Task<List<FeedbackCriterion>> GetAllCriteriaAsync()
        {
            return await _context.FeedbackCriteria.ToListAsync();
        }

        public async Task<List<TutorFeedback>> GetFeedbackByLearnerEmailAsync(string learnerEmail)
        {
            return await _context.TutorFeedbacks
            .Include(f => f.TutorFeedbackDetails)
                .ThenInclude(d => d.Criterion)
            .Where(f => f.LearnerEmail == learnerEmail)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
        }

        public async Task<List<TutorFeedback>> GetFeedbackByTutorIdAsync(int tutorId)
        {
            return await _context.TutorFeedbacks
            .Include(f => f.TutorFeedbackDetails)
                .ThenInclude(d => d.Criterion)
            .Where(f => f.TutorId == tutorId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
        }

        public async Task<List<TutorFeedback>> GetAllFeedbacksAsync()
        {
            return await _context.TutorFeedbacks
            .Include(f => f.TutorFeedbackDetails)
                .ThenInclude(d => d.Criterion)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
        }

        public async Task UpdateFeedbackAsync(TutorFeedback feedback)
        {
            _context.TutorFeedbacks.Update(feedback);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFeedbackDetailsAsync(IEnumerable<TutorFeedbackDetail> details)
        {
            _context.TutorFeedbackDetails.RemoveRange(details);
            await _context.SaveChangesAsync();
        }

        public async Task<double> GetTutorAvgRatingAsync(int tutorId)
        {
            return await _context.TutorFeedbacks
           .Where(f => f.TutorId == tutorId)
           .AverageAsync(f => (double?)f.OverallRating)
           ?? 0;
        }

        public async Task<int> GetTutorFeedbackCountAsync(int tutorId)
        {
            return await _context.TutorFeedbacks
            .Where(f => f.TutorId == tutorId)
            .CountAsync();
        }
    }
}
