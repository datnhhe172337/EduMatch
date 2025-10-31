using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class SystemFeeRepository : ISystemFeeRepository
    {
        private readonly EduMatchContext _context;
        public SystemFeeRepository(EduMatchContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SystemFee>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            return await _context.SystemFees
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.SystemFees.CountAsync();
        }

        public async Task<SystemFee?> GetByIdAsync(int id)
        {
            return await _context.SystemFees.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task CreateAsync(SystemFee entity)
        {
            await _context.SystemFees.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SystemFee entity)
        {
            _context.SystemFees.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.SystemFees.FirstOrDefaultAsync(x => x.Id == id);
            if (entity != null)
            {
                _context.SystemFees.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
