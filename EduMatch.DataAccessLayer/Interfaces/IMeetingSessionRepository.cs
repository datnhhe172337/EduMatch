using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IMeetingSessionRepository
    {
        Task<MeetingSession?> GetByIdAsync(int id);
        Task<MeetingSession?> GetByScheduleIdAsync(int scheduleId);
        Task CreateAsync(MeetingSession meetingSession);
        Task UpdateAsync(MeetingSession meetingSession);
        Task DeleteAsync(int id);
    }
}
