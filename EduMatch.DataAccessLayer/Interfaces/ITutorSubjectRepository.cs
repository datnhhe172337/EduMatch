using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITutorSubjectRepository
	{
		Task<TutorSubject?> GetByIdFullAsync(int id);
		Task<TutorSubject?> GetByTutorIdFullAsync(int tutorId);
		Task<IReadOnlyList<TutorSubject>> GetByTutorIdAsync(int tutorId);
		Task<IReadOnlyList<TutorSubject>> GetBySubjectIdAsync(int subjectId);
		Task<IReadOnlyList<TutorSubject>> GetByLevelIdAsync(int levelId);
		Task<IReadOnlyList<TutorSubject>> GetByHourlyRateRangeAsync(decimal minRate, decimal maxRate);
		Task<IReadOnlyList<TutorSubject>> GetTutorsBySubjectAndLevelAsync(int subjectId, int levelId);
		Task<IReadOnlyList<TutorSubject>> GetAllFullAsync();
		Task AddAsync(TutorSubject entity);
		Task UpdateAsync(TutorSubject entity);
		Task RemoveByIdAsync(int id);
		Task RemoveByTutorIdAsync(int tutorId);
	}
}
