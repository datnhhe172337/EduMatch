// Filename: DepositRepository.cs
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

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
    }
}