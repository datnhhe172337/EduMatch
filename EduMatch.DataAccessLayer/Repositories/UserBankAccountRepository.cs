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
    public class UserBankAccountRepository : IUserBankAccountRepository
    {
        protected readonly EduMatchContext _context;
        protected readonly DbSet<UserBankAccount> _dbSet;

        public UserBankAccountRepository(EduMatchContext context)
        {
            _context = context;
            _dbSet = context.Set<UserBankAccount>();
        }

        public async Task<IEnumerable<UserBankAccount>> GetAccountsByUserEmailAsync(string userEmail)
        {
            return await _dbSet
                .Where(acc => acc.UserEmail == userEmail)
                .Include(acc => acc.Bank)
                .OrderBy(acc => acc.Bank.Name)
                .ToListAsync();
        }

        public async Task<UserBankAccount?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task AddAsync(UserBankAccount entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Remove(UserBankAccount entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
