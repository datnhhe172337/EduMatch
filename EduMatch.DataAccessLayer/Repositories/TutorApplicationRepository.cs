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
    public class TutorApplicationRepository : ITutorApplicationRepository
    {
        private readonly EduMatchContext _context;

        public TutorApplicationRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<bool> HasAppliedAsync(int classRequestId, int tutorId)
        {
            return await _context.TutorApplications
                .AnyAsync(x => x.ClassRequestId == classRequestId && x.TutorId == tutorId);
        }

        public async Task AddApplicationAsync(TutorApplication app)
        {
            _context.TutorApplications.Add(app);
            await _context.SaveChangesAsync();
        }

        public async Task<ClassRequest?> GetClassRequestByIdAsync(int classRequestId)
        {
            return await _context.ClassRequests.FirstOrDefaultAsync(x => x.Id == classRequestId);
        }

        public async Task<TutorProfile?> GetTutorByEmailAsync(string userEmail)
        {
            return await _context.TutorProfiles
                    .FirstOrDefaultAsync(t => t.UserEmail == userEmail);
        }

        public async Task<List<TutorApplication?>> GetApplicationsByClassRequestAsync(int classRequestId)
        {
            return await _context.TutorApplications
                .Include(a => a.Tutor)
                    .ThenInclude(t => t.UserEmailNavigation)
                        .ThenInclude(u => u.UserProfile)
                .Where(a => a.ClassRequestId == classRequestId && a.Status == 0)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TutorApplication?>> GetApplicationsByTutorAsync(string tutorEmail)
        {
            return await _context.TutorApplications
                .Include(a => a.ClassRequest)
                    .ThenInclude(cr => cr.LearnerEmailNavigation)
                        .ThenInclude(u => u.UserProfile)
                .Include(a => a.ClassRequest.Subject)
                .Include(a => a.ClassRequest.Level)
                .Where(a => a.Tutor.UserEmail == tutorEmail && a.Status == 0)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TutorApplication?>> GetCanceledApplicationsByTutorAsync(string tutorEmail)
        {
            return await _context.TutorApplications
                .Include(a => a.ClassRequest)
                    .ThenInclude(cr => cr.LearnerEmailNavigation)
                        .ThenInclude(u => u.UserProfile)
                .Include(a => a.ClassRequest.Subject)
                .Include(a => a.ClassRequest.Level)
                .Where(a => a.Tutor.UserEmail == tutorEmail && a.Status == 1)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<TutorApplication?> GetApplicationByIdAsync(int id)
        {
            return await _context.TutorApplications
                .Include(a => a.Tutor)
                .Include(t => t.ClassRequest)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task UpdateAsync(TutorApplication app)
        {
            _context.TutorApplications.Update(app);
            await _context.SaveChangesAsync();
        }


    }
}
