using EduMatch.DataAccessLayer.Entities;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface ILearnerTrialLessonRepository
    {
        Task<LearnerTrialLesson> AddAsync(string learnerEmail, int tutorId, int subjectId);

        Task<bool> ExistsAsync(string learnerEmail, int tutorId, int subjectId);
    }
}
