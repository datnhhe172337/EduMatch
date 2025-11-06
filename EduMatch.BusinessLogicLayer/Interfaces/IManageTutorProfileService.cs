using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.TutorProfile;
using EduMatch.BusinessLogicLayer.Requests.User;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IManageTutorProfileService
    {

        Task<TutorProfileDto?> GetByEmailAsync(string email);


        Task<TutorProfileDto?> GetByIdFullAsync(int id);


		Task<TutorProfileDto> UpdateTutorProfileAsync(int tutorId, TutorProfileUpdateRequest request);
    }
}