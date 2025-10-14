using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(string email, string password, string baseUrl);
        Task<IEnumerable<ManageUserDto>> GetUserByRoleAsync(int roleId);
        Task<bool> DeactivateUserAsync(string email);
        Task<bool> ActivateUserAsync(string email);
        Task<User> CreateAdminAccAsync(string email);
    }
}
