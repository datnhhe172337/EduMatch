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
        private readonly EduMatchContext _context;

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
                            .Include(u => u.UserProfile)
                            .SingleOrDefaultAsync(u => u.Email == email);
        }


        public async Task<IEnumerable<User>> GetLearnerAsync()
        {
            return await _context.Users
                .Where(u => u.RoleId == 1)
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetTutorAsync()
        {
            return await _context.Users
                .Where(u => u.RoleId == 2)
                .Include(u => u.Role)
                .Include(u => u.TutorProfile)
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

        public async Task CreateAdminAccAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        public async Task CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task CreateUserProfileAsync(UserProfile user)
        {
            _context.UserProfiles.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
		{
			_context.Users.Update(user);
			await _context.SaveChangesAsync();
		}

		public async Task<IEnumerable<User>> GetAdminAsync()
        {
            return await _context.Users
                .Where(u => u.RoleId == 3)
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserProfile)
                .Include(u => u.TutorProfile)
                .OrderByDescending(u => u.TutorProfile != null
                ? u.TutorProfile.CreatedAt
                : u.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateRoleUserAsync(string email, int roleId)
        {
            var user = await _context.Users.FindAsync(email);
            if (user == null) return false;
            user.RoleId = roleId;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

		public async Task UpdateNameAndPhoneUserAsync(string name, string phone, string email)
		{

			var user = await GetUserByEmailAsync( email);
            if (user != null)
            {
                user.UserName = name;
                user.Phone = phone;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
			}
		}

		public async Task<Role> GetRoleByIdAsync(int roleId)
		{
		   var role = await _context.Roles.SingleOrDefaultAsync(r => r.Id == roleId);

            if (role == null)
            {
                throw new Exception("Role not found");
			}
            return role;
		}
	}
}
