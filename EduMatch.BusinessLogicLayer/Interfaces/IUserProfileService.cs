using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.User;
using EduMatch.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfile?> GetByEmailAsync(string email);

        Task<UserProfileDto?> UpdateAsync(UserProfileUpdateRequest request);
		Task<UserProfileDto?> GetByEmailDatAsync(string email);

        Task<IEnumerable<ProvinceDto>> GetProvincesAsync();

        Task<IEnumerable<SubDistrictDto>> GetSubDistrictsByProvinceIdAsync(int provinceId);

    }
}
