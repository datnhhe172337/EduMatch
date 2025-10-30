using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.MeetingSession;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IMeetingSessionService
    {
        Task<MeetingSessionDto?> GetByIdAsync(int id);
        Task<List<MeetingSessionDto>> GetByScheduleIdAsync(int scheduleId);
        Task<MeetingSessionDto> CreateAsync(MeetingSessionCreateRequest request);
        Task<MeetingSessionDto> UpdateAsync(MeetingSessionUpdateRequest request);
        Task DeleteAsync(int id);
    }
}
