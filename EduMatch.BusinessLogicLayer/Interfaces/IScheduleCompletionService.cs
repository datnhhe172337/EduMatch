using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IScheduleCompletionService
    {
        /// <summary>
        /// Auto-complete pending schedule confirmations that passed the confirmation deadline.
        /// Returns number of completions updated.
        /// </summary>
        Task<int> AutoCompletePastDueAsync();

        /// <summary>
        /// Learner confirms a schedule as finished; optionally triggers immediate payout release.
        /// Returns true if the confirmation updated the record, false if it was already confirmed.
        /// </summary>
        Task<bool> ConfirmAsync(int scheduleId, bool releasePayoutImmediately = true);
    }
}
