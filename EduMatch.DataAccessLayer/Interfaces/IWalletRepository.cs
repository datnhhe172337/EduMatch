using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetWalletByUserEmailAsync(string userEmail);
        Task<Wallet?> GetByIdAsync(int id);

        Task AddAsync(Wallet entity);
        void Update(Wallet entity);
        Task<decimal> GetTotalLockedBalanceForRoleAsync(string roleName);
        Task<decimal> GetTotalAvailableBalanceAsync();
    }
}
