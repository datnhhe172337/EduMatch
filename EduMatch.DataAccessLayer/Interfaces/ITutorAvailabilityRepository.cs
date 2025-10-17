using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITutorAvailabilityRepository
	{
		Task<TutorAvailability?> GetByIdFullAsync(int id, CancellationToken ct = default);
		Task<TutorAvailability?> GetByTutorIdFullAsync(int tutorId, CancellationToken ct = default);
		Task<IReadOnlyList<TutorAvailability>> GetByTutorIdAsync(int tutorId, CancellationToken ct = default);
		//Task<IReadOnlyList<TutorAvailability>> GetByDayOfWeekAsync(DayOfWeek dayOfWeek, CancellationToken ct = default);
		//Task<IReadOnlyList<TutorAvailability>> GetByTimeSlotAsync(int slotId, CancellationToken ct = default);
		//Task<IReadOnlyList<TutorAvailability>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default);
		Task<IReadOnlyList<TutorAvailability>> GetAllFullAsync(CancellationToken ct = default);
		Task AddAsync(TutorAvailability entity, CancellationToken ct = default);
		Task AddRangeAsync(IEnumerable<TutorAvailability> entity, CancellationToken ct = default);
		Task UpdateAsync(TutorAvailability entity, CancellationToken ct = default);
		Task RemoveByIdAsync(int id, CancellationToken ct = default);
		Task RemoveByTutorIdAsync(int tutorId, CancellationToken ct = default);
	}
}
