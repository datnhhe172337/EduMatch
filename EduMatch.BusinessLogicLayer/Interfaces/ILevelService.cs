using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Requests.Level;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface ILevelService
	{
		/// <summary>
		/// Lấy Level theo ID
		/// </summary>
		Task<LevelDto?> GetByIdAsync(int id);
		/// <summary>
		/// Lấy tất cả Level
		/// </summary>
		Task<IReadOnlyList<LevelDto>> GetAllAsync();
		/// <summary>
		/// Tìm Level theo tên
		/// </summary>
		Task<IReadOnlyList<LevelDto>> GetByNameAsync(string name);
		/// <summary>
		/// Tạo Level mới
		/// </summary>
		Task<LevelDto> CreateAsync(LevelCreateRequest request);
		/// <summary>
		/// Cập nhật Level
		/// </summary>
		Task<LevelDto> UpdateAsync(LevelUpdateRequest request);
		/// <summary>
		/// Xóa Level theo ID
		/// </summary>
		Task DeleteAsync(int id);
	}
}
