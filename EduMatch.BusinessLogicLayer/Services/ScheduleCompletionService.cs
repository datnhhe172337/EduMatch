using System;
using System.Threading.Tasks;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class ScheduleCompletionService : IScheduleCompletionService
    {
        private readonly IScheduleCompletionRepository _completionRepository;
        private readonly ITutorPayoutRepository _payoutRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ScheduleCompletionService(
            IScheduleCompletionRepository completionRepository,
            ITutorPayoutRepository payoutRepository,
            IUnitOfWork unitOfWork)
        {
            _completionRepository = completionRepository;
            _payoutRepository = payoutRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> AutoCompletePastDueAsync()
        {
            var now = DateTime.UtcNow;
            var dueCompletions = await _completionRepository.GetPendingAutoCompleteAsync(now);
            if (dueCompletions.Count == 0)
                return 0;

            var updated = 0;
            foreach (var completion in dueCompletions)
            {
                completion.Status = (byte)ScheduleCompletionStatus.AutoCompleted;
                completion.AutoCompletedAt = now;
                completion.UpdatedAt = now;
                _completionRepository.Update(completion);

                // If a payout exists and is still Pending, move it to ReadyForPayout
                var payout = await _payoutRepository.GetByScheduleIdAsync(completion.ScheduleId);
                if (payout != null && payout.Status == (byte)TutorPayoutStatus.Pending)
                {
                    payout.Status = (byte)TutorPayoutStatus.ReadyForPayout;
                    payout.PayoutTrigger = (byte)TutorPayoutTrigger.AutoCompleted;
                    payout.UpdatedAt = now;
                    _payoutRepository.Update(payout);
                }

                updated++;
            }

            await _unitOfWork.CompleteAsync();
            return updated;
        }
    }
}
