// Filename: DepositRepository.cs
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class DepositRepository : IDepositRepository
    {
        protected readonly EduMatchContext _context;
        protected readonly DbSet<Deposit> _dbSet;

        public DepositRepository(EduMatchContext context)
        {
            _context = context;
            _dbSet = context.Set<Deposit>();
        }

        public async Task AddAsync(Deposit entity)
        {
            await _dbSet.AddAsync(entity);
        }
        public async Task<Deposit?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(d => d.Wallet)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public void Update(Deposit entity)
        {
            _dbSet.Update(entity);
        }

        public async Task<IEnumerable<Deposit>> FindAsync(Expression<Func<Deposit, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
    }
}