using System.Collections.Generic;
using EduMatch.BusinessLogicLayer.DTOs;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface ITutorPayoutService
    {
        /// <summary>
        /// Process and release tutor payouts that are ready and scheduled for today or earlier.
        /// Returns the number of payouts marked as Paid.
        /// </summary>
        Task<int> ProcessDuePayoutsAsync();

        /// <summary>
        /// Get all tutor payouts for a booking.
        /// </summary>
        Task<List<TutorPayoutDto>> GetByBookingIdAsync(int bookingId);
    }
}
