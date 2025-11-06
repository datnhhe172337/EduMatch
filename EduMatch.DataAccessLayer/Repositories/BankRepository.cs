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
    public class BankRepository : IBankRepository
    {
        protected readonly EduMatchContext _context; 
        protected readonly DbSet<Bank> _dbSet;

        public BankRepository(EduMatchContext context)
        {
            _context = context;
            _dbSet = context.Set<Bank>();
        }

        public async Task<IEnumerable<Bank>> GetAllBanksAsync()
        {           
            return await _dbSet.OrderBy(b => b.Name).ToListAsync();
        }
    }
}
