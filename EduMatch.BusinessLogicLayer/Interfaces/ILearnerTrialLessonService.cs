using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface ILearnerTrialLessonService
    {
        Task<bool> HasTrialedAsync(string learnerEmail, int tutorId, int subjectId);

        Task<bool> RecordTrialAsync(string learnerEmail, int tutorId, int subjectId);
    }
}
