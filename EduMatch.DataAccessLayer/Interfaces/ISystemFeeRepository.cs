using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface ISystemFeeRepository
    {
        /// <summary>
        /// Lấy danh sách SystemFee với phân trang
        /// </summary>
        Task<IEnumerable<SystemFee>> GetAllAsync(int page = 1, int pageSize = 10);
        /// <summary>
        /// Đếm tổng số SystemFee
        /// </summary>
        Task<int> CountAsync();
        /// <summary>
        /// Lấy SystemFee theo ID
        /// </summary>
        Task<SystemFee?> GetByIdAsync(int id);
        /// <summary>
        /// Tạo SystemFee mới
        /// </summary>
        Task CreateAsync(SystemFee entity);
        /// <summary>
        /// Cập nhật SystemFee
        /// </summary>
        Task UpdateAsync(SystemFee entity);
        /// <summary>
        /// Xóa SystemFee theo ID
        /// </summary>
        Task DeleteAsync(int id);
    }
}
