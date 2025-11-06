using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ITimeSlotRepository
	{
		/// <summary>
		/// Lấy TimeSlot theo ID
		/// </summary>
		Task<TimeSlot?> GetByIdAsync(int id);
		/// <summary>
		/// Lấy tất cả TimeSlot
		/// </summary>
		Task<IReadOnlyList<TimeSlot>> GetAllAsync();
		/// <summary>
		/// Lấy TimeSlot theo khoảng thời gian
		/// </summary>
		Task<IReadOnlyList<TimeSlot>> GetByTimeRangeAsync(TimeOnly startTime, TimeOnly endTime);
		/// <summary>
		/// Lấy TimeSlot theo thời gian chính xác
		/// </summary>
		Task<TimeSlot?> GetByExactTimeAsync(TimeOnly startTime, TimeOnly endTime);
		/// <summary>
		/// Thêm TimeSlot mới
		/// </summary>
		Task AddAsync(TimeSlot entity);
		/// <summary>
		/// Cập nhật TimeSlot
		/// </summary>
		Task UpdateAsync(TimeSlot entity);
		/// <summary>
		/// Xóa TimeSlot theo ID
		/// </summary>
		Task RemoveByIdAsync(int id);
	}
}
