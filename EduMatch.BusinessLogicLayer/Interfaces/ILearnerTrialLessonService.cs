using System.Threading.Tasks;
using System.Collections.Generic;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface ILearnerTrialLessonService
    {
        Task<bool> HasTrialedAsync(string learnerEmail, int tutorId, int subjectId);

        Task<bool> RecordTrialAsync(string learnerEmail, int tutorId, int subjectId);

        Task<IReadOnlyList<DTOs.TrialLessonSubjectStatusDto>> GetSubjectTrialStatusesAsync(string learnerEmail, int tutorId);

        Task<bool> DeleteTrialAsync(string learnerEmail, int tutorId, int subjectId);
    }
}
