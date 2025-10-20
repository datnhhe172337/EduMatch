using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TimeSlot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface ITimeSlotService
	{
		Task<TimeSlotDto?> GetByIdAsync(int id);
		Task<IReadOnlyList<TimeSlotDto>> GetAllAsync();
		Task<IReadOnlyList<TimeSlotDto>> GetByTimeRangeAsync(TimeOnly startTime, TimeOnly endTime);
		Task<TimeSlotDto?> GetByExactTimeAsync(TimeOnly startTime, TimeOnly endTime);
		Task<TimeSlotDto> CreateAsync(TimeSlotCreateRequest request);
		Task<TimeSlotDto> UpdateAsync(TimeSlotUpdateRequest request);
		Task DeleteAsync(int id);
	}
}
