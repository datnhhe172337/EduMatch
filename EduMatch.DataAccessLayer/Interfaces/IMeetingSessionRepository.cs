using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IMeetingSessionRepository
    {
        /// <summary>
        /// Lấy MeetingSession theo ID
        /// </summary>
        Task<MeetingSession?> GetByIdAsync(int id);
        /// <summary>
        /// Lấy MeetingSession theo ScheduleId
        /// </summary>
        Task<MeetingSession?> GetByScheduleIdAsync(int scheduleId);
        /// <summary>
        /// Tạo MeetingSession mới
        /// </summary>
        Task CreateAsync(MeetingSession meetingSession);
        /// <summary>
        /// Cập nhật MeetingSession
        /// </summary>
        Task UpdateAsync(MeetingSession meetingSession);
        /// <summary>
        /// Xóa MeetingSession theo ID
        /// </summary>
        Task DeleteAsync(int id);
    }
}
