using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.Schedule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IScheduleService
    {
        /// <summary>
        /// Lấy danh sách Schedule theo bookingId và status với phân trang
        /// </summary>
        Task<List<ScheduleDto>> GetAllByBookingIdAndStatusAsync(int bookingId, int? status, int page = 1, int pageSize = 10);
        /// <summary>
        /// Lấy danh sách Schedule theo bookingId và status (không phân trang)
        /// </summary>
        Task<List<ScheduleDto>> GetAllByBookingIdAndStatusNoPagingAsync(int bookingId, int? status);
        /// <summary>
        /// Đếm tổng số Schedule theo bookingId và status
        /// </summary>
        Task<int> CountByBookingIdAndStatusAsync(int bookingId, int? status);
        /// <summary>
        /// Lấy Schedule theo AvailabilityId
        /// </summary>
        Task<ScheduleDto?> GetByAvailabilityIdAsync(int availabilitiId);
        /// <summary>
        /// Lấy Schedule theo ID
        /// </summary>
        Task<ScheduleDto?> GetByIdAsync(int id);
        /// <summary>
        /// Tạo Schedule mới
        /// </summary>
        Task<ScheduleDto> CreateAsync(ScheduleCreateRequest request);
        /// <summary>
        /// Tạo danh sách Schedule cho một Booking; tổng số Schedule sau khi tạo phải bằng TotalSessions của Booking
        /// </summary>
        Task<List<ScheduleDto>> CreateListAsync(List<ScheduleCreateRequest> requests);
        /// <summary>
        /// Cập nhật Schedule
        /// </summary>
        Task<ScheduleDto> UpdateAsync(ScheduleUpdateRequest request);
        /// <summary>
        /// Xóa Schedule theo ID
        /// </summary>
        Task DeleteAsync(int id);
        /// <summary>
        /// Hủy toàn bộ Schedule theo bookingId
        /// </summary>
        Task<List<ScheduleDto>> CancelAllByBookingAsync(int bookingId);
    }
}
