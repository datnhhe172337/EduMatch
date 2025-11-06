using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TimeSlot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface ITimeSlotService
	{
		/// <summary>
		/// Lấy TimeSlot theo ID
		/// </summary>
		Task<TimeSlotDto?> GetByIdAsync(int id);
		/// <summary>
		/// Lấy tất cả TimeSlot
		/// </summary>
		Task<IReadOnlyList<TimeSlotDto>> GetAllAsync();
		/// <summary>
		/// Lấy TimeSlot theo khoảng thời gian
		/// </summary>
		Task<IReadOnlyList<TimeSlotDto>> GetByTimeRangeAsync(TimeOnly startTime, TimeOnly endTime);
		/// <summary>
		/// Lấy TimeSlot theo thời gian chính xác
		/// </summary>
		Task<TimeSlotDto?> GetByExactTimeAsync(TimeOnly startTime, TimeOnly endTime);
		/// <summary>
		/// Tạo TimeSlot mới
		/// </summary>
		Task<TimeSlotDto> CreateAsync(TimeSlotCreateRequest request);
		/// <summary>
		/// Cập nhật TimeSlot
		/// </summary>
		Task<TimeSlotDto> UpdateAsync(TimeSlotUpdateRequest request);
		/// <summary>
		/// Xóa TimeSlot theo ID
		/// </summary>
		Task DeleteAsync(int id);
	}
}
