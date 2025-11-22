using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.RefundPolicy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IRefundPolicyService
    {
        /// <summary>
        /// Lấy tất cả RefundPolicy với lọc theo IsActive
        /// </summary>
        Task<List<RefundPolicyDto>> GetAllAsync(bool? isActive = null);

        /// <summary>
        /// Lấy RefundPolicy theo ID
        /// </summary>
        Task<RefundPolicyDto?> GetByIdAsync(int id);

        /// <summary>
        /// Tạo RefundPolicy mới
        /// </summary>
        Task<RefundPolicyDto> CreateAsync(RefundPolicyCreateRequest request);

        /// <summary>
        /// Cập nhật RefundPolicy
        /// </summary>
        Task<RefundPolicyDto> UpdateAsync(RefundPolicyUpdateRequest request);

        /// <summary>
        /// Cập nhật trạng thái IsActive của RefundPolicy
        /// </summary>
        Task<RefundPolicyDto> UpdateIsActiveAsync(int id, bool isActive);
    }
}

