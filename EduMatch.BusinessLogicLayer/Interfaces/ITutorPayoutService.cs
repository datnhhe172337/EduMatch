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
    }
}
