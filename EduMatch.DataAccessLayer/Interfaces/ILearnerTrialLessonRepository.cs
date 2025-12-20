using EduMatch.DataAccessLayer.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface ILearnerTrialLessonRepository
    {
        Task<LearnerTrialLesson> AddAsync(string learnerEmail, int tutorId, int subjectId);

        Task<bool> ExistsAsync(string learnerEmail, int tutorId, int subjectId);

        Task<IReadOnlyList<LearnerTrialLesson>> GetByLearnerAndTutorAsync(string learnerEmail, int tutorId);

        Task<bool> DeleteAsync(string learnerEmail, int tutorId, int subjectId);
    }
}
