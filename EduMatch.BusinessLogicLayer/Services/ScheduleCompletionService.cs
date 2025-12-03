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
        private readonly ITutorPayoutService _tutorPayoutService;

        public ScheduleCompletionService(
            IScheduleCompletionRepository completionRepository,
            ITutorPayoutRepository payoutRepository,
            IUnitOfWork unitOfWork,
            ITutorPayoutService tutorPayoutService)
        {
            _completionRepository = completionRepository;
            _payoutRepository = payoutRepository;
            _unitOfWork = unitOfWork;
            _tutorPayoutService = tutorPayoutService;
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

        public async Task<bool> ConfirmAsync(int scheduleId, bool releasePayoutImmediately = true)
        {
            var completion = await _completionRepository.GetByScheduleIdAsync(scheduleId)
                ?? throw new InvalidOperationException("Schedule completion not found.");

            var currentStatus = (ScheduleCompletionStatus)completion.Status;
            if (currentStatus == ScheduleCompletionStatus.LearnerConfirmed)
                return false; // already confirmed

            if (currentStatus == ScheduleCompletionStatus.ReportedOnHold)
                throw new InvalidOperationException("Schedule is reported and cannot be confirmed until resolved.");

            var now = DateTime.UtcNow;
            completion.Status = (byte)ScheduleCompletionStatus.LearnerConfirmed;
            completion.ConfirmedAt = now;
            completion.UpdatedAt = now;
            _completionRepository.Update(completion);

            var payout = await _payoutRepository.GetByScheduleIdAsync(scheduleId);
            if (payout != null)
            {
                // Only move forward if not on hold/cancelled/paid
                var payoutStatus = (TutorPayoutStatus)payout.Status;
                if (payoutStatus == TutorPayoutStatus.OnHold)
                    throw new InvalidOperationException("Payout is on hold; resolve the report first.");

                if (payoutStatus == TutorPayoutStatus.Pending)
                {
                    payout.Status = (byte)TutorPayoutStatus.ReadyForPayout;
                    payout.PayoutTrigger = (byte)TutorPayoutTrigger.LearnerConfirmed;
                    payout.UpdatedAt = now;
                    _payoutRepository.Update(payout);
                }
            }

            await _unitOfWork.CompleteAsync();

            if (releasePayoutImmediately && payout != null && payout.Status == (byte)TutorPayoutStatus.ReadyForPayout)
            {
                await _tutorPayoutService.ProcessDuePayoutsAsync();
            }

            return true;
        }
    }
}
