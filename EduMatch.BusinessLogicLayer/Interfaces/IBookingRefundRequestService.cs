using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.BookingRefundRequest;
using EduMatch.DataAccessLayer.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IBookingRefundRequestService
    {
        /// <summary>
        /// Lấy tất cả BookingRefundRequest với lọc theo Status
        /// </summary>
        Task<List<BookingRefundRequestDto>> GetAllAsync(BookingRefundRequestStatus? status = null);

        /// <summary>
        /// Lấy BookingRefundRequest theo ID
        /// </summary>
        Task<BookingRefundRequestDto?> GetByIdAsync(int id);

        /// <summary>
        /// Lấy tất cả BookingRefundRequest theo LearnerEmail với lọc theo Status
        /// </summary>
        Task<List<BookingRefundRequestDto>> GetAllByEmailAsync(string learnerEmail, BookingRefundRequestStatus? status = null);

        /// <summary>
        /// Tạo BookingRefundRequest mới
        /// </summary>
        Task<BookingRefundRequestDto> CreateAsync(BookingRefundRequestCreateRequest request);

        /// <summary>
        /// Cập nhật BookingRefundRequest
        /// </summary>
        Task<BookingRefundRequestDto> UpdateAsync(BookingRefundRequestUpdateRequest request);

        /// <summary>
        /// Cập nhật Status của BookingRefundRequest
        /// </summary>
        Task<BookingRefundRequestDto> UpdateStatusAsync(int id, BookingRefundRequestStatus status);
    }
}

