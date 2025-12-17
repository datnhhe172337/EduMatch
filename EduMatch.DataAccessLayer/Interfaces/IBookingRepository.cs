using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IBookingRepository
    {
        /// <summary>
        /// Lấy danh sách Booking theo learnerEmail với phân trang và lọc theo status, tutorSubjectId
        /// </summary>
        Task<IEnumerable<Booking>> GetAllByLearnerEmailAsync(string email, int? status, int? tutorSubjectId, int page, int pageSize);
        /// <summary>
        /// Đếm tổng số Booking theo learnerEmail với lọc theo status, tutorSubjectId
        /// </summary>
        Task<int> CountByLearnerEmailAsync(string email, int? status, int? tutorSubjectId);
        /// <summary>
        /// Lấy danh sách Booking theo tutorId với phân trang và lọc theo status, tutorSubjectId
        /// </summary>
        Task<IEnumerable<Booking>> GetAllByTutorIdAsync(int tutorId, int? status, int? tutorSubjectId, int page, int pageSize);
        /// <summary>
        /// Đếm tổng số Booking theo tutorId với lọc theo status, tutorSubjectId
        /// </summary>
        Task<int> CountByTutorIdAsync(int tutorId, int? status, int? tutorSubjectId);
        /// <summary>
        /// Lấy danh sách Booking theo learnerEmail (không phân trang) với lọc theo status, tutorSubjectId
        /// </summary>
        Task<IEnumerable<Booking>> GetAllByLearnerEmailNoPagingAsync(string email, int? status, int? tutorSubjectId);
        /// <summary>
        /// Lấy danh sách Booking theo tutorId (không phân trang) với lọc theo status, tutorSubjectId
        /// </summary>
        Task<IEnumerable<Booking>> GetAllByTutorIdNoPagingAsync(int tutorId, int? status, int? tutorSubjectId);
        /// <summary>
        /// Lấy Booking theo ID
        /// </summary>
        Task<Booking?> GetByIdAsync(int id);
        /// <summary>
        /// Tạo Booking mới
        /// </summary>
        Task CreateAsync(Booking booking);
        /// <summary>
        /// Cập nhật Booking
        /// </summary>
        Task UpdateAsync(Booking booking);
        /// <summary>
        /// Xóa Booking theo ID
        /// </summary>
        Task DeleteAsync(int id);
        /// <summary>
        /// Lấy danh sách Booking Pending cần auto-cancel dựa trên thời gian tạo và lịch học sắp diễn ra
        /// </summary>
        Task<List<Booking>> GetPendingBookingsNeedingAutoCancelAsync(DateTime createdBeforeUtc, DateTime scheduleStartBeforeUtc);
        /// <summary>
        /// Lấy danh sách Booking có status Confirmed
        /// </summary>
        Task<List<Booking>> GetConfirmedBookingsAsync();
    }
}
