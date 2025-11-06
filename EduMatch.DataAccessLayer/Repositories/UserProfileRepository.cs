using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
	{

        private readonly EduMatchContext _context;

        public UserProfileRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task CreateUserProfileAsync(UserProfile profile)
        {
            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }

        public async Task<UserProfile?> GetByEmailAsync(string email)
        {
            return await _context.UserProfiles
                .Include(up => up.UserEmailNavigation)                
                    .ThenInclude(u => u.FavoriteTutors)             
                        .ThenInclude(ft => ft.Tutor)                 
                .FirstOrDefaultAsync(up => up.UserEmail == email);
        }

        public async Task<IEnumerable<Province>> GetProvincesAsync()
        {
            return await _context.Provinces.ToListAsync();
        }

        public async Task<IEnumerable<SubDistrict>> GetSubDistrictsByProvinceIdAsync(int provinceId)
        {
            return await _context.SubDistricts
                .Include(x => x.Province)
                .Where(u =>  u.ProvinceId == provinceId)
                .ToListAsync();
        }

        public async Task UpdateAsync(UserProfile profile)
        {
            //_context.UserProfiles.Update(profile);
            //await _context.SaveChangesAsync();
            await _context.SaveChangesAsync();
        }
        public async Task UpdateUserProfileAndUserAsync(UserProfile profile, User user)
        {
            _context.UserProfiles.Update(profile);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }


    }
}
