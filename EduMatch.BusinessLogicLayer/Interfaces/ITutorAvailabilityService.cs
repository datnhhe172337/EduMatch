using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Enum;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface ITutorAvailabilityService
	{
		Task<TutorAvailabilityDto?> GetByIdFullAsync(int id);
		Task<TutorAvailabilityDto?> GetByTutorIdFullAsync(int tutorId);
		Task<IReadOnlyList<TutorAvailabilityDto>> GetByTutorIdAsync(int tutorId);
		Task<IReadOnlyList<TutorAvailabilityDto>> GetByDayOfWeekAsync(DayOfWeek dayOfWeek);
		Task<IReadOnlyList<TutorAvailabilityDto>> GetByTimeSlotAsync(int slotId);
		Task<IReadOnlyList<TutorAvailabilityDto>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
		Task<IReadOnlyList<TutorAvailabilityDto>> GetAllFullAsync();
		Task<TutorAvailabilityDto> CreateAsync(TutorAvailabilityCreateRequest request);
		Task<TutorAvailabilityDto> UpdateAsync(TutorAvailabilityUpdateRequest request);
		Task DeleteAsync(int id);
		Task DeleteByTutorIdAsync(int tutorId);
	}
}
