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
        Task<bool> ConfirmAsync(int scheduleId, bool releasePayoutImmediately = true, string? currentUserEmail = null, bool adminAction = false);

        /// <summary>
        /// Wrapper to confirm and pay in one call; leaves existing methods untouched.
        /// </summary>
        Task<bool> FinishAndPayAsync(int scheduleId, string? currentUserEmail = null, bool adminAction = false);

        /// <summary>
        /// Mark a schedule as reported/on-hold and tie it to a report.
        /// </summary>
        Task<bool> MarkReportedAsync(int scheduleId, int reportId, string? currentUserEmail = null);

        /// <summary>
        /// Admin resolution: release payout (back to ReadyForPayout) or cancel payout after review.
        /// </summary>
        Task<bool> ResolveReportAsync(int scheduleId, bool releaseToTutor);

        /// <summary>
        /// Learner cancels the schedule completion (no payout will be released).
        /// </summary>
        Task<bool> CancelAsync(int scheduleId, string? currentUserEmail = null, bool adminAction = false);
    }
}
