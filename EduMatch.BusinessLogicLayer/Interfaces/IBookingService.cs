using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.Booking;
using EduMatch.DataAccessLayer.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IBookingService
    {
        /// <summary>
        /// Lấy danh sách Booking theo learnerEmail với phân trang và lọc theo status, tutorSubjectId
        /// </summary>
        Task<List<BookingDto>> GetAllByLearnerEmailAsync(string email, BookingStatus? status, int? tutorSubjectId, int page = 1, int pageSize = 10);
        /// <summary>
        /// Lấy danh sách Booking theo learnerEmail (không phân trang)
        /// </summary>
        Task<List<BookingDto>> GetAllByLearnerEmailNoPagingAsync(string email, BookingStatus? status, int? tutorSubjectId);
        /// <summary>
        /// Đếm tổng số Booking theo learnerEmail với lọc theo status, tutorSubjectId
        /// </summary>
        Task<int> CountByLearnerEmailAsync(string email, BookingStatus? status, int? tutorSubjectId);
        /// <summary>
        /// Lấy danh sách Booking theo tutorId với phân trang và lọc theo status, tutorSubjectId
        /// </summary>
        Task<List<BookingDto>> GetAllByTutorIdAsync(int tutorId, BookingStatus? status, int? tutorSubjectId, int page = 1, int pageSize = 10);
        /// <summary>
        /// Lấy danh sách Booking theo tutorId (không phân trang)
        /// </summary>
        Task<List<BookingDto>> GetAllByTutorIdNoPagingAsync(int tutorId, BookingStatus? status, int? tutorSubjectId);
        /// <summary>
        /// Đếm tổng số Booking theo tutorId với lọc theo status, tutorSubjectId
        /// </summary>
        Task<int> CountByTutorIdAsync(int tutorId, BookingStatus? status, int? tutorSubjectId);
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
        /// <summary>
        /// Cập nhật PaymentStatus của Booking
        /// </summary>
        Task<BookingDto> UpdatePaymentStatusAsync(int id, PaymentStatus paymentStatus);
        /// <summary>
        /// Thanh toán booking: khóa tiền từ ví học viên.
        /// </summary>
        Task<BookingDto> PayForBookingAsync(int bookingId, string learnerEmail);
        /// <summary>
        /// Cập nhật Status của Booking
        /// </summary>
        Task<BookingDto> UpdateStatusAsync(int id, BookingStatus status);
        /// <summary>
        /// Hủy booking do học viên yêu cầu và hoàn trả số tiền còn lại (không trừ phí).
        /// </summary>
        Task<BookingDto> CancelByLearnerAsync(int bookingId, string learnerEmail);
        /// <summary>
        /// Xem trước thông tin hủy booking: số buổi chưa học và số tiền dự kiến hoàn lại.
        /// </summary>
        Task<BookingCancelPreviewDto> GetCancelPreviewAsync(int bookingId);
        /// <summary>
        /// Hoàn tiền booking với tỷ lệ phần trăm dành cho học viên.
        /// </summary>
        Task<BookingDto> RefundBookingAsync(int bookingId, decimal learnerPercentage);
        /// <summary>
        /// Tự động hủy các booking Pending quá hạn xác nhận
        /// </summary>
        Task<int> AutoCancelUnconfirmedBookingsAsync();
        /// <summary>
        /// Tự động hoàn thành các booking Confirmed khi tất cả schedule đều không còn Upcoming
        /// </summary>
        Task<int> AutoCompleteConfirmedBookingsAsync();
    }
}
