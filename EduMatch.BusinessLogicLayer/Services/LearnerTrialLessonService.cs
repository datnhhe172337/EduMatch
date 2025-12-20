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
        private readonly IUserRepository _userRepository;
        private readonly ISubjectRepository _subjectRepository;

        public LearnerTrialLessonService(ILearnerTrialLessonRepository trialLessonRepository, ITutorSubjectRepository tutorSubjectRepository, IUserRepository userRepository, ISubjectRepository subjectRepository)
        {
            _trialLessonRepository = trialLessonRepository;
            _tutorSubjectRepository = tutorSubjectRepository;
            _userRepository = userRepository;
            _subjectRepository = subjectRepository;
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

        public async Task<bool> DeleteTrialAsync(string learnerEmail, int tutorId, int subjectId)
        {
            if (string.IsNullOrWhiteSpace(learnerEmail))
            {
                throw new ArgumentException("Learner email is required", nameof(learnerEmail));
            }

            // Kiểm tra learner tồn tại
            var learner = await _userRepository.GetUserByEmailAsync(learnerEmail);
            if (learner == null)
            {
                return false;
            }

            // Kiểm tra subject tồn tại
            var subject = await _subjectRepository.GetByIdAsync(subjectId);
            if (subject == null)
            {
                return false;
            }

            // Kiểm tra tutor và subject có tồn tại trong TutorSubject (tutor có dạy subject này không)
            var tutorSubjects = await _tutorSubjectRepository.GetByTutorIdAsync(tutorId);
            var tutorSubjectExists = tutorSubjects.Any(ts => ts.TutorId == tutorId && ts.SubjectId == subjectId);
            if (!tutorSubjectExists)
            {
                return false;
            }

            // Kiểm tra trial lesson có tồn tại không
            var trialExists = await _trialLessonRepository.ExistsAsync(learnerEmail, tutorId, subjectId);
            if (!trialExists)
            {
                return false;
            }

            return await _trialLessonRepository.DeleteAsync(learnerEmail, tutorId, subjectId);
        }
    }
}
