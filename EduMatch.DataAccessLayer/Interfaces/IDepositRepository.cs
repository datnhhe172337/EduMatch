using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IDepositRepository
    {
        Task AddAsync(Deposit entity);
        Task<Deposit?> GetByIdAsync(int id);
        void Update(Deposit entity);

        Task<IEnumerable<Deposit>> FindAsync(Expression<Func<Deposit, bool>> predicate);
    }
}
