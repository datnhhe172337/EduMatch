using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITutorAvailabilityRepository
	{
		Task<TutorAvailability?> GetByIdFullAsync(int id, CancellationToken ct = default);
		Task<IReadOnlyList<TutorAvailability>> GetByTutorIdAsync(int tutorId, CancellationToken ct = default);
		Task<IReadOnlyList<TutorAvailability>> GetByStatusAsync(TutorAvailabilityStatus status, CancellationToken ct = default);
		Task<IReadOnlyList<TutorAvailability>> GetAllFullAsync(CancellationToken ct = default);

		Task AddAsync(TutorAvailability entity, CancellationToken ct = default);
		Task AddRangeAsync(IEnumerable<TutorAvailability> entity, CancellationToken ct = default);

		Task UpdateAsync(TutorAvailability entity, CancellationToken ct = default);

		Task RemoveByIdAsync(int id, CancellationToken ct = default);
	}
}
