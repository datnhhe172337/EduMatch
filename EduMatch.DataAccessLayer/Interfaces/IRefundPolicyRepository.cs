using EduMatch.DataAccessLayer.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.DataAccessLayer.Interfaces
{
    public interface IRefundPolicyRepository
    {
        /// <summary>
        /// Lấy tất cả RefundPolicy với lọc theo IsActive
        /// </summary>
        Task<IEnumerable<RefundPolicy>> GetAllAsync(bool? isActive = null);

        /// <summary>
        /// Lấy RefundPolicy theo ID
        /// </summary>
        Task<RefundPolicy?> GetByIdAsync(int id);

        /// <summary>
        /// Tạo RefundPolicy mới
        /// </summary>
        Task CreateAsync(RefundPolicy entity);

        /// <summary>
        /// Cập nhật RefundPolicy
        /// </summary>
        Task UpdateAsync(RefundPolicy entity);

        /// <summary>
        /// Xóa RefundPolicy theo ID
        /// </summary>
        Task DeleteAsync(int id);
    }
}

