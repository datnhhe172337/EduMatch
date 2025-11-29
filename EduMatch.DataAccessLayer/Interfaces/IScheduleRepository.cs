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
        /// Lấy danh sách Schedule theo BookingId và Status (không phân trang)
        /// </summary>
        Task<IEnumerable<Schedule>> GetAllByBookingIdAndStatusNoPagingAsync(int bookingId, int? status);
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
        /// Lấy danh sách Schedule theo LearnerEmail (qua Booking) và optional khoảng thời gian (qua TutorAvailability.StartDate) và Status
        /// </summary>
        Task<IEnumerable<Schedule>> GetAllByLearnerEmailAsync(string learnerEmail, DateTime? startDate = null, DateTime? endDate = null, int? status = null);
        /// <summary>
        /// Lấy danh sách Schedule theo TutorEmail (qua TutorProfile -> TutorSubject -> Booking) và optional khoảng thời gian và Status
        /// </summary>
        Task<IEnumerable<Schedule>> GetAllByTutorEmailAsync(string tutorEmail, DateTime? startDate = null, DateTime? endDate = null, int? status = null);
        /// <summary>
        /// Kiểm tra tutor có lịch học trùng với slot và ngày hay không (loại trừ Schedule bị Cancelled)
        /// </summary>
        Task<bool> HasTutorScheduleOnSlotDateAsync(int tutorId, int slotId, DateTime date);
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
        /// <summary>
        /// Lấy tất cả Schedule có status Upcoming hoặc InProgress với đầy đủ thông tin Availability và Slot
        /// </summary>
        Task<IEnumerable<Schedule>> GetAllUpcomingAndInProgressAsync();
    }
}
