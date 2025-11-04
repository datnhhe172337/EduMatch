using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class SystemFeeRepository : ISystemFeeRepository
    {
        protected readonly EduMatchContext _context;
        protected readonly DbSet<SystemFee> _dbSet;

        public SystemFeeRepository(EduMatchContext context)
        {
            _context = context;
            _dbSet = context.Set<SystemFee>();
        }

        public async Task<SystemFee?> GetCurrentActiveFeeAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(f => f.IsActive == true && f.EffectiveFrom <= now)
                .OrderByDescending(f => f.EffectiveFrom)
                .FirstOrDefaultAsync();
        }
    }
}