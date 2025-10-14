using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.DataAccessLayer.Entities;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IManageTutorProfileService
    {
        Task<TutorProfile?> GetByEmailAsync(string email);
        Task<TutorProfile?> GetByIdAsync(int id);
        Task<bool> UpdateTutorProfileAsync(string email, UpdateTutorProfileDto dto);
    }
}
