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
    }
}
