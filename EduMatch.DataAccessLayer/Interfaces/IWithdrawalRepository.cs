using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IWithdrawalRepository
    {
        Task AddAsync(Withdrawal entity);

        Task<Withdrawal?> GetByIdAsync(int id);
        void Update(Withdrawal entity);
        Task<IEnumerable<Withdrawal>> GetWithdrawalsByUserEmailAsync(string userEmail);

        Task<IEnumerable<Withdrawal>> GetPendingWithdrawalsAsync();
        Task<Withdrawal?> GetWithdrawalByIdAsync(int id);
        
    }
}
