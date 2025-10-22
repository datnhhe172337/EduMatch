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
    public class FavoriteTutorRepository : IFavoriteTutorRepository
    {
        private readonly EduMatchContext _context;

        public FavoriteTutorRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<bool> AddFavoriteAsync(string userEmail, int tutorId)
        {
            var existing = await _context.FavoriteTutors
                .FirstOrDefaultAsync(f => f.UserEmail == userEmail && f.TutorId == tutorId);

            if (existing != null)
                return false;

            var favorite = new FavoriteTutor
            {
                UserEmail = userEmail,
                TutorId = tutorId,
                CreatedAt = DateTime.Now
            };

            _context.FavoriteTutors.Add(favorite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFavoriteAsync(string userEmail, int tutorId)
        {
            var favorite = await _context.FavoriteTutors
                .FirstOrDefaultAsync(f => f.UserEmail == userEmail && f.TutorId == tutorId);

            if (favorite == null)
                return false;

            _context.FavoriteTutors.Remove(favorite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFavoriteAsync(string userEmail, int tutorId)
        {
            return await _context.FavoriteTutors
                .AnyAsync(f => f.UserEmail == userEmail && f.TutorId == tutorId);
        }

        public async Task<List<TutorProfile>> GetFavoriteTutorsAsync(string userEmail)
        {
            return await _context.FavoriteTutors
                .Where(f => f.UserEmail == userEmail)
                .Include(f => f.Tutor)
                .ThenInclude(tp => tp.UserEmailNavigation)
                .Select(f => f.Tutor)
                .ToListAsync();
        }
    }
}
