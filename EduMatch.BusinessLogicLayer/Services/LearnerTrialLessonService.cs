using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.DataAccessLayer.Interfaces;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class LearnerTrialLessonService : ILearnerTrialLessonService
    {
        private readonly ILearnerTrialLessonRepository _trialLessonRepository;
        private readonly ITutorSubjectRepository _tutorSubjectRepository;

        public LearnerTrialLessonService(ILearnerTrialLessonRepository trialLessonRepository, ITutorSubjectRepository tutorSubjectRepository)
        {
            _trialLessonRepository = trialLessonRepository;
            _tutorSubjectRepository = tutorSubjectRepository;
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

        public async Task<IReadOnlyList<TrialLessonSubjectStatusDto>> GetSubjectTrialStatusesAsync(string learnerEmail, int tutorId)
        {
            if (string.IsNullOrWhiteSpace(learnerEmail))
            {
                throw new ArgumentException("Learner email is required", nameof(learnerEmail));
            }

            var tutorSubjects = await _tutorSubjectRepository.GetByTutorIdAsync(tutorId);
            var trials = await _trialLessonRepository.GetByLearnerAndTutorAsync(learnerEmail, tutorId);
            var trialSubjectIds = trials.Select(t => t.SubjectId).ToHashSet();

            var result = tutorSubjects
                .Select(ts => new TrialLessonSubjectStatusDto
                {
                    SubjectId = ts.SubjectId,
                    SubjectName = ts.Subject?.SubjectName ?? string.Empty,
                    HasTrialed = trialSubjectIds.Contains(ts.SubjectId)
                })
                .ToList();

            return result;
        }
    }
}
