using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{

    public interface IUserProfileRepository
    {
        Task CreateUserProfileAsync (UserProfile profile);
        Task<UserProfile?> GetByEmailAsync(string email);
        Task UpdateAsync(UserProfile profile);

        Task<IEnumerable<Province>> GetProvincesAsync();

        Task<IEnumerable<SubDistrict>> GetSubDistrictsByProvinceIdAsync(int provinceId);
    }

}
