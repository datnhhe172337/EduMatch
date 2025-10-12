using EduMatch.DataAccessLayer.Data;
using EduMatch.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class UserProfileRepository
    {
        private readonly EduMatchContext _context;

        public UserProfileRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<UserProfile?> GetByEmailAsync(string email)
        {
            return await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserEmail == email);
        }

        public async Task UpdateAsync(UserProfile profile)
        {
            _context.UserProfiles.Update(profile);
            await _context.SaveChangesAsync();
        }
    }
}
