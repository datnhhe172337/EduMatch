using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface ITutorFeedbackRepository
    {
        Task<bool> ExistsByBookingAsync(int bookingId);
        Task<TutorFeedback?> GetFeedbackByBookingAsync(int bookingId, string learnerEmail, int tutorId);
        Task<TutorFeedback> GetByIdIncludeDetailsAsync(int id);
        Task AddFeedbackAsync(TutorFeedback feedback);
        Task AddFeedbackDetailRangeAsync(List<TutorFeedbackDetail> details);
        Task SaveAsync();
        Task<int> CountCompletedSessionsAsync(int bookingId);
        Task<int> GetTotalSessionsAsync(int bookingId);
        Task<List<FeedbackCriterion>> GetAllCriteriaAsync();
        Task<List<TutorFeedback>> GetFeedbackByLearnerEmailAsync(string learnerEmail);
        Task<List<TutorFeedback>> GetFeedbackByTutorIdAsync(int tutorId);
        Task<List<TutorFeedback>> GetAllFeedbacksAsync();
        Task UpdateFeedbackAsync(TutorFeedback feedback);
        Task RemoveFeedbackDetailsAsync(IEnumerable<TutorFeedbackDetail> details);
        Task<double> GetTutorAvgRatingAsync(int tutorId);
        Task<int> GetTutorFeedbackCountAsync(int tutorId);
    }
}
