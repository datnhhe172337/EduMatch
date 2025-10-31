using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface ISystemFeeRepository
    {
        Task<IEnumerable<SystemFee>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<int> CountAsync();
        Task<SystemFee?> GetByIdAsync(int id);
        Task CreateAsync(SystemFee entity);
        Task UpdateAsync(SystemFee entity);
        Task DeleteAsync(int id);
    }
}
