using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class TutorPayoutRepository : ITutorPayoutRepository
    {
        private readonly EduMatchContext _context;
        private readonly DbSet<TutorPayout> _dbSet;

        public TutorPayoutRepository(EduMatchContext context)
        {
            _context = context;
            _dbSet = context.Set<TutorPayout>();
        }

        public async Task<List<TutorPayout>> GetReadyForPayoutAsync(DateOnly payoutDate)
        {
            return await _dbSet
                .Where(x => x.Status == 2 && x.ScheduledPayoutDate <= payoutDate) // 2 = ReadyForPayout
                .ToListAsync();
        }

        public async Task<TutorPayout?> GetByScheduleIdAsync(int scheduleId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.ScheduleId == scheduleId);
        }

        public async Task<List<TutorPayout>> GetByBookingIdAsync(int bookingId)
        {
            return await _dbSet
                .Where(x => x.BookingId == bookingId)
                .ToListAsync();
        }

        public async Task AddAsync(TutorPayout entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(TutorPayout entity)
        {
            _dbSet.Update(entity);
        }
    }
}
