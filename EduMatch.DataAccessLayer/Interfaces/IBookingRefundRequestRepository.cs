using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IBookingRefundRequestRepository
    {
        /// <summary>
        /// Lấy tất cả BookingRefundRequest với lọc theo Status
        /// </summary>
        Task<IEnumerable<BookingRefundRequest>> GetAllAsync(int? status = null);

        /// <summary>
        /// Lấy BookingRefundRequest theo ID
        /// </summary>
        Task<BookingRefundRequest?> GetByIdAsync(int id);

        /// <summary>
        /// Lấy tất cả BookingRefundRequest theo LearnerEmail với lọc theo Status
        /// </summary>
        Task<IEnumerable<BookingRefundRequest>> GetAllByEmailAsync(string learnerEmail, int? status = null);

        /// <summary>
        /// Lấy tất cả BookingRefundRequest theo BookingId
        /// </summary>
        Task<IEnumerable<BookingRefundRequest>> GetAllByBookingIdAsync(int bookingId);

        /// <summary>
        /// Tạo BookingRefundRequest mới
        /// </summary>
        Task CreateAsync(BookingRefundRequest entity);

        /// <summary>
        /// Cập nhật BookingRefundRequest
        /// </summary>
        Task UpdateAsync(BookingRefundRequest entity);

        /// <summary>
        /// Xóa BookingRefundRequest theo ID
        /// </summary>
        Task DeleteAsync(int id);
    }
}

