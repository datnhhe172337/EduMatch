using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IScheduleRepository
    {
        Task<IEnumerable<Schedule>> GetAllByBookingIdAndStatusAsync(int bookingId, int? status, int page, int pageSize);
        Task<int> CountByBookingIdAndStatusAsync(int bookingId, int? status);
        Task<Schedule?> GetByAvailabilityIdAsync(int availabilitiId);
        Task<Schedule?> GetByIdAsync(int id);
        Task<IEnumerable<Schedule>> GetAllByBookingIdOrderedAsync(int bookingId);
        Task CreateAsync(Schedule schedule);
        Task UpdateAsync(Schedule schedule);
        Task DeleteAsync(int id);
    }
}
