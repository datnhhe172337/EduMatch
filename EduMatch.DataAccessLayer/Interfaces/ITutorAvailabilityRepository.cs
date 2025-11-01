using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITutorAvailabilityRepository
	{
		Task<TutorAvailability?> GetByIdFullAsync(int id);
		Task<IReadOnlyList<TutorAvailability>> GetByTutorIdAsync(int tutorId);
		Task<IReadOnlyList<TutorAvailability>> GetByTutorIdFullAsync(int tutorId);
		Task<IReadOnlyList<TutorAvailability>> GetByStatusAsync(TutorAvailabilityStatus status);
		Task<IReadOnlyList<TutorAvailability>> GetAllFullAsync();

		Task AddAsync(TutorAvailability entity);
		Task AddRangeAsync(IEnumerable<TutorAvailability> entity);

		Task UpdateAsync(TutorAvailability entity);

		Task RemoveByIdAsync(int id);
	}
}
