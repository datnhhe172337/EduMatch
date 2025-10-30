using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IScheduleRepository
    {
        Task<IEnumerable<Schedule>> GetAllByBookingIdAndStatusAsync(int bookingId, int? status, int page, int pageSize);
        Task<int> CountByBookingIdAndStatusAsync(int bookingId, int? status);
        Task<IEnumerable<Schedule>> GetByAvailabilityIdAsync(int availabilitiId, int page, int pageSize);
        Task<int> CountByAvailabilityIdAsync(int availabilitiId);
        Task<Schedule?> GetByIdAsync(int id);
        Task CreateAsync(Schedule schedule);
        Task UpdateAsync(Schedule schedule);
    }
}
