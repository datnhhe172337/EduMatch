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
    public class WalletRepository : IWalletRepository
    {
        protected readonly EduMatchContext _context;
        protected readonly DbSet<Wallet> _dbSet;

        public WalletRepository(EduMatchContext context)
        {
            _context = context;
            _dbSet = context.Set<Wallet>();
        }

        public async Task<Wallet?> GetWalletByUserEmailAsync(string userEmail)
        {
            return await _dbSet
                .FirstOrDefaultAsync(w => w.UserEmail == userEmail);
        }

        public async Task AddAsync(Wallet entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(Wallet entity)
        {
            _dbSet.Update(entity);
        }
    }
}
