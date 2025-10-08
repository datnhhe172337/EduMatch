using EduMatch.DataAccessLayer.Data;
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
    }
}
