using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.MeetingSession;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IMeetingSessionService
    {
        /// <summary>
        /// Lấy MeetingSession theo ID
        /// </summary>
        Task<MeetingSessionDto?> GetByIdAsync(int id);
        /// <summary>
        /// Lấy MeetingSession theo ScheduleId
        /// </summary>
        Task<MeetingSessionDto?> GetByScheduleIdAsync(int scheduleId);
        /// <summary>
        /// Tạo MeetingSession mới
        /// </summary>
        Task<MeetingSessionDto> CreateAsync(MeetingSessionCreateRequest request);
        /// <summary>
        /// Cập nhật MeetingSession
        /// </summary>
        Task<MeetingSessionDto> UpdateAsync(MeetingSessionUpdateRequest request);
        /// <summary>
        /// Xóa MeetingSession theo ID
        /// </summary>
        Task DeleteAsync(int id);
    }
}
