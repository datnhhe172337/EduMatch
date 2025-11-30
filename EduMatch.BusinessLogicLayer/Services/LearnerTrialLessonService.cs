using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Interfaces;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class LearnerTrialLessonService : ILearnerTrialLessonService
    {
        private readonly ILearnerTrialLessonRepository _trialLessonRepository;

        public LearnerTrialLessonService(ILearnerTrialLessonRepository trialLessonRepository)
        {
            _trialLessonRepository = trialLessonRepository;
        }

        public async Task<bool> HasTrialedAsync(string learnerEmail, int tutorId, int subjectId)
        {
            return await _trialLessonRepository.ExistsAsync(learnerEmail, tutorId, subjectId);
        }

        public async Task<bool> RecordTrialAsync(string learnerEmail, int tutorId, int subjectId)
        {
            var exists = await _trialLessonRepository.ExistsAsync(learnerEmail, tutorId, subjectId);
            if (exists)
            {
                return false;
            }

            await _trialLessonRepository.AddAsync(learnerEmail, tutorId, subjectId);
            return true;
        }
    }
}
