using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);

        Task<bool> IsEmailAvailableAsync(string email);
        Task<IEnumerable<User>> GetLearnerAsync();
        Task<IEnumerable<User>> GetTutorAsync();
        Task<bool> UpdateUserStatusAsync(string email, bool isActive);
        Task<User> CreateAdminAccAsync(User user);
    }
}
