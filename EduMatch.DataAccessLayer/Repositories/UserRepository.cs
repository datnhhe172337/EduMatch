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
    public class UserRepository : IUserRepository
    {
        private readonly EduMatchContext  _context;

        public UserRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);

        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.Include(u => u.Role)
                            .SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetLearnerAsync()
        {
            return await _context.Users
                .Where(u => u.RoleId == 1)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetTutorAsync()
        {
            return await _context.Users
                .Where(u => u.RoleId == 2)
                .Include(u => u.TutorProfile)
                .ThenInclude(tp => tp.TutorSubjects)
                .ThenInclude(ts => ts.Subject)
                .OrderByDescending(u => u.TutorProfile != null
                ? u.TutorProfile.CreatedAt
                : u.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateUserStatusAsync(string email, bool isActive)
        {
            var user = await _context.Users.FindAsync(email);
            if (user == null) return false;

            user.IsActive = isActive;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User> CreateAdminAccAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
