using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Repositories
{
    public class WalletTransactionRepository : IWalletTransactionRepository
    {
        protected readonly EduMatchContext _context;
        protected readonly DbSet<WalletTransaction> _dbSet;

        public WalletTransactionRepository(EduMatchContext context)
        {
            _context = context;
            _dbSet = context.Set<WalletTransaction>();
        }

        public async Task AddAsync(WalletTransaction entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task<IEnumerable<WalletTransaction>> GetTransactionsByWalletIdAsync(int walletId)
        {
            return await _dbSet
                .Include(t => t.Booking)
                .Where(t => t.WalletId == walletId)
                .OrderByDescending(t => t.CreatedAt) 
                .ToListAsync();
        }

        public async Task<WalletTransaction?> GetPendingWithdrawalTransactionAsync(int withdrawalId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.WithdrawalId == withdrawalId && t.Status == TransactionStatus.Pending);
        }

        public void Update(WalletTransaction entity)
        {
            _dbSet.Update(entity);
        }
    }
}
