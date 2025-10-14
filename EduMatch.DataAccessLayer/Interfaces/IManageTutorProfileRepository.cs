using EduMatch.DataAccessLayer.Entities;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IManageTutorProfileRepository
    {
        Task<TutorProfile?> GetByEmailAsync(string email);
        Task<TutorProfile?> GetByIdAsync(int id);
        Task<bool> UpdateTutorProfileAsync(string email, TutorProfile updatedProfile, UserProfile updatedUserProfile);
    }
}
