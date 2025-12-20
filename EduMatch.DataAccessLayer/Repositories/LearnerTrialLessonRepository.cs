using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class LearnerTrialLessonRepository : ILearnerTrialLessonRepository
    {
        private readonly EduMatchContext _context;

        public LearnerTrialLessonRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<LearnerTrialLesson> AddAsync(string learnerEmail, int tutorId, int subjectId)
        {
            var existing = await _context.LearnerTrialLessons
                .FirstOrDefaultAsync(t => t.LearnerEmail == learnerEmail && t.TutorId == tutorId && t.SubjectId == subjectId);

            if (existing != null)
            {
                return existing;
            }

            var trial = new LearnerTrialLesson
            {
                LearnerEmail = learnerEmail,
                TutorId = tutorId,
                SubjectId = subjectId,
                CreatedAt = DateTime.UtcNow
            };

            _context.LearnerTrialLessons.Add(trial);
            await _context.SaveChangesAsync();
            return trial;
        }

        public async Task<bool> ExistsAsync(string learnerEmail, int tutorId, int subjectId)
        {
            return await _context.LearnerTrialLessons
                .AnyAsync(t => t.LearnerEmail == learnerEmail && t.TutorId == tutorId && t.SubjectId == subjectId);
        }

        public async Task<IReadOnlyList<LearnerTrialLesson>> GetByLearnerAndTutorAsync(string learnerEmail, int tutorId)
        {
            return await _context.LearnerTrialLessons
                .Where(t => t.LearnerEmail == learnerEmail && t.TutorId == tutorId)
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync(string learnerEmail, int tutorId, int subjectId)
        {
            var trial = await _context.LearnerTrialLessons
                .FirstOrDefaultAsync(t => t.LearnerEmail == learnerEmail && t.TutorId == tutorId && t.SubjectId == subjectId);

            if (trial == null)
            {
                return false;
            }

            _context.LearnerTrialLessons.Remove(trial);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
