using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.Booking;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IBookingService
    {
        /// <summary>
        /// Lấy danh sách Booking theo learnerEmail với phân trang và lọc theo status, tutorSubjectId
        /// </summary>
        Task<List<BookingDto>> GetAllByLearnerEmailAsync(string email, int? status, int? tutorSubjectId, int page = 1, int pageSize = 10);
        /// <summary>
        /// Lấy danh sách Booking theo learnerEmail (không phân trang)
        /// </summary>
        Task<List<BookingDto>> GetAllByLearnerEmailNoPagingAsync(string email, int? status, int? tutorSubjectId);
        /// <summary>
        /// Đếm tổng số Booking theo learnerEmail với lọc theo status, tutorSubjectId
        /// </summary>
        Task<int> CountByLearnerEmailAsync(string email, int? status, int? tutorSubjectId);
        /// <summary>
        /// Lấy danh sách Booking theo tutorId với phân trang và lọc theo status, tutorSubjectId
        /// </summary>
        Task<List<BookingDto>> GetAllByTutorIdAsync(int tutorId, int? status, int? tutorSubjectId, int page = 1, int pageSize = 10);
        /// <summary>
        /// Lấy danh sách Booking theo tutorId (không phân trang)
        /// </summary>
        Task<List<BookingDto>> GetAllByTutorIdNoPagingAsync(int tutorId, int? status, int? tutorSubjectId);
        /// <summary>
        /// Đếm tổng số Booking theo tutorId với lọc theo status, tutorSubjectId
        /// </summary>
        Task<int> CountByTutorIdAsync(int tutorId, int? status, int? tutorSubjectId);
        /// <summary>
        /// Lấy Booking theo ID
        /// </summary>
        Task<BookingDto?> GetByIdAsync(int id);
        /// <summary>
        /// Tạo Booking mới
        /// </summary>
        Task<BookingDto> CreateAsync(BookingCreateRequest request);
        /// <summary>
        /// Cập nhật Booking
        /// </summary>
        Task<BookingDto> UpdateAsync(BookingUpdateRequest request);
        /// <summary>
        /// Xóa Booking theo ID
        /// </summary>
        Task DeleteAsync(int id);
    }
}
