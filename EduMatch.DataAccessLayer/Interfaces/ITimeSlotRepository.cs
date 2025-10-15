using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITimeSlotRepository
	{
		Task<TimeSlot?> GetByIdAsync(int id, CancellationToken ct = default);
		Task<IReadOnlyList<TimeSlot>> GetAllAsync(CancellationToken ct = default);
		Task<IReadOnlyList<TimeSlot>> GetByTimeRangeAsync(TimeOnly startTime, TimeOnly endTime, CancellationToken ct = default);
		Task<TimeSlot?> GetByExactTimeAsync(TimeOnly startTime, TimeOnly endTime, CancellationToken ct = default);
		Task AddAsync(TimeSlot entity, CancellationToken ct = default);
		Task UpdateAsync(TimeSlot entity, CancellationToken ct = default);
		Task RemoveByIdAsync(int id, CancellationToken ct = default);
	}
}
