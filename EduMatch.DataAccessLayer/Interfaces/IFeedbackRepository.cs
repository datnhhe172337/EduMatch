using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IFeedbackRepository
    {
        Task AddFeedbackAsync(TutorFeedback feedback);
        Task AddFeedbackDetailRangeAsync(IEnumerable<TutorFeedbackDetail> details);
        Task<bool> ExistsByBookingAsync(int bookingId, string learnerEmail);
        Task<int> CountCompletedSessionsAsync(int bookingId);
        Task<int> GetTotalSessionsAsync(int bookingId);
    }
}
