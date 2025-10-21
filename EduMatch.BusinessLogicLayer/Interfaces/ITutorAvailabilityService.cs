using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface ITutorAvailabilityService
    {
        Task<TutorAvailabilityDto?> GetByIdFullAsync(int id);
        Task<IReadOnlyList<TutorAvailabilityDto>> GetByTutorIdAsync(int tutorId);
        Task<IReadOnlyList<TutorAvailabilityDto>> GetAllFullAsync();
        Task<TutorAvailabilityDto> CreateAsync(TutorAvailabilityCreateRequest request);
        Task<List<TutorAvailabilityDto>> CreateBulkAsync(List<TutorAvailabilityCreateRequest> requests);
        Task DeleteAsync(int id);
        Task<TutorAvailabilityDto> UpdateAsync(TutorAvailabilityUpdateRequest request);

        // --- NEW/UPDATED METHODS ---
        Task<TutorAvailabilityDto> UpdateAsync(UpdateTutorAvailabilityRequest request);
        Task ReconcileAsync(int tutorId, List<UpdateTutorAvailabilityRequest> incomingAvailabilities);
    }
}