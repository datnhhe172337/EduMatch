using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TutorAvailability;
using EduMatch.DataAccessLayer.Enum;
using System;
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
    }
}