using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class TutorRatingSummaryService : ITutorRatingSummaryService
    {
        private readonly ITutorFeedbackRepository _feedbackRepo;
        private readonly ITutorRatingSummaryRepository _summaryRepo;

        public TutorRatingSummaryService(
            ITutorFeedbackRepository feedbackRepo,
            ITutorRatingSummaryRepository summaryRepo)
        {
            _feedbackRepo = feedbackRepo;
            _summaryRepo = summaryRepo;
        }

        public async Task<TutorRatingSummary> AddRatingSummary(int tutorId)
        {
            var existing = await _summaryRepo.GetByTutorIdAsync(tutorId);
            if (existing != null)
                return existing;

            var summary = new TutorRatingSummary
            {
                TutorId = tutorId,
                AverageRating = 5.0,
                TotalFeedbackCount = 0,
                UpdatedAt = DateTime.UtcNow,
            };

            await _summaryRepo.AddAsync(summary);
            await _summaryRepo.SaveAsync();

            return summary;
        }

        public async Task<TutorRatingSummary> EnsureAndUpdateSummaryAsync(int tutorId)
        {
            // Lấy avg và count từ feedback
            double avg = await _feedbackRepo.GetTutorAvgRatingAsync(tutorId);
            int count = await _feedbackRepo.GetTutorFeedbackCountAsync(tutorId);

            var summary = await _summaryRepo.GetByTutorIdAsync(tutorId);

            if (summary == null)
            {
                summary = new TutorRatingSummary
                {
                    TutorId = tutorId,
                    AverageRating = avg,
                    TotalFeedbackCount = count,
                    UpdatedAt = DateTime.UtcNow
                };
                await _summaryRepo.AddAsync(summary);
            }
            else
            {
                summary.AverageRating = avg;
                summary.TotalFeedbackCount = count;
                summary.UpdatedAt = DateTime.UtcNow;
                await _summaryRepo.UpdateAsync(summary);
            }

            await _summaryRepo.SaveAsync();
            return summary;
        }

        public async Task<TutorRatingSummary?> GetByTutorIdAsync(int tutorId)
        {
            return await _summaryRepo.GetByTutorIdAsync(tutorId);
        }
    }

}
