using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface ITutorEducationService
    {
        Task<TutorEducationDto?> GetByIdFullAsync(int id);
        Task<TutorEducationDto?> GetByTutorIdFullAsync(int tutorId);
        Task<IReadOnlyList<TutorEducationDto>> GetByTutorIdAsync(int tutorId);
        Task<IReadOnlyList<TutorEducationDto>> GetByInstitutionIdAsync(int institutionId);
        Task<IReadOnlyList<TutorEducationDto>> GetByVerifiedStatusAsync(VerifyStatus verified);
        Task<IReadOnlyList<TutorEducationDto>> GetPendingVerificationsAsync();
        Task<IReadOnlyList<TutorEducationDto>> GetAllFullAsync();
        Task<TutorEducationDto> CreateAsync(TutorEducationCreateRequest request);
        Task<List<TutorEducationDto>> CreateBulkAsync(List<TutorEducationCreateRequest> requests);
        Task DeleteAsync(int id);
        Task DeleteByTutorIdAsync(int tutorId);
        Task<TutorEducationDto> UpdateAsync(TutorEducationUpdateRequest request);

        Task<TutorEducationDto> UpdateAsync(UpdateTutorEducationRequest request);
        Task ReconcileAsync(int tutorId, List<UpdateTutorEducationRequest> incomingEducations);
    }
}
