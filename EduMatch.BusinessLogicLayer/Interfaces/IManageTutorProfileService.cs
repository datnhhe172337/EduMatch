using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.User;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IManageTutorProfileService
    {
        // --- FIXED: Return type is now TutorProfileDto? ---
        Task<TutorProfileDto?> GetByEmailAsync(string email);

        // --- FIXED: Renamed to GetByIdFullAsync and return type is TutorProfileDto? ---
        Task<TutorProfileDto?> GetByIdFullAsync(int id);

        // --- FIXED: tutorId is no longer nullable (int?) ---
        Task<TutorProfileDto> UpdateTutorProfileAsync(int tutorId, UpdateTutorProfileRequest request);
    }
}