using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IScheduleRepository
    {
        /// <summary>
        /// Lấy danh sách Schedule theo BookingId và Status với phân trang
        /// </summary>
        Task<IEnumerable<Schedule>> GetAllByBookingIdAndStatusAsync(int bookingId, int? status, int page, int pageSize);
        /// <summary>
        /// Đếm số lượng Schedule theo BookingId và Status
        /// </summary>
        Task<int> CountByBookingIdAndStatusAsync(int bookingId, int? status);
        /// <summary>
        /// Lấy Schedule theo AvailabilityId
        /// </summary>
        Task<Schedule?> GetByAvailabilityIdAsync(int availabilitiId);
        /// <summary>
        /// Lấy Schedule theo ID
        /// </summary>
        Task<Schedule?> GetByIdAsync(int id);
        /// <summary>
        /// Lấy danh sách Schedule theo BookingId đã sắp xếp
        /// </summary>
        Task<IEnumerable<Schedule>> GetAllByBookingIdOrderedAsync(int bookingId);
        /// <summary>
        /// Tạo Schedule mới
        /// </summary>
        Task CreateAsync(Schedule schedule);
        /// <summary>
        /// Cập nhật Schedule
        /// </summary>
        Task UpdateAsync(Schedule schedule);
        /// <summary>
        /// Xóa Schedule theo ID
        /// </summary>
        Task DeleteAsync(int id);
    }
}
