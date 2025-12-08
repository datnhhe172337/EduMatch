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
        Task<IEnumerable<User>> GetAllUsers();
        Task<IEnumerable<User>> GetLearnerAsync();
        Task<IEnumerable<User>> GetTutorAsync();
        Task<IEnumerable<User>> GetAdminAsync();
        Task<bool> UpdateUserStatusAsync(string email, bool isActive);
        Task CreateAdminAccAsync(User user);
        Task CreateUserAsync(User user);
		Task UpdateUserAsync(User user);
		Task UpdateNameAndPhoneUserAsync(string name,string phone, string email);
        Task<bool> UpdateRoleUserAsync(string email, int roleId);
        Task CreateUserProfileAsync(UserProfile user);
        Task<Role> GetRoleByIdAsync(int roleId);

	}
}
