using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.ScheduleChangeRequest;
using EduMatch.DataAccessLayer.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IScheduleChangeRequestService
    {
        /// <summary>
        /// Lấy ScheduleChangeRequest theo ID
        /// </summary>
        Task<ScheduleChangeRequestDto?> GetByIdAsync(int id);

        /// <summary>
        /// Tạo ScheduleChangeRequest mới
        /// </summary>
        Task<ScheduleChangeRequestDto> CreateAsync(ScheduleChangeRequestCreateRequest request);

        /// <summary>
        /// Cập nhật ScheduleChangeRequest
        /// </summary>
        Task<ScheduleChangeRequestDto> UpdateAsync(ScheduleChangeRequestUpdateRequest request);

        /// <summary>
        /// Cập nhật Status của ScheduleChangeRequest
        /// </summary>
        Task<ScheduleChangeRequestDto> UpdateStatusAsync(int id, ScheduleChangeRequestStatus status);

        /// <summary>
        /// Lấy danh sách ScheduleChangeRequest theo RequesterEmail
        /// </summary>
        Task<List<ScheduleChangeRequestDto>> GetAllByRequesterEmailAsync(string requesterEmail, ScheduleChangeRequestStatus? status = null);

        /// <summary>
        /// Lấy danh sách ScheduleChangeRequest theo RequestedToEmail
        /// </summary>
        Task<List<ScheduleChangeRequestDto>> GetAllByRequestedToEmailAsync(string requestedToEmail, ScheduleChangeRequestStatus? status = null);

		/// <summary>
		/// Lấy danh sách ScheduleChangeRequest theo ScheduleId (có thể lọc theo status)
		/// </summary>
		Task<List<ScheduleChangeRequestDto>> GetAllByScheduleIdAsync(int scheduleId, ScheduleChangeRequestStatus? status = null);

        /// <summary>
        /// Tự động hủy các ScheduleChangeRequest Pending quá 3 ngày hoặc sắp đến giờ học
        /// </summary>
        Task<int> AutoCancelExpiredPendingRequestsAsync();
    }
}

