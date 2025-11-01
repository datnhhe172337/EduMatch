using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.SystemFee;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface ISystemFeeService
    {
        Task<List<SystemFeeDto>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<int> CountAsync();
        Task<SystemFeeDto?> GetByIdAsync(int id);
        Task<SystemFeeDto> CreateAsync(SystemFeeCreateRequest request);
        Task<SystemFeeDto> UpdateAsync(SystemFeeUpdateRequest request);
        Task DeleteAsync(int id);
    }
}
