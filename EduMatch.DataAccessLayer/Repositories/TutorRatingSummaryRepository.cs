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
    public class TutorRatingSummaryRepository : ITutorRatingSummaryRepository
    {
        private readonly EduMatchContext _context;

        public TutorRatingSummaryRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<TutorRatingSummary?> GetByTutorIdAsync(int tutorId)
        {
            return await _context.TutorRatingSummaries
                .FirstOrDefaultAsync(x => x.TutorId == tutorId);
        }

        public async Task AddAsync(TutorRatingSummary summary)
        {
            await _context.TutorRatingSummaries.AddAsync(summary);
        }

        public async Task UpdateAsync(TutorRatingSummary summary)
        {
            _context.TutorRatingSummaries.Update(summary);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }

}
