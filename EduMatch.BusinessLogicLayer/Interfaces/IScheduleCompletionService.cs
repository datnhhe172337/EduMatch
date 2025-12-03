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
    }
}
