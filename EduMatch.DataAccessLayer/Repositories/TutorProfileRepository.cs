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
    public class TutorProfileRepository
    {
        private readonly EduMatchContext _context;

        public TutorProfileRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<TutorProfile?> GetByEmailAsync(string email)
        {
            return await _context.TutorProfiles.FirstOrDefaultAsync(t => t.UserEmail == email);
        }

        public async Task<TutorProfile?> GetByIdAsync(int id)
        {
            return await _context.TutorProfiles.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateAsync(TutorProfile profile)
        {
            _context.TutorProfiles.Update(profile);
            await _context.SaveChangesAsync();
        }
    }
}
