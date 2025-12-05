using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class ScheduleCompletionRepository : IScheduleCompletionRepository
    {
        private readonly EduMatchContext _context;
        private readonly DbSet<ScheduleCompletion> _dbSet;

        public ScheduleCompletionRepository(EduMatchContext context)
        {
            _context = context;
            _dbSet = context.Set<ScheduleCompletion>();
        }

        public async Task<ScheduleCompletion?> GetByScheduleIdAsync(int scheduleId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.ScheduleId == scheduleId);
        }

        public async Task<List<ScheduleCompletion>> GetPendingAutoCompleteAsync(DateTime cutoffUtc)
        {
            return await _dbSet
                .Where(x => x.Status == 0 && x.ConfirmationDeadline <= cutoffUtc) // 0 = PendingConfirm
                .ToListAsync();
        }

        public async Task AddAsync(ScheduleCompletion entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(ScheduleCompletion entity)
        {
            _dbSet.Update(entity);
        }
    }
}
