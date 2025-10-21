using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITimeSlotRepository
	{
		Task<TimeSlot?> GetByIdAsync(int id);
		Task<IReadOnlyList<TimeSlot>> GetAllAsync();
		Task<IReadOnlyList<TimeSlot>> GetByTimeRangeAsync(TimeOnly startTime, TimeOnly endTime);
		Task<TimeSlot?> GetByExactTimeAsync(TimeOnly startTime, TimeOnly endTime);
		Task AddAsync(TimeSlot entity);
		Task UpdateAsync(TimeSlot entity);
		Task RemoveByIdAsync(int id);
	}
}
