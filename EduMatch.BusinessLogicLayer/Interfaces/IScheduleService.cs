using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.Schedule;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IScheduleService
    {
        /// <summary>
        /// Lấy danh sách Schedule theo bookingId và status với phân trang
        /// </summary>
        Task<List<ScheduleDto>> GetAllByBookingIdAndStatusAsync(int bookingId, ScheduleStatus? status, int page = 1, int pageSize = 10);
        /// <summary>
        /// Lấy danh sách Schedule theo bookingId và status (không phân trang)
        /// </summary>
        Task<List<ScheduleDto>> GetAllByBookingIdAndStatusNoPagingAsync(int bookingId, ScheduleStatus? status);
        /// <summary>
        /// Đếm tổng số Schedule theo bookingId và status
        /// </summary>
        Task<int> CountByBookingIdAndStatusAsync(int bookingId, ScheduleStatus? status);
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
        /// <summary>
        /// Lấy tất cả lịch học của Learner theo email (có thể lọc theo khoảng thời gian và Status)
        /// </summary>
        Task<List<ScheduleDto>> GetAllByLearnerEmailAsync(string learnerEmail, DateTime? startDate = null, DateTime? endDate = null, ScheduleStatus? status = null);
        /// <summary>
        /// Lấy tất cả lịch dạy của Tutor theo email (có thể lọc theo khoảng thời gian và Status)
        /// </summary>
        Task<List<ScheduleDto>> GetAllByTutorEmailAsync(string tutorEmail, DateTime? startDate = null, DateTime? endDate = null, ScheduleStatus? status = null);

        /// <summary>
        /// Lấy một số buổi dạy của Tutor theo email và status, sắp xếp theo thời gian tăng dần (mặc định lấy 1 buổi).
        /// Nếu không truyền status ở API thì mặc định là Upcoming.
        /// </summary>
        Task<List<ScheduleDto>> GetByTutorEmailAndStatusAsync(string tutorEmail, ScheduleStatus status, int bookingId, int take = 1);

        Task<ScheduleAttendanceSummaryDto> GetAttendanceSummaryByBookingAsync(int bookingId);
        /// <summary>
        /// Kiểm tra tutor có lịch học trùng với slot và ngày hay không (loại trừ Schedule bị Cancelled)
        /// </summary>
        Task<bool> HasTutorScheduleConflictAsync(int tutorId, int slotId, DateTime date);

        /// <summary>
        /// Cập nhật Status của Schedule (chỉ cho phép update tiến dần, ngoại lệ: Completed và Absent có thể update qua lại)
        /// </summary>
        Task<ScheduleDto> UpdateStatusAsync(int id, ScheduleStatus status);
    }
}
