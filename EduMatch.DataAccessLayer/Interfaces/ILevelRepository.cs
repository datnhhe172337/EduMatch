using EduMatch.DataAccessLayer.Entities;

namespace EduMatch.DataAccessLayer.Interfaces
{
	public interface ILevelRepository
	{
		/// <summary>
		/// Lấy Level theo ID
		/// </summary>
		Task<Level?> GetByIdAsync(int id);
		/// <summary>
		/// Lấy tất cả Level
		/// </summary>
		Task<IReadOnlyList<Level>> GetAllAsync();
		/// <summary>
		/// Tìm Level theo tên
		/// </summary>
		Task<IReadOnlyList<Level>> GetByNameAsync(string name);
		/// <summary>
		/// Thêm Level mới
		/// </summary>
		Task AddAsync(Level entity);
		/// <summary>
		/// Cập nhật Level
		/// </summary>
		Task UpdateAsync(Level entity);
		/// <summary>
		/// Xóa Level theo ID
		/// </summary>
		Task RemoveByIdAsync(int id);
	}
}
