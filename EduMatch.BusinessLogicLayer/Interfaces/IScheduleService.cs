using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.Schedule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IScheduleService
    {
        Task<List<ScheduleDto>> GetAllByBookingIdAndStatusAsync(int bookingId, int? status, int page = 1, int pageSize = 10);
        Task<int> CountByBookingIdAndStatusAsync(int bookingId, int? status);
        Task<ScheduleDto?> GetByAvailabilityIdAsync(int availabilitiId);
        Task<ScheduleDto?> GetByIdAsync(int id);
        Task<ScheduleDto> CreateAsync(ScheduleCreateRequest request);
        Task<ScheduleDto> UpdateAsync(ScheduleUpdateRequest request);
        Task DeleteAsync(int id);
    }
}
