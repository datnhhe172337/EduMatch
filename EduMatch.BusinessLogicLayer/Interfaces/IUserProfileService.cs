using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfile?> GetByEmailAsync(string email);
        Task<bool> UpdateUserProfileAsync(string email, UpdateUserProfileDto dto);
    }
}
