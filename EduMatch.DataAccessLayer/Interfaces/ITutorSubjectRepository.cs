using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITutorSubjectRepository
	{
		Task<TutorSubject?> GetByIdFullAsync(int id, CancellationToken ct = default);
		Task<TutorSubject?> GetByTutorIdFullAsync(int tutorId, CancellationToken ct = default);
		Task<IReadOnlyList<TutorSubject>> GetByTutorIdAsync(int tutorId, CancellationToken ct = default);
		Task<IReadOnlyList<TutorSubject>> GetBySubjectIdAsync(int subjectId, CancellationToken ct = default);
		Task<IReadOnlyList<TutorSubject>> GetByLevelIdAsync(int levelId, CancellationToken ct = default);
		Task<IReadOnlyList<TutorSubject>> GetByHourlyRateRangeAsync(decimal minRate, decimal maxRate, CancellationToken ct = default);
		Task<IReadOnlyList<TutorSubject>> GetTutorsBySubjectAndLevelAsync(int subjectId, int levelId, CancellationToken ct = default);
		Task<IReadOnlyList<TutorSubject>> GetAllFullAsync(CancellationToken ct = default);
		Task AddAsync(TutorSubject entity, CancellationToken ct = default);
		Task UpdateAsync(TutorSubject entity, CancellationToken ct = default);
		Task RemoveByIdAsync(int id, CancellationToken ct = default);
		Task RemoveByTutorIdAsync(int tutorId, CancellationToken ct = default);
	}
}
