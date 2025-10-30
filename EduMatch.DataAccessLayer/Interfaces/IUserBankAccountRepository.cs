using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IUserBankAccountRepository
    {
        Task<IEnumerable<UserBankAccount>> GetAccountsByUserEmailAsync(string userEmail);
        Task<UserBankAccount?> GetByIdAsync(int id);
        Task AddAsync(UserBankAccount entity);
        void Remove(UserBankAccount entity);
    }
}
