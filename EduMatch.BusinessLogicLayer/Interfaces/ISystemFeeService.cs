using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.SystemFee;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface ISystemFeeService
    {
        /// <summary>
        /// Lấy danh sách SystemFee với phân trang
        /// </summary>
        Task<List<SystemFeeDto>> GetAllAsync(int page = 1, int pageSize = 10);
        /// <summary>
        /// Lấy tất cả SystemFee (không phân trang)
        /// </summary>
        Task<List<SystemFeeDto>> GetAllNoPagingAsync();
        /// <summary>
        /// Đếm tổng số SystemFee
        /// </summary>
        Task<int> CountAsync();
        /// <summary>
        /// Lấy SystemFee theo ID
        /// </summary>
        Task<SystemFeeDto?> GetByIdAsync(int id);
        /// <summary>
        /// Tạo SystemFee mới
        /// </summary>
        Task<SystemFeeDto> CreateAsync(SystemFeeCreateRequest request);
        /// <summary>
        /// Cập nhật SystemFee
        /// </summary>
        Task<SystemFeeDto> UpdateAsync(SystemFeeUpdateRequest request);
        /// <summary>
        /// Xóa SystemFee theo ID
        /// </summary>
        Task DeleteAsync(int id);
        /// <summary>
        /// Lấy SystemFee đang hoạt động (IsActive = true), lấy giá trị đầu tiên
        /// </summary>
        Task<SystemFeeDto?> GetActiveSystemFeeAsync();
    }
}
