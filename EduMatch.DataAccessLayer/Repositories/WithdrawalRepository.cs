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
    public class WithdrawalRepository : IWithdrawalRepository
    {
        protected readonly EduMatchContext _context;
        protected readonly DbSet<Withdrawal> _dbSet;

        public WithdrawalRepository(EduMatchContext context)
        {
            _context = context;
            _dbSet = context.Set<Withdrawal>();
        }

        public async Task AddAsync(Withdrawal entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task<Withdrawal?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public void Update(Withdrawal entity)
        {
            _dbSet.Update(entity);
        }
        public async Task<IEnumerable<Withdrawal>> GetWithdrawalsByUserEmailAsync(string userEmail)
        {
            return await _dbSet
                .Include(w => w.Wallet)
                .Include(w => w.UserBankAccount)
                .ThenInclude(uba => uba.Bank)
                .Where(w => w.Wallet.UserEmail == userEmail)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Withdrawal>> GetPendingWithdrawalsAsync()
        {
            return await _dbSet
                .Include(w => w.Wallet) 
                .Include(w => w.UserBankAccount) 
                .ThenInclude(uba => uba.Bank)
                .Where(w => w.Status == Enum.WithdrawalStatus.Pending)
                .OrderBy(w => w.CreatedAt) 
                .ToListAsync();
        }
        public async Task<Withdrawal?> GetWithdrawalByIdAsync(int id)
        {
            return await _dbSet
                .Include(w => w.Wallet) // Include wallet
                .FirstOrDefaultAsync(w => w.Id == id);
        }
    }
}
